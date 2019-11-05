using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace WebItBusiness.UtilityClasses
{
    public class wiSendGrid
    {
        private string ApiKey
        {
            get => System.Configuration.ConfigurationManager.AppSettings["SendGridAPIKey"].ToString();
        }

        public void SendEmail(string fromAddress, string fromName, string toAddress, string toName, string subject, string emailBody, bool isHtml)
        {
            var client = new SendGridClient(ApiKey);
            EmailAddress from = new EmailAddress(fromAddress, fromName);
            EmailAddress to = new EmailAddress(toAddress, toName);
            string TextBody = (isHtml ? string.Empty : emailBody);
            string HtmlBody = (isHtml ? emailBody : string.Empty);

            SendGridMessage msg = MailHelper.CreateSingleEmail(from, to, subject, TextBody, HtmlBody);
            client.SendEmailAsync(msg).Wait();
        }
    }
}
