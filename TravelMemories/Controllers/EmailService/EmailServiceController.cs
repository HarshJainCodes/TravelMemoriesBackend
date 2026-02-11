using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Mail;
using TravelMemories.Contracts.Data;
using TravelMemories.Controllers.Authentication;
using TravelMemories.Database;
using TravelMemoriesBackend.Contracts.Data;

namespace TravelMemories.Controllers.EmailService
{
    [ApiController]
    [AllowAnonymous]
    [Route("[controller]")]
    public class EmailServiceController : ControllerBase
    {
        private IConfiguration _configuration;
        private ImageMetadataDBContext _imageMetadataDBContext;
        private LoginController _loginController;

        public EmailServiceController(IConfiguration configuration, ImageMetadataDBContext imageMetadataDBContext, LoginController loginController)
        {
            _configuration = configuration;
            _imageMetadataDBContext = imageMetadataDBContext;
            _loginController = loginController;
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
                existingEmail.IssuedAt = DateTime.UtcNow;
            }
            else
            {
                // sending OTP for the first time to this email
                _imageMetadataDBContext.VerificationCodes.Add(new VerificationCodes
                {
                    UserEmail = emailParameters.ReceiverEmail,
                    OTP = otp,
                    IssuedAt = DateTime.UtcNow,
                });

                _imageMetadataDBContext.UserInfo.Add(new UserInfo
                {
                    Email = emailParameters.ReceiverEmail,
                    Name = emailParameters.ReceiverEmail,
                    ProfilePictureURL = "",
                });

                _imageMetadataDBContext.SubscriptionDetails.Add(new SubscriptionDetails
                {
                    UserEmail = emailParameters.ReceiverEmail
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

        [HttpPost("VerifyOtp")]
        public async Task<IActionResult> VerifyOTP(VerifyOTPParams verifyOTPParams)
        {
            VerificationCodes verificationCodes = _imageMetadataDBContext.VerificationCodes.Where((record) => record.UserEmail == verifyOTPParams.Email).FirstOrDefault();

            if (verificationCodes != null)
            {
                // if OTP is older than 10 minutes, reject it
                if (verifyOtpCorrect(verifyOTPParams))
                {
                    _loginController.GenerateJWTToken(new JWTInputs
                    {
                        Name = "User",
                        Email = verifyOTPParams.Email,
                    }, HttpContext);

                    return Ok();
                }
                return Unauthorized();
            }
            else
            {
                return Unauthorized();
            }
        }

        [HttpPost]
        [Route("VerifyOtpVsCode")]
        public async Task<OtpVerifyVsCode> VerifyOtpVsCode(VerifyOTPParams verifyOTPParams)
        {
            if (verifyOtpCorrect(verifyOTPParams))
            {
                return new OtpVerifyVsCode()
                {
                    RedirectUri = "http://127.0.0.1:33418"
                };
            }
            
            return new OtpVerifyVsCode();
        }

        [NonAction]
        public bool verifyOtpCorrect(VerifyOTPParams verifyOTPParams)
        {
            VerificationCodes verificationCodes = _imageMetadataDBContext.VerificationCodes.Where((record) => record.UserEmail == verifyOTPParams.Email).FirstOrDefault();

            if (verificationCodes.OTP.ToString() == verifyOTPParams.OTP && (DateTime.UtcNow - verificationCodes.IssuedAt).Minutes < 10)
            {
                return true;
            }
            return false;
        }

        [NonAction]
        public int GenerateRandomOTP()
        {
            Random random = new Random();
            return random.Next(100000, 999999);
        }
    }

    public class OtpVerifyVsCode
    {
        public string RedirectUri { get; set; }
    }

    public class EmailParameters
    {
        public string ReceiverEmail { get; set; }
    }

    public class VerifyOTPParams
    {
        public string Email { get; set; }

        public string OTP { get; set; }
    }
}
