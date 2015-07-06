using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Mail;
using System.Net;

namespace Dlp.Framework {

    /// <summary>
    /// Delegate for exceptions occurred when sending a mail message.
    /// </summary>
    /// <param name="sender">Object that raised the exception.</param>
    /// <param name="e">Information abount the error.</param>
    public delegate void SendMailErrorEventHandler(object sender, SendMailErrorEventArgs e);

    public interface IMailService {

        /// <summary>
        /// Event fired when an exception occurs when sending an email message.
        /// </summary>
        event SendMailErrorEventHandler OnSendMailError;

        /// <summary>
        /// Sends a mail message async.
        /// </summary>
        /// <param name="mailServerConfiguration">The mail account and smtp configuration.</param>
        /// <param name="mailContent">The mail message content.</param>
        void SendEmailAsync(IMailServerConfiguration mailServerConfiguration, IMailContent mailContent);

        /// <summary>
        /// Sends a mail message.
        /// </summary>
        /// <param name="mailServerConfiguration">The mail account and smtp configuration.</param>
        /// <param name="mailContent">The mail message content.</param>
        void SendEmail(IMailServerConfiguration mailServerConfiguration, IMailContent mailContent);
    }

    /// <summary>
    /// Mail service utility.
    /// </summary>
    public class MailService : IMailService {

        /// <summary>
        /// Event fired when an exception occurs when sending an email message.
        /// </summary>
        public event SendMailErrorEventHandler OnSendMailError;

        /// <summary>
        /// Sends a mail message async.
        /// </summary>
        /// <param name="mailServerConfiguration">The mail account and smtp configuration.</param>
        /// <param name="mailContent">The mail message content.</param>
        public void SendEmailAsync(IMailServerConfiguration mailServerConfiguration, IMailContent mailContent) {

            this.SendEmail(mailServerConfiguration, mailContent, true);
        }

        /// <summary>
        /// Sends a mail message.
        /// </summary>
        /// <param name="mailServerConfiguration">The mail account and smtp configuration.</param>
        /// <param name="mailContent">The mail message content.</param>
        public void SendEmail(IMailServerConfiguration mailServerConfiguration, IMailContent mailContent) {

            this.SendEmail(mailServerConfiguration, mailContent, false);
        }

        private void SendEmail(IMailServerConfiguration mailServerConfiguration, IMailContent mailContent, bool isAsync) {

            MailMessage mailMessage = new MailMessage();

            try {
                mailMessage.From = new MailAddress(mailServerConfiguration.MailAccount, mailContent.DisplayName);
                mailMessage.Subject = mailContent.Subject;
                mailMessage.Body = mailContent.Body;
                mailMessage.IsBodyHtml = mailContent.IsBodyHtml;
                mailMessage.Priority = mailContent.Priority;

                // Verifica se algum anexo foi especificado.
                if (mailContent.AttachmentList != null && mailContent.AttachmentList.Any()) {
                    foreach (Attachment attachment in mailContent.AttachmentList) { mailMessage.Attachments.Add(attachment); }
                }

                // Adiciona todos os destinatários da mensagem.
                mailMessage.To.Add(string.Join(",", mailContent.ReceiverMailList));

                SmtpClient client = new SmtpClient(mailServerConfiguration.SmtpServerAddress, mailServerConfiguration.SmtpPort);
                client.EnableSsl = mailServerConfiguration.UseSsl;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential(mailServerConfiguration.MailAccount, mailServerConfiguration.MailAccountPassword);

                // Verifica se o email deve ser enviado de forma síncrona ou assíncrona.
                if (isAsync == true) { client.SendAsync(mailMessage, null); }
                else { client.Send(mailMessage); }
            }
            catch (Exception ex) {

                // Dispara o evento de erro.
                if (this.OnSendMailError != null) { this.OnSendMailError(this, new SendMailErrorEventArgs(ex)); }
            }
            finally {
                // Finaliza qualquer recurso alocado por arquivos anexos.
                if (mailMessage.Attachments != null) { mailMessage.Attachments.Dispose(); }
            }
        }
    }

    /// <summary>
    /// Class that represents an error ocurred when sending a mail message.
    /// </summary>
    public sealed class SendMailErrorEventArgs : EventArgs {

        /// <summary>
        /// Initializes a new instance of the SendMailErrorEventArgs class.
        /// </summary>
        /// <param name="ex"></param>
        public SendMailErrorEventArgs(Exception ex) { this.Exception = ex; }

        /// <summary>
        /// Gets the exception occurred when sending the mail message.
        /// </summary>
        public Exception Exception { get; private set; }
    }

    /// <summary>
    /// Class that represents an email object to be sent.
    /// </summary>
    public class MailContent : IMailContent {

        /// <summary>
        /// Initializes a new instance of the MailContent class.
        /// </summary>
        public MailContent() {
            this.AttachmentList = new List<Attachment>();
            this.ReceiverMailList = new List<string>();
        }

        /// <summary>
        /// Gets or sets the mail subject.
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// Gets or sets the mail body message.
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// Gets or sets the display name of the sender.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the mail recipients addresses.
        /// </summary>
        public IEnumerable<string> ReceiverMailList { get; set; }

        /// <summary>
        /// Gets or sets the flag indicating whether the mail body is clear text or html. Default value is false (clear text).
        /// </summary>
        public bool IsBodyHtml { get; set; }

        /// <summary>
        /// Gets or sets the mail priority.
        /// </summary>
        public MailPriority Priority { get; set; }

        /// <summary>
        /// Gets or sets the attachments for the mail.
        /// </summary>
        public IEnumerable<Attachment> AttachmentList { get; set; }
    }

    public interface IMailContent {

        /// <summary>
        /// Gets or sets the mail subject.
        /// </summary>
        string Subject { get; set; }

        /// <summary>
        /// Gets or sets the mail body message.
        /// </summary>
        string Body { get; set; }

        /// <summary>
        /// Gets or sets the display name of the sender.
        /// </summary>
        string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the mail recipients addresses.
        /// </summary>
        IEnumerable<string> ReceiverMailList { get; set; }

        /// <summary>
        /// Gets or sets the flag indicating whether the mail body is clear text or html. Default value is false (clear text).
        /// </summary>
        bool IsBodyHtml { get; set; }

        /// <summary>
        /// Gets or sets the mail priority.
        /// </summary>
        MailPriority Priority { get; set; }

        /// <summary>
        /// Gets or sets the attachments for the mail.
        /// </summary>
        IEnumerable<Attachment> AttachmentList { get; set; }
    }

    public interface IMailServerConfiguration {

        /// <summary>
        /// Gets or sets the mail account that will be used to send messages.
        /// </summary>
        string MailAccount { get; set; }

        /// <summary>
        /// Gets or sets the mail account password.
        /// </summary>
        string MailAccountPassword { get; set; }

        /// <summary>
        /// Gets or sets the SMTP server address.
        /// </summary>
        string SmtpServerAddress { get; set; }

        /// <summary>
        /// Gets or sets the SMTP server port.
        /// </summary>
        short SmtpPort { get; set; }

        /// <summary>
        /// Gets or sets the flag that indicates if a SSL connection is required by the SMTP server address.
        /// </summary>
        bool UseSsl { get; set; }
    }

    /// <summary>
    /// Class that represents the mail server configuration to be used when sending messages.
    /// </summary>
    public class MailServerConfiguration : IMailServerConfiguration {

        /// <summary>
        /// Initializes a new instance of the MailServerConfiguration class.
        /// </summary>
        public MailServerConfiguration() { }

        /// <summary>
        /// Gets or sets the mail account that will be used to send messages.
        /// </summary>
        public string MailAccount { get; set; }

        /// <summary>
        /// Gets or sets the mail account password.
        /// </summary>
        public string MailAccountPassword { get; set; }

        /// <summary>
        /// Gets or sets the SMTP server address.
        /// </summary>
        public string SmtpServerAddress { get; set; }

        /// <summary>
        /// Gets or sets the SMTP server port.
        /// </summary>
        public short SmtpPort { get; set; }

        /// <summary>
        /// Gets or sets the flag that indicates if a SSL connection is required by the SMTP server address.
        /// </summary>
        public bool UseSsl { get; set; }
    }
}
