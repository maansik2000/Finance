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
        //IMailService interface is implemented by MailService class in that all the methods are defined.
        //using asynchronous programming, the application can work on other task without waiting for the task to be completed
        //async keyword is used to make the method asynchronous, If any Second Method, as method2 has a dependency on method1,
        //then it will wait for the completion of Method1 with the help of await keyword.
        //The async keyword marks the method as asynchronous.
        //The await keyword waits for the async method to complete until it returns a value.
        private IConfiguration _configuration;

        public MailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public async Task<ApiResponse> SendEmailAsync(string toEmail,string Subject, string content)
        {
            try
            {
                var apiPassword = _configuration["GmailAppPassword"];   //getting the email api from the app settings
                string fromMail = "mansisarkar4@gmail.com";             //from the mail we will send our email
                MailMessage message = new MailMessage();                //creating object of MailMessage for sending mail
                message.From = new MailAddress(fromMail);               //adding from mail 
                message.Subject = Subject;                              //adding subject
                message.To.Add(new MailAddress(toEmail));               //adding to Mail
                message.Body = content;                                 //adding body
                message.IsBodyHtml = true;                              //setting html format to true in body

                var smtpClient = new SmtpClient("smtp.gmail.com")       //configuring smtp client
                {
                    Port = 587,
                    Credentials = new NetworkCredential(fromMail, apiPassword),
                    EnableSsl = true,
                };

                smtpClient.Send(message);                               //sending email to the user

                return new ApiResponse                                  //sending true response
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
