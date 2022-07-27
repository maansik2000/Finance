using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using SendGrid;
using System.Threading.Tasks;
using Microsoft.Graph;
using System.Net.Mail;
using System.Net;
using Authentication.Models;
using System.Text;

namespace Authentication.Services
{
    public interface IMailService
    {
        Task<ApiResponse> SendEmailAsync(string toEmail, string Subject, string conten);

    }

    public class MailService : IMailService
    {
        private IConfiguration _configuration;

        public MailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public async Task<ApiResponse> SendEmailAsync(string toEmail,string Subject, string content)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("&lt;html><head><title>CONFIRMATION EMAIL:</title></head><body>");


                sb.Append("&lt;p>HERE IS THE LINK:</p><br/>");
                sb.Append("&lt;p>" + "&lt;a href=" + "http://www.google.com" + ">" + "Click" + "</a></p><br/>");

                var apiPassword = _configuration["GmailAppPassword"];
                string fromMail = "mansisarkar4@gmail.com";
                MailMessage message = new MailMessage();
                message.From = new MailAddress(fromMail);
                message.Subject = Subject;
                message.To.Add(new MailAddress(toEmail));
                message.Body = content;
                message.IsBodyHtml = true;

                var smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential(fromMail, apiPassword),
                    EnableSsl = true,
                };

                smtpClient.Send(message);

                return new ApiResponse
                {
                    success = true,
                    message = "Email sent successfully"
                };
            }
            catch (SmtpException ex)
            {
                throw new ApplicationException
                  ("SmtpException has occured: " + ex.Message);
            }
            catch (Exception ex)
            {
                throw ex;
            }


        }
    }
}
