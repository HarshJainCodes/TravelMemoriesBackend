using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Mail;
using TravelMemories.Contracts.Data;
using TravelMemories.Database;

namespace TravelMemories.Controllers.EmailService
{
    [ApiController]
    [Route("[controller]")]
    public class EmailServiceController : ControllerBase
    {
        private IConfiguration _configuration;
        private ImageMetadataDBContext _imageMetadataDBContext;

        public EmailServiceController(IConfiguration configuration, ImageMetadataDBContext imageMetadataDBContext)
        {
            _configuration = configuration;
            _imageMetadataDBContext = imageMetadataDBContext;
        }

        [HttpPost]
        public async Task SendEmail(EmailParameters emailParameters)
        {
            string smtpServer = "smtp.zoho.in";
            int portNumber = 587; // for TLS

            string senderEmail = _configuration["otpSenderEmail"];
            string senderEmailPassword = _configuration["otpSenderPassword"];

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            var smtpClient = new SmtpClient(smtpServer, portNumber)
            {
                Credentials = new NetworkCredential(senderEmail, senderEmailPassword),
                EnableSsl = true,
            };

            MailMessage mail = new MailMessage();

            int otp = GenerateRandomOTP();

            VerificationCodes existingEmail = _imageMetadataDBContext.VerificationCodes.Where(v => v.UserEmail == emailParameters.ReceiverEmail).FirstOrDefault();
            if (existingEmail != null)
            {
                // has already sent an OTP to this email, update the OTP
                existingEmail.OTP = otp;
            }
            else
            {
                // sending OTP for the first time to this email
                _imageMetadataDBContext.VerificationCodes.Add(new VerificationCodes
                {
                    UserEmail = emailParameters.ReceiverEmail,
                    OTP = otp,
                });
            }
            await _imageMetadataDBContext.SaveChangesAsync();

            mail.From = new MailAddress(senderEmail);
            mail.To.Add(emailParameters.ReceiverEmail);
            mail.Subject = "Travel Memories - Verification Code";
            mail.Body = $"Use This OTP to sign in to your Travel Memories Account : {otp}. Do not share this OTP with anyone.";

            try
            {
                smtpClient.Send(mail);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        [NonAction]
        public int GenerateRandomOTP()
        {
            Random random = new Random();
            return random.Next(100000, 999999);
        }
    }

    public class EmailParameters
    {
        public string ReceiverEmail { get; set; }
    }
}
