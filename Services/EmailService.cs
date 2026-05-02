using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace voucherMicroservice.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration configuration;


        public EmailService(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public Task SendEmailAsync(string toEmail, string subject, string body)
            => SendEmailAsync(toEmail, toEmail, subject, body);

        public async Task SendEmailAsync(
            string toEmail, string toName, string subject, string body)
        {
            var settings = configuration.GetSection("EmailSettings");
            var fromEmail = settings["FromEmail"]!;
            var fromName = settings["FromName"] ?? "Food Voucher System";
            var smtpHost = settings["SmtpHost"]!;
            var smtpPort = int.Parse(settings["SmtpPort"] ?? "587");
            var username = settings["SmtpUsername"]!;
            var password = settings["SmtpPassword"]!;
            var enableSsl = bool.Parse(settings["EnableSsl"] ?? "true");

            using var client = new SmtpClient(smtpHost, smtpPort)
            {
                Credentials = new NetworkCredential(username, password),
                EnableSsl = enableSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Timeout = 20000
            };

            using var mail = new MailMessage
            {
                From = new MailAddress(fromEmail, fromName),
                Subject = subject,
                Body = BuildHtmlBody(subject, body),
                IsBodyHtml = true
            };

            mail.To.Add(new MailAddress(toEmail, toName));

            try
            {
                await client.SendMailAsync(mail);
            }
            catch (Exception ex)
            {
                // Log but don't throw — email failure should never crash the app
                Console.WriteLine($"[EmailService] Failed to send to {toEmail}: {ex.Message}");
            }
        }

        // ── Simple branded HTML template ─────────────────
        private string BuildHtmlBody(string subject, string plainBody)
        {
            return $@"
                <!DOCTYPE html>
                <html>
                <head>
                <meta charset='utf-8'>
                <meta name='viewport' content='width=device-width, initial-scale=1'>
                <style>
                    body {{ margin: 0; padding: 0; background: #f1f5f9;
                        font-family: 'Segoe UI', Arial, sans-serif; }}
                    .wrapper {{ max-width: 560px; margin: 32px auto; }}
                    .header {{
                    background: linear-gradient(135deg, #1e3a5f, #2563eb);
                    border-radius: 12px 12px 0 0;
                    padding: 28px 32px;
                    text-align: center;
                    }}
                    .header h1 {{
                    color: #fff; margin: 0;
                    font-size: 1.2rem; font-weight: 700;
                    }}
                    .header p {{
                    color: rgba(255,255,255,0.75);
                    font-size: 0.82rem; margin: 4px 0 0;
                    }}
                    .body {{
                    background: #fff;
                    padding: 28px 32px;
                    color: #374151;
                    font-size: 0.92rem;
                    line-height: 1.7;
                    }}
                    .footer {{
                    background: #f8fafc;
                    border-radius: 0 0 12px 12px;
                    border-top: 1px solid #e5e7eb;
                    padding: 16px 32px;
                    text-align: center;
                    font-size: 0.75rem;
                    color: #9ca3af;
                    }}
                    .footer a {{ color: #2563eb; text-decoration: none; }}
                </style>
                </head>
                <body>
                <div class='wrapper'>
                    <div class='header'>
                    <h1>🍽️ Food Voucher System</h1>
                    <p>Sarawak Skills Education</p>
                    </div>
                    <div class='body'>
                    <h2 style='margin:0 0 12px;color:#111827;font-size:1rem'>{subject}</h2>
                    <p style='margin:0'>{plainBody.Replace("\n", "<br/>")}</p>
                    </div>
                    <div class='footer'>
                    <p>This is an automated message from the Food Voucher System.<br/>
                    Please do not reply to this email.</p>
                    <p>© {DateTime.Now.Year} Sarawak Skills Education · 
                    <a href='mailto:fvs@sarawakskills.edu.my'>fvs@sarawakskills.edu.my</a></p>
                    </div>
                </div>
                </body>
                </html>";
        }

    }

    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string body);
        Task SendEmailAsync(string toEmail, string toName, string subject, string body);
    }
}