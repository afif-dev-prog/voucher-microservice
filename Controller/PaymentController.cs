using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using voucherMicroservice.Data;
using voucherMicroservice.Model;
using voucherMicroservice.Services;

namespace voucherMicroservice.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly DataContext _db;
        private readonly IWebPushService webPushService;
        private readonly IConfiguration configuration;
        private readonly IEmailService emailService;

        public PaymentController(DataContext db, IWebPushService webPushService, IConfiguration configuration, IEmailService emailService)
        {
            this._db = db;
            this.webPushService = webPushService;
            this.configuration = configuration;
            this.emailService = emailService;
        }
        // Student subscribes to push notifications
        [HttpPost("/api/voucher/payment/subscribe")]
        // [Authorize]
        public async Task<IActionResult> Subscribe([FromBody] PushSubscriptionRequest req)
        {
            var studentId = req?.StudentId;

            if (string.IsNullOrEmpty(studentId))
                return Unauthorized();

            // Remove old subscriptions for this student
            var existing = await _db.push_subscription
                .Where(s => s.student_id == studentId)
                .ToListAsync();
            _db.push_subscription.RemoveRange(existing);

            var sub = new StudentPushSubscription
            {
                student_id = studentId,
                endpoint = req.Endpoint,
                p256dh = req.P256dh,
                auth = req.Auth,
                created_at = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };

            await _db.push_subscription.AddAsync(sub);
            await _db.SaveChangesAsync();

            return Ok(new { success = true, message = "Subscribed to push notifications." });
        }
        [HttpGet("/api/voucher/payment/debug-subscriptions/{userId}")]
        public async Task<IActionResult> DebugSubscriptions(string userId)
        {
            var subs = await _db.push_subscription
                .Where(s => s.student_id == userId)
                .Select(s => new { s.student_id, s.endpoint, s.created_at })
                .ToListAsync();

            return Ok(new { count = subs.Count, subs });
        }
        // Get VAPID public key for frontend
        [HttpGet("/api/voucher/payment/vapid-public-key")]
        public IActionResult GetVapidPublicKey()
        {
            return Ok(new { publicKey = configuration["VapidKeys:PublicKey"] });
        }

        // Seller initiates payment request
        [HttpPost("/api/voucher/payment/initiate")]
        // [Authorize]
        public async Task<IActionResult> InitiatePayment([FromBody] InitiatePaymentRequest req)
        {
            // Debug — check what's coming in
            if (req == null)
                return BadRequest(new { success = false, message = "Request body is null." });

            if (string.IsNullOrEmpty(req.SellerUsername))
                return BadRequest(new { success = false, message = "SellerUsername is empty." });

            if (string.IsNullOrEmpty(req.StudentId))
                return BadRequest(new { success = false, message = "StudentId is empty." });

            if (req.Amount <= 0)
                return BadRequest(new { success = false, message = "Amount must be greater than 0." });

            var seller = await _db.seller
                .FirstOrDefaultAsync(s => s.username == req.SellerUsername);

            if (seller == null)
                return NotFound(new { success = false, message = $"Seller '{req.SellerUsername}' not found." });

            var sellerName = seller.s_name ?? req.SellerUsername;

            var student = await _db.student
                .FirstOrDefaultAsync(s => s.student_id == req.StudentId);

            if (student == null)
                return NotFound(new { success = false, message = $"Student '{req.StudentId}' not found." });

            if (student.balance < req.Amount)
                return BadRequest(new { success = false, message = "Insufficient balance." });

            var existing = await _db.pending_payments
                .Where(p => p.student_id == req.StudentId && p.status == "pending")
                .ToListAsync();

            foreach (var p in existing)
            {
                p.status = "expired";
                p.resolved_at = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            }

            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var pending = new PendingPayment
            {
                student_id = req.StudentId,
                seller_username = req.SellerUsername,
                seller_name = sellerName,
                amount = req.Amount,
                status = "pending",
                created_at = now,
                expires_at = now + 60
            };

            await _db.pending_payments.AddAsync(pending);
            await _db.SaveChangesAsync();

            // Check if webPushService is registered
            if (webPushService != null)
            {
                await webPushService.SendPaymentApprovalRequest(
                    req.StudentId, sellerName, req.Amount, pending.id);
            }

            return Ok(new
            {
                success = true,
                paymentId = pending.id,
                message = "Payment request sent to student."
            });
        }

        // Check payment status (seller polls this)
        [HttpGet("/api/voucher/payment/status/{paymentId}")]
        // [Authorize]
        public async Task<IActionResult> GetStatus(string paymentId)
        {
            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var payment = await _db.pending_payments
                .FirstOrDefaultAsync(p => p.id == paymentId);

            if (payment == null)
                return NotFound(new { success = false, message = "Payment not found." });

            // Auto-expire
            if (payment.status == "pending" && now > payment.expires_at)
            {
                payment.status = "expired";
                payment.resolved_at = now;
                await _db.SaveChangesAsync();
            }

            return Ok(new
            {
                success = true,
                data = new
                {
                    payment.id,
                    payment.status,
                    payment.amount,
                    payment.seller_name,
                    payment.student_id,
                    expires_at = payment.expires_at,
                    seconds_remaining = Math.Max(0, payment.expires_at - now)
                }
            });
        }

        // Student approves payment
        [HttpPost("/api/voucher/payment/approve/{paymentId}")]
        // [Authorize]
        public async Task<IActionResult> Approve([FromBody] string studentId, string paymentId)
        {
            var student_id = studentId;

            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var payment = await _db.pending_payments
                .FirstOrDefaultAsync(p => p.id == paymentId && p.student_id == student_id);

            if (payment == null)
                return NotFound(new { success = false, message = "Payment not found." });

            if (payment.status != "pending")
                return BadRequest(new { success = false, message = $"Payment already {payment.status}." });

            if (now > payment.expires_at)
            {
                payment.status = "expired";
                await _db.SaveChangesAsync();
                return BadRequest(new { success = false, message = "Payment request expired." });
            }

            using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                var student = await _db.student
                    .FirstOrDefaultAsync(s => s.student_id == student_id);

                if (student == null || student.balance < payment.amount)
                {
                    await transaction.RollbackAsync();
                    return BadRequest(new { success = false, message = "Insufficient balance." });
                }

                var groupId = Guid.NewGuid().ToString();

                // Deduct student balance
                await _db.student
                    .Where(s => s.student_id == student_id)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(s => s.balance, s => s.balance - payment.amount)
                        .SetProperty(s => s.date_update, s => (int)now));

                // Add seller balance
                await _db.seller
                    .Where(s => s.username == payment.seller_username)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(s => s.balance, s => s.balance + payment.amount)
                        .SetProperty(s => s.date_update, s => (int)now));

                // Record transaction
                var history = new PayHistory
                {
                    transaction_id = groupId,
                    student_id = student_id,
                    seller = payment.seller_name,
                    debit = payment.amount,
                    credit = 0,
                    remark = $"QRP - Payment to {payment.seller_name}",
                    pay_date = (int)now,
                    user_update = student.student_name,
                    month_credit = string.Empty
                };

                await _db.payhistory.AddAsync(history);

                payment.status = "approved";
                payment.resolved_at = now;

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                // Notify student — balance deducted


                // if (!string.IsNullOrEmpty(student.email))
                // {
                //     await emailService.SendEmailAsync(
                //         student.email,
                //         student.student_name ?? studentId,
                //         "Payment Confirmation",
                //         $"Your payment of RM {payment.amount:F2} to {payment.seller_name} was successful.\n\n" +
                //         $"Remaining balance: RM {student.balance:F2}\n\n" +
                //         $"Date: {DateTime.Now:dd MMM yyyy, HH:mm}"
                //     );
                // }
                await webPushService.SendToUser(
                                    studentId,
                                    "Payment Successful",
                                    $"RM {payment.amount:F2} paid to {payment.seller_name}. Check your balance.",
                                    "PAYMENT_DEDUCTED",
                                    payment.id
                                );

                // // Notify seller — money received
                await webPushService.SendToUser(
                    payment.seller_username,
                    "Payment Received",
                    $"RM {payment.amount:F2} received from student {studentId}.",
                    "PAYMENT_RECEIVED",
                    payment.id
                );
                return Ok(new
                {
                    success = true,
                    message = "Payment approved successfully.",
                    data = new { balance = student.balance - payment.amount }
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { success = false, message = ex.Message });
            }


        }

        // Student rejects payment
        [HttpPost("/api/voucher/payment/reject/{paymentId}")]
        // [Authorize]
        public async Task<IActionResult> Reject([FromBody] string studentId, string paymentId)
        {
            var student_id = studentId;

            var payment = await _db.pending_payments
                .FirstOrDefaultAsync(p => p.id == paymentId && p.student_id == student_id);

            if (payment == null)
                return NotFound(new { success = false, message = "Payment not found." });

            if (payment.status != "pending")
                return BadRequest(new { success = false, message = $"Payment already {payment.status}." });

            payment.status = "rejected";
            payment.resolved_at = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            await _db.SaveChangesAsync();

            return Ok(new { success = true, message = "Payment rejected." });
        }

        // Student polls for pending payments (fallback for iOS)
        [HttpGet("/api/voucher/payment/pending")]
        [Authorize]
        public async Task<IActionResult> GetPendingPayments(string studentId)
        {
            var student_id = studentId;

            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            // Auto-expire old ones
            await _db.pending_payments
                .Where(p => p.student_id == studentId &&
                            p.status == "pending" &&
                            p.expires_at < now)
                .ExecuteUpdateAsync(p => p
                    .SetProperty(p => p.status, "expired")
                    .SetProperty(p => p.resolved_at, now));

            var pending = await _db.pending_payments
                .Where(p => p.student_id == studentId && p.status == "pending")
                .OrderByDescending(p => p.created_at)
                .FirstOrDefaultAsync();

            return Ok(new
            {
                success = true,
                data = pending == null ? null : new
                {
                    pending.id,
                    pending.seller_name,
                    pending.amount,
                    pending.status,
                    seconds_remaining = Math.Max(0, pending.expires_at - now)
                }
            });
        }
    }
}