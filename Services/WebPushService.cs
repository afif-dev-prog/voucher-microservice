using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using voucherMicroservice.Data;
using voucherMicroservice.Model;
using WebPush;

namespace voucherMicroservice.Services
{

    public class WebPushService : IWebPushService
    {
        private readonly DataContext dataContext;
        private readonly IConfiguration configuration;

        public WebPushService(DataContext dataContext, IConfiguration configuration)
        {
            this.dataContext = dataContext;
            this.configuration = configuration;
        }

        public async Task<string> GetVapidKeys()
        {
            var vapidKeys = VapidHelper.GenerateVapidKeys();
            string vKeys = $"Public: {vapidKeys.PublicKey}" + " " + $"Private: {vapidKeys.PrivateKey}";

            return vKeys;
        }

        public async Task SendPaymentApprovalRequest(string? studentId, string? sellerName, decimal? amount, string paymentId)
        {
            var payload = System.Text.Json.JsonSerializer.Serialize(new
            {
                type = "PAYMENT_APPROVAL",
                paymentId,
                sellerName,
                amount,
                title = "Payment Request",
                message = $"RM {amount:F2} from {sellerName}"
            });

            await PushToUser(studentId, payload);
        }
        // ── Send to single user ───────────────
        public async Task SendToUser(
            string userId, string title, string message,
            string type, string? referenceId = null)
        {
            var payload = System.Text.Json.JsonSerializer.Serialize(new
            {
                type,
                title,
                message,
                referenceId
            });

            // Save to notifications table
            var notification = new Notifications
            {
                user_id = userId,
                user_type = type.Contains("SELLER") ? "SELLER" : "STUDENT",
                type = type,
                title = title,
                message = message,
                created_at = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                reference_id = referenceId
            };
            await dataContext.notifications.AddAsync(notification);
            await dataContext.SaveChangesAsync();

            await PushToUser(userId, payload);
        }

        // ── Send to all users of a type ───────
        public async Task SendToGroup(
            string userType, string title, string message, string type)
        {
            var payload = System.Text.Json.JsonSerializer.Serialize(new
            {
                type,
                title,
                message
            });

            // Get all subscriptions for user type
            List<string> userIds;

            if (userType == "ALL")
            {
                var studentIds = await dataContext.student
                    .Select(s => s.student_id).ToListAsync();
                var sellerIds = await dataContext.seller
                    .Select(s => s.username).ToListAsync();
                userIds = studentIds.Concat(sellerIds).ToList();
            }
            else if (userType == "STUDENT")
            {
                userIds = await dataContext.student.Select(s => s.student_id).ToListAsync();
            }
            else
            {
                userIds = await dataContext.seller.Select(s => s.username).ToListAsync();
            }

            // Save notifications in bulk
            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var notifications = userIds.Select(uid => new Notifications
            {
                user_id = uid,
                user_type = userType == "ALL" ? "ALL" : userType,
                type = type,
                title = title,
                message = message,
                created_at = now
            }).ToList();

            await dataContext.notifications.AddRangeAsync(notifications);
            await dataContext.SaveChangesAsync();

            // Push to all
            foreach (var uid in userIds)
                await PushToUser(uid, payload);
        }

        // ── Internal push helper ──────────────
        private async Task PushToUser(string userId, string payload)
        {
            var subscriptions = await dataContext.push_subscription
                .Where(s => s.student_id == userId)
                .ToListAsync();

            if (!subscriptions.Any()) return;

            var subject = configuration["VapidKeys:Subject"]!;
            var publicKey = configuration["VapidKeys:PublicKey"]!;
            var privateKey = configuration["VapidKeys:PrivateKey"]!;

            foreach (var sub in subscriptions)
            {
                try
                {
                    var pushSub = new WebPush.PushSubscription(
                        sub.endpoint, sub.p256dh, sub.auth);
                    var vapid = new WebPush.VapidDetails(subject, publicKey, privateKey);
                    var client = new WebPush.WebPushClient();
                    await client.SendNotificationAsync(pushSub, payload, vapid);
                }
                catch
                {
                    dataContext.push_subscription.Remove(sub);
                    await dataContext.SaveChangesAsync();
                }
            }
        }
        // public async Task SendPaymentApprovalRequest(
        // string? studentId, string? sellerName, decimal? amount, string? paymentId)
        // {
        //     var subscriptions = await dataContext.push_subscription
        //         .Where(s => s.student_id == studentId)
        //         .ToListAsync();

        //     if (!subscriptions.Any()) return;

        //     var subject = configuration["VapidKeys:Subject"]!;
        //     var publicKey = configuration["VapidKeys:PublicKey"]!;
        //     var privateKey = configuration["VapidKeys:PrivateKey"]!;

        //     var payload = System.Text.Json.JsonSerializer.Serialize(new
        //     {
        //         type = "PAYMENT_APPROVAL",
        //         paymentId,
        //         sellerName,
        //         amount,
        //         message = $"Payment request of RM {amount:F2} from {sellerName}"
        //     });

        //     foreach (var sub in subscriptions)
        //     {
        //         try
        //         {
        //             var pushSubscription = new WebPush.PushSubscription(
        //                 sub.endpoint, sub.p256dh, sub.auth);

        //             var vapidDetails = new WebPush.VapidDetails(subject, publicKey, privateKey);
        //             var client = new WebPush.WebPushClient();
        //             await client.SendNotificationAsync(pushSubscription, payload, vapidDetails);
        //         }
        //         catch
        //         {
        //             // Remove invalid subscription
        //             dataContext.push_subscription.Remove(sub);
        //             await dataContext.SaveChangesAsync();
        //         }
        //     }
        // }

    }

    public interface IWebPushService
    {
        Task<string> GetVapidKeys();
        Task SendPaymentApprovalRequest(string? studentId, string? sellerName, decimal? amount, string paymentId);
        Task SendToUser(string userId, string title, string message, string type, string? referenceId = null);
        Task SendToGroup(string userType, string title, string message, string type);
    }
}