using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using MimeKit;
using TekLabServiceCore.Extensions;
using TekLabServiceCore.Models;

namespace TekLabServiceCore.Common
{
    public class EmailSender
    {
        SmtpClient smtpClient { get; set; }
        IConfiguration configuration { get; set; }
        public EmailSender(IConfiguration _configuration)
        {
            configuration = _configuration;
        }
        public void SendEnquiryEmail(Enquiry enquiry )
        {
            connectSMTPCLient();
            MimeMessage emailMessage = new MimeMessage();

            MailboxAddress from = new MailboxAddress("TekLab Inquiry Notification",
            configuration.GetSMTPUserName());
            emailMessage.From.Add(from);

            var emailRecipients = configuration.GetEmailRecipients();
            InternetAddressList recipients = new InternetAddressList();
            foreach (var rec in emailRecipients)
            {
                recipients.Add(new MailboxAddress("", rec));
            }
            emailMessage.To.AddRange(recipients);

            emailMessage.Subject = "Student Inquiry Received";

            BodyBuilder bodyBuilder = new BodyBuilder();
            bodyBuilder.HtmlBody = "<h3>The below Inquiry received:</h3></br><div><h5>FirstName:</h5><span>" + enquiry.FirstName + "</span></div>" +
                                   "<div><h5>LastName:</h5><span>" + enquiry.LastName + "</span></div>" +
                                   "<div><h5>Mobile:</h5><span>" + enquiry.Mobile + "</span></div>" +
                                   "<div><h5>Email:</h5><span>" + enquiry.Email + "</span></div>" +
                                   "<div><h5>Message:</h5><span>" + enquiry.Message + "</span></div>";

            emailMessage.Body = bodyBuilder.ToMessageBody();
            smtpClient.Send(emailMessage);
            disConnectSMTPCLient();
        }

        private void connectSMTPCLient() {
            smtpClient = new SmtpClient();
            smtpClient.Connect(configuration.GetSMTPServer(), int.Parse(configuration.GetSMTPPort()), true);
            smtpClient.Authenticate(configuration.GetSMTPUserName(), configuration.GetSMTPPassword());
        }
        private void disConnectSMTPCLient()
        {
            smtpClient.Disconnect(true);
            smtpClient.Dispose();
        }

    }
}
