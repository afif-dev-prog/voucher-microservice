using System;
using System.Collections.Generic;
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
    public class AnnouncementController : ControllerBase
    {
        private readonly DataContext _db;
        private readonly IWebPushService _pushService;
        private readonly IEmailService _emailService;

        public AnnouncementController(
            DataContext db,
            IWebPushService pushService,
            IEmailService emailService)
        {
            _db = db;
            _pushService = pushService;
            _emailService = emailService;
        }

        // ── Superadmin sends announcement ─────
        [HttpPost("/api/voucher/announcements/send")]
        [Authorize]
        public async Task<IActionResult> Send([FromBody] AnnouncementRequest req)
        {
            var createdBy = User.FindFirst("sub")?.Value ?? "admin";
            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            var announcement = new Announcement
            {
                title = req.Title,
                message = req.Message,
                target = req.Target,
                send_email = req.SendEmail,
                send_push = req.SendPush,
                created_by = createdBy,
                created_at = now
            };

            await _db.announcements.AddAsync(announcement);
            await _db.SaveChangesAsync();

            // Send push notification
            if (req.SendPush)
            {
                // await _pushService.SendToGroup(
                //     req.Target, req.Title, req.Message, "ANNOUNCEMENT");
            }

            // Send email
            if (req.SendEmail)
            {
                // await _emailService.SendEmailAsync(req);
            }

            return Ok(new
            {
                success = true,
                message = $"Announcement sent to {req.Target}.",
                data = new { announcement.id }
            });
        }

        // private async Task SendAnnouncementEmails(AnnouncementRequest req)
        // {
        //     List<string> emails = new();

        //     if (req.Target == "ALL" || req.Target == "STUDENT")
        //     {
        //         var studentEmails = await _db.student
        //             .Where(s => s.email != null && s.email != "")
        //             .Select(s => s.email!)
        //             .ToListAsync();
        //         emails.AddRange(studentEmails);
        //     }

        //     if (req.Target == "ALL" || req.Target == "SELLER")
        //     {
        //         // Add seller emails if your seller table has email
        //         // var sellerEmails = await _db.seller
        //         //     .Where(s => s.email != null)
        //         //     .Select(s => s.email!)
        //         //     .ToListAsync();
        //         // emails.AddRange(sellerEmails);
        //     }

        //     foreach (var email in emails.Distinct())
        //     {
        //         try
        //         {
        //             await _emailService.SendEmailAsync(
        //                 email, req.Title, req.Message);
        //         }
        //         catch { /* continue on individual failure */ }
        //     }
        // }

        // ── Get announcements list (admin) ────
        [HttpGet("/api/voucher/announcements/getall")]
        [Authorize]
        public async Task<IActionResult> GetAll(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            var total = await _db.announcements.CountAsync();
            var data = await _db.announcements
                .OrderByDescending(a => a.created_at)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new
            {
                success = true,
                data,
                pagination = new
                {
                    totalCount = total,
                    totalPages = (int)Math.Ceiling(total / (double)pageSize),
                    currentPage = pageNumber,
                    hasNext = pageNumber * pageSize < total,
                    hasPrevious = pageNumber > 1
                }
            });
        }

        // ── Get user notifications ────────────
        [HttpGet("/api/voucher/announcements/my")]
        [Authorize]
        public async Task<IActionResult> GetMyNotifications(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            var userId = User.FindFirst("sub")?.Value
                      ?? User.FindFirst(System.IdentityModel.Tokens.Jwt
                            .JwtRegisteredClaimNames.Sub)?.Value;

            var query = _db.notifications
                .Where(n => n.user_id == userId)
                .OrderByDescending(n => n.created_at);

            var total = await query.CountAsync();
            var unreadCount = await query.CountAsync(n => !n.is_read);
            var data = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new
            {
                success = true,
                unreadCount,
                data,
                pagination = new
                {
                    totalCount = total,
                    totalPages = (int)Math.Ceiling(total / (double)pageSize),
                    currentPage = pageNumber,
                    hasNext = pageNumber * pageSize < total,
                    hasPrevious = pageNumber > 1
                }
            });
        }

        // ── Mark notification as read ─────────
        [HttpPatch("/api/voucher/announcements/{id}/read")]
        [Authorize]
        public async Task<IActionResult> MarkRead(string id)
        {
            var userId = User.FindFirst("sub")?.Value
                      ?? User.FindFirst(System.IdentityModel.Tokens.Jwt
                            .JwtRegisteredClaimNames.Sub)?.Value;

            await _db.notifications
                .Where(n => n.id == id && n.user_id == userId)
                .ExecuteUpdateAsync(n => n
                    .SetProperty(n => n.is_read, true));

            return Ok(new { success = true });
        }

        // ── Mark all as read ──────────────────
        [HttpPatch("/api/voucher/announcements/read-all")]
        [Authorize]
        public async Task<IActionResult> MarkAllRead()
        {
            var userId = User.FindFirst("sub")?.Value
                      ?? User.FindFirst(System.IdentityModel.Tokens.Jwt
                            .JwtRegisteredClaimNames.Sub)?.Value;

            await _db.notifications
                .Where(n => n.user_id == userId && !n.is_read)
                .ExecuteUpdateAsync(n => n
                    .SetProperty(n => n.is_read, true));

            return Ok(new { success = true });
        }
    }
}