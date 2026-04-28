using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using voucherMicroservice.Data;
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

        public async Task SendPaymentApprovalRequest(
        string studentId, string sellerName, decimal amount, string paymentId)
        {
            var subscriptions = await dataContext.push_subscription
                .Where(s => s.student_id == studentId)
                .ToListAsync();

            if (!subscriptions.Any()) return;

            var subject = configuration["VapidKeys:Subject"]!;
            var publicKey = configuration["VapidKeys:PublicKey"]!;
            var privateKey = configuration["VapidKeys:PrivateKey"]!;

            var payload = System.Text.Json.JsonSerializer.Serialize(new
            {
                type = "PAYMENT_APPROVAL",
                paymentId,
                sellerName,
                amount,
                message = $"Payment request of RM {amount:F2} from {sellerName}"
            });

            foreach (var sub in subscriptions)
            {
                try
                {
                    var pushSubscription = new WebPush.PushSubscription(
                        sub.endpoint, sub.p256dh, sub.auth);

                    var vapidDetails = new WebPush.VapidDetails(subject, publicKey, privateKey);
                    var client = new WebPush.WebPushClient();
                    await client.SendNotificationAsync(pushSubscription, payload, vapidDetails);
                }
                catch
                {
                    // Remove invalid subscription
                    dataContext.push_subscription.Remove(sub);
                    await dataContext.SaveChangesAsync();
                }
            }
        }
    }

    public interface IWebPushService
    {
        Task<string> GetVapidKeys();
        Task SendPaymentApprovalRequest(
        string studentId, string sellerName, decimal amount, string paymentId);
    }
}