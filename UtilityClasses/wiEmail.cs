using System;
using System.Net.Mail;
using System.Collections.Generic;

namespace WebIt.Business.UtilityClasses
{
    /// <summary>
    /// Business class for sending emails
    /// </summary>
    public class wiEmail : wiBusinessObject
    {
        private List<string> _Attachments;

        /// <summary>
        /// wiEmail constructor
        /// </summary>
        public wiEmail()
        {
            this._Attachments = new List<string>();
        }

        /// <summary>
        /// Add a file attachment to this email message
        /// </summary>
        /// <param name="attachment"></param>
        public void AddAttachment(string attachment)
        {
            if (attachment != string.Empty)
                this._Attachments.Add(attachment);
        }

        /// <summary>
        /// Send an email with an embedded image
        /// </summary>
        /// <param name="toAddress"></param>
        /// <param name="fromAddress"></param>
        /// <param name="subject"></param>
        /// <param name="body"></param>
        /// <param name="isHtml"></param>
        /// <param name="priority"></param>
        /// <param name="embedImage"></param>
        /// <param name="embedImagePath"></param>
        /// <param name="embedImageId"></param>
        public void SendEmail(string toAddress, string fromAddress, string subject, string body, bool isHtml, MailPriority priority
            , string embedImage, string embedImagePath, string embedImageId)
        {
            // Create an email message and populate it
            MailMessage Message = new MailMessage();
            Message.From = new MailAddress(fromAddress);
            Message.Subject = subject;
            Message.Body = body;
            Message.IsBodyHtml = isHtml;
            Message.Priority = priority;

            // Check to see if the To Address is a semicolon delimited list and add each one
            string[] Addresses = toAddress.Split(Convert.ToChar(";"));
            if (Addresses.Length > 0)
            {
                foreach (string To in Addresses)
                {
                    if (To != string.Empty)
                        Message.To.Add(new MailAddress(To));
                }
            }
            else
                Message.To.Add(new MailAddress(toAddress));

            // If an image needs to be embedded, embed it and add an alternate view to the email for programs that can't display HTML mail
            if (embedImage != string.Empty)
            {
                string FileName = embedImagePath + embedImage;
                AlternateView av = AlternateView.CreateAlternateViewFromString(body, null, System.Net.Mime.MediaTypeNames.Text.Html);
                LinkedResource lr = new LinkedResource(FileName);
                lr.ContentId = embedImageId;
                lr.ContentType.Name = FileName;
                av.LinkedResources.Add(lr);
                Message.AlternateViews.Add(av);
            }

            // Add any attachments that were given
            foreach (string attachment in this._Attachments)
            {
                Message.Attachments.Add(new Attachment(attachment));
            }

            // Create a SMTP client 
            SmtpClient Client = new SmtpClient();
            Client.Send(Message);
        }

        /// <summary>
        /// Send an email
        /// </summary>
        /// <param name="toAddress"></param>
        /// <param name="fromAddress"></param>
        /// <param name="subject"></param>
        /// <param name="body"></param>
        /// <param name="isHtml"></param>
        /// <param name="priority"></param>
        public void SendEmail(string toAddress, string fromAddress, string subject, string body, bool isHtml, MailPriority priority)
        {
            this.SendEmail(toAddress, fromAddress, subject, body, isHtml, priority, "", "", "");
        }
    }
}
