using Microsoft.AspNetCore.Identity.UI.Services;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace ibhayiPharmacy.Services
{
    public class SmtpEmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var mail = new MailMessage();
            mail.From = new MailAddress("o.m.makapela@gmail.com"); // Replace with your email
            mail.To.Add(email);
            mail.Subject = subject;
            mail.Body = htmlMessage;
            mail.IsBodyHtml = true;

            using (var smtp = new SmtpClient("smtp.gmail.com", 587))
            {
                smtp.Credentials = new NetworkCredential("o.m.makapela@gmail.com", "dqrv tctk ubln snjv");
                smtp.EnableSsl = true;
                smtp.Send(mail);
            }

            return Task.CompletedTask;
        }
    }
}

