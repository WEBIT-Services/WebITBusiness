using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace WebItBusiness.UtilityClasses
{
    public class wiSendGrid
    {
        private static string ApiKey
        {
            get => System.Configuration.ConfigurationManager.AppSettings["SendGridAPIKey"].ToString();
        }

        /// <summary>
        /// Send an email using the SendGrid service. The SendGridAPIKey api key MUST be in your project's application config file!
        /// </summary>
        /// <param name="fromAddress">The email address to use as the FROM address</param>
        /// <param name="fromName">The person's name/description to use in the FROM address</param>
        /// <param name="toAddress">The email address to use as the TO address</param>
        /// <param name="toName">The person's name/description to use in the TO address</param>
        /// <param name="subject">The Subject for the email</param>
        /// <param name="emailBody">The Body of the email</param>
        /// <param name="isHtml">Flag that specifies if the Body contains HTML and should be displayed as such in the mail client</param>
        public static void SendEmail(string fromAddress, string fromName, string toAddress, string toName, string subject, string emailBody, bool isHtml)
        {
            var client = new SendGridClient(ApiKey);
            EmailAddress from = new EmailAddress(fromAddress, fromName);
            EmailAddress to = new EmailAddress(toAddress, toName);
            string TextBody = (isHtml ? string.Empty : emailBody);
            string HtmlBody = (isHtml ? emailBody : string.Empty);

            SendGridMessage msg = MailHelper.CreateSingleEmail(from, to, subject, TextBody, HtmlBody);
            client.SendEmailAsync(msg).Wait();
        }
        public static void SendEmailWAtt(string fromAddress, string fromName, string toAddress, string toName, string subject, string emailBody, MemoryStream ms, string fileName, bool isHtml)
        {
            var client = new SendGridClient(ApiKey);
            EmailAddress from = new EmailAddress(fromAddress, fromName);
            EmailAddress to = new EmailAddress(toAddress, toName);
            string TextBody = (isHtml ? string.Empty : emailBody);
            string HtmlBody = (isHtml ? emailBody : string.Empty);

            SendGridMessage msg = MailHelper.CreateSingleEmail(from, to, subject, TextBody, HtmlBody);
            msg.AddAttachmentAsync(fileName, ms);
            client.SendEmailAsync(msg).Wait();
        }
    }
}
