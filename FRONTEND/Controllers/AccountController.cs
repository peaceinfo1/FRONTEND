using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using DAL.SHARED;
using Microsoft.EntityFrameworkCore;
using DAL.AUDIT;
using BAL.Identity;
using Newtonsoft.Json;
using BAL.Messaging.Contracts;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using BAL.Addresses;
using DAL.USER;
using DAL.Models;

namespace FRONTEND.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> SignInManager;
        private readonly UserManager<ApplicationUser> UserManager;
        private readonly UserDbContext ApplicationContext;
        private readonly SharedDbContext SharedContext;
        private readonly AuditDbContext AuditContext;
        public readonly IUserRoleClaim UserRoleClaim;
        private readonly IMessageMailService Notification;
        private readonly IAddresses Addresses;

        public AccountController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager,
            UserDbContext applicationContext, SharedDbContext sharedContext, AuditDbContext auditContext, 
            IUserRoleClaim userRoleClaim, IMessageMailService notification, IAddresses addresses)
        {
            SignInManager = signInManager;
            UserManager = userManager;
            ApplicationContext = applicationContext;
            SharedContext = sharedContext;
            AuditContext = auditContext;
            UserRoleClaim = userRoleClaim;
            Notification = notification;
            Addresses = addresses;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // Shafi: Create OtpModel
        public class OtpModel
        {
            public int OtpId { get; set; }
            public string Mobile { get; set; }
            public string Status { get; set; }
        }
        // End:

        [HttpPost]
        public async Task<JsonResult> Register(string countryCode, string mobile)
        {
            if (countryCode != null && mobile != null)
            {
                // Shafi: Get country by cound code
                var country = await SharedContext.Country.Where(i => i.PhoneCode == countryCode).FirstOrDefaultAsync();
                // End:

                // Shafi: Get user by mobile
                var userRecord = await ApplicationContext.Users.Where(i => i.UserName == mobile).FirstOrDefaultAsync();
                // End:

                // Shafi: If user same mobile does not exist then create a new user
                if (userRecord == null)
                {
                    // Shafi: Generate OTP
                    var generatedOtpId = await UserRoleClaim.GenerateUserRegistrationOtp(mobile);
                    var getOtp = await AuditContext.UserRegistrationOTPVerification.Where(i => i.OtpID == generatedOtpId).FirstOrDefaultAsync();
                    // End:

                    // Shafi: Send OTP Via SMS
                    Notification.SendSMSInternational(countryCode, mobile, $"Your OTP for MIM is {getOtp.OTP}");
                    // End:

                    OtpModel otp = new OtpModel
                    {
                        OtpId = generatedOtpId,
                        Mobile = mobile,
                        Status = "Success"
                    };

                    // Serialize OtpModel Object to Json
                    var jsonOtpModel = JsonConvert.SerializeObject(otp);
                    // End:

                    // Shafi: Return jsonOtpModel
                    return Json(jsonOtpModel);
                    // End:
                }
                // End:
                // Shafi: If user already exists then execute this
                else
                {
                    var userProfile = await ApplicationContext.UserProfile.Where(i => i.OwnerGuid == userRecord.Id).FirstOrDefaultAsync();
                    if (userProfile != null)
                    {
                        OtpModel otp = new OtpModel
                        {
                            OtpId = 0,
                            Mobile = mobile,
                            Status = "User with same mobile already exists."
                        };

                        // Serialize OtpModel Object to Json
                        var jsonOtpModel = JsonConvert.SerializeObject(otp);
                        // End:

                        // Shafi: Return jsonOtpModel
                        return Json(jsonOtpModel);
                        // End:
                    }
                    else
                    {
                        OtpModel otp = new OtpModel
                        {
                            OtpId = 0,
                            Mobile = mobile,
                            Status = "User with same mobile already exists."
                        };

                        // Serialize OtpModel Object to Json
                        var jsonOtpModel = JsonConvert.SerializeObject(otp);
                        // End:

                        // Shafi: Return jsonOtpModel
                        return Json(jsonOtpModel);
                        // End:
                    }
                }
                // End:
            }
            else
            {
                return Json("Sorry");
            }
        }

        [HttpPost]
        public async Task<JsonResult> OTPConfirmation(string otp, string mobile, int otpId)
        {
            var otpRecord = await AuditContext.UserRegistrationOTPVerification.Where(i => i.OtpID == otpId).FirstOrDefaultAsync();

            if (otpRecord != null && otpRecord.OTP == Int32.Parse(otp) && otpRecord.Mobile == mobile)
            {
                return Json("Success");
            }
            else
            {
                return Json("Fail");
            }
        }

        // Shafi: Create PasswordConfirmationModel
        public class UserRegistrationModel
        {
            public int? OtpId { get; set; }
            public string Mobile { get; set; }
            public string Email { get; set; }
            public string Password { get; set; }
            public string ConfirmPassword { get; set; }
            public string Status { get; set; }
        }
        // End:

        [HttpPost]
        public async Task<JsonResult> PasswordConfirmation(int? otpId, string mobile, string email, string password, string confirmPassword)
        {
            UserRegistrationModel userModel = new UserRegistrationModel
            {
                OtpId = otpId,
                Mobile = mobile,
                Email = email,
                Password = password,
                ConfirmPassword = confirmPassword
            };

            if(userModel.OtpId != null && mobile != null && email != null && password != null && confirmPassword != null && password == confirmPassword)
            {
                // Shafi: Find OTP record by OtpId
                var otpRecord = await AuditContext.UserRegistrationOTPVerification.Where(i => i.OtpID == userModel.OtpId).FirstOrDefaultAsync();
                if(otpRecord == null)
                {
                    // Shafi: Set userModel.Status to Invalid OTP Record Found
                    userModel.Status = "Invalid OTP Record";
                    // End:

                    // Shafi: Return Json Result
                    return Json(userModel.Status);
                    // End:
                }
                // End:

                // Shafi: Match mobile number with otpRecord mobile
                if (otpRecord.Mobile == mobile)
                {
                    // Shafi: Create User
                    var user = new ApplicationUser { UserName = mobile, Email = email, PhoneNumber = mobile };
                    await UserManager.CreateAsync(user, password);
                    // End:

                    // Shafi: Change OTP Status to verified
                    otpRecord.Status = "Verified";
                    otpRecord.OTPVerified = true;
                    AuditContext.Update(otpRecord);
                    await AuditContext.SaveChangesAsync();
                    // End:

                    // Shafi: Notify user via SMS after creating account
                    Notification.SendSMS(mobile, $"Dear user, thanks for registering your account with www.myinteriormart.com your User ID is {user.PhoneNumber} and password is {password}");
                    // End:

                    // Shafi: Send email verification link to user
                    if(email != null)
                    {
                        string returnUrl = null;
                        returnUrl = returnUrl ?? Url.Content("~/");
                        var code = await UserManager.GenerateEmailConfirmationTokenAsync(user);
                        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                        var callbackUrl = Url.Page(
                            "/Account/ConfirmEmail",
                            pageHandler: null,
                            values: new { area = "Identity", userId = user.Id, code = code, returnUrl = returnUrl },
                            protocol: Request.Scheme);

                        Notification.SendEmail(email, "Email Confirmation Link", $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here.<br/><br/>Visit www.myinteriormart.com</a>.");
                    }
                    // End:

                    // Shafi: Sign in user
                    await SignInManager.SignInAsync(user, false);
                    // End:

                    // Shafi: Set userModel.Status to Success
                    userModel.Status = "Success";
                    // End:

                    // Shafi: Return Json Result
                    return Json(userModel.Status);
                    // End:
                }
                // End:
                else
                {
                    // Shafi: Set userModel.Status to Mobile Not Matched
                    userModel.Status = "Mobile Not Matched";
                    // End:

                    // Shafi: Return Json Result
                    return Json(userModel.Status);
                    // End:
                }
            }
            else if(password != confirmPassword)
            {
                // Shafi: Set userModel.Status to Password Not Matched
                userModel.Status = "Password Not Matched";
                // End:

                // Shafi: Return Json Result
                return Json(userModel.Status);
                // End:
            }
            else
            {
                // Shafi: Set userModel.Status to Oops! Something Went Wrong
                userModel.Status = "Oops! Something Went Wrong";
                // End:

                // Shafi: Return Json Result
                return Json(userModel.Status);
                // End:
            }
        }

        [HttpPost]
        public async Task<JsonResult> GenerateLoginOTP(string mobile)
        {
            // Shafi: Get user by mobile
            var user = await UserManager.FindByNameAsync(mobile);
            // End:

            // Shafi: If user same mobile does not exist then create a new user
            if (user != null)
            {
                // Shafi: Get user profile
                var userProfile = await ApplicationContext.UserProfile.Where(i => i.OwnerGuid == user.Id).FirstOrDefaultAsync();
                // End:

                // Shafi: If userProfile is not null then execute this
                if(userProfile != null)
                {
                    // Shafi: Get Country
                    var country = await Addresses.CountryDetailsAsync(userProfile.CountryID);
                    // End:

                    if(country != null)
                    {
                        // Shafi: Generate OTP
                        var generatedOtpId = await UserRoleClaim.GenerateUserLoginOtp(mobile);
                        var getOtp = await AuditContext.UserLoginOTPVerification.Where(i => i.OtpID == generatedOtpId).FirstOrDefaultAsync();
                        // End:

                        // Shafi: Send OTP Via SMS
                        Notification.SendSMSInternational(country.PhoneCode, mobile, $"Your OTP for MIM is {getOtp.OTP}");
                        // End:

                        OtpModel otp = new OtpModel
                        {
                            OtpId = generatedOtpId,
                            Mobile = mobile,
                            Status = "Success"
                        };

                        // Serialize OtpModel Object to Json
                        var jsonOtpModel = JsonConvert.SerializeObject(otp);
                        // End:

                        // Shafi: Return jsonOtpModel
                        return Json(jsonOtpModel);
                        // End:
                    }
                    else
                    {
                        OtpModel otp = new OtpModel
                        {
                            OtpId = 0,
                            Mobile = mobile,
                            Status = $"Country code not found."
                        };

                        // Serialize OtpModel Object to Json
                        var jsonOtpModel = JsonConvert.SerializeObject(otp);
                        // End:

                        // Shafi: Return jsonOtpModel
                        return Json(jsonOtpModel);
                        // End:
                    }
                }
                else
                {
                    OtpModel otp = new OtpModel
                    {
                        OtpId = 0,
                        Mobile = mobile,
                        Status = $"User profile does not exists."
                    };

                    // Serialize OtpModel Object to Json
                    var jsonOtpModel = JsonConvert.SerializeObject(otp);
                    // End:

                    // Shafi: Return jsonOtpModel
                    return Json(jsonOtpModel);
                    // End:
                }
                // End:
            }
            // End:
            else
            {
                OtpModel otp = new OtpModel
                {
                    OtpId = 0,
                    Mobile = mobile,
                    Status = $"User name with {mobile} does not exists."
                };

                // Serialize OtpModel Object to Json
                var jsonOtpModel = JsonConvert.SerializeObject(otp);
                // End:

                // Shafi: Return jsonOtpModel
                return Json(jsonOtpModel);
                // End:
            }
        }

        [HttpPost]
        public async Task<JsonResult> VerifyLoginOtp(string mobile, int otpId, int otp)
        {
            // Shafi: Find otp record by otp record id
            var record = await AuditContext.UserLoginOTPVerification.Where(i => i.OtpID == otpId).FirstOrDefaultAsync();
            // End:

            if(record != null)
            {
                if(record.OTP == otp && record.Mobile == mobile)
                {
                    // Shafi: Update otpRecord Status
                    record.Status = "Verified";
                    record.OTPVerified = true;
                    AuditContext.Update(record);
                    await AuditContext.SaveChangesAsync();
                    // End:

                    // Shafi: Find User
                    var user = await UserManager.FindByNameAsync(mobile);
                    // End:

                    // Shafi: Sign in user
                    await SignInManager.SignInAsync(user, false);
                    // End:

                    // Shafi: Return jsonOtpModel
                    return Json("Success");
                    // End:
                }
                else
                {
                    // Shafi: Return jsonOtpModel
                    return Json("Fail");
                    // End:
                }
            }
            else
            {
                // Shafi: Return jsonOtpModel
                return Json("Invalid OTP Record");
                // End:
            }
        }

        public IActionResult Login()
        {
            return View();
        }

        public IActionResult LoginWithLink()
        {
            return View();
        }

        public IActionResult SecureData()
        {
            return View();
        }

        //[HttpPost]
        //public async Task<JsonResult> Encode(string encodeValue)
        //{
        //    //SHA1 sha1 = SHA1.Create();
        //    //byte[] hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(encodeValue));

        //   var token = await 

        //    return Json(hash);
        //}

        [HttpPost]
        public JsonResult Decode(string decodeValue)
        {
            return Json(decodeValue);
        }

        public async Task<JsonResult> GenerateLoginLink(string mobileEmail)
        {
            if(mobileEmail.Contains("@"))
            {
                // Shafi: Get user from email
                var getUser = await ApplicationContext.Users.Where(i => i.Email == mobileEmail).FirstOrDefaultAsync();
                // End:

                if(getUser != null)
                {
                    // Shafi: Find user
                    var user = await UserManager.FindByIdAsync(getUser.Id);
                    // End:

                    // Shafi: Generate Link
                    if(user != null)
                    {
                        await SignInManager.SignInAsync(user, false);
                        return Json("Success");
                    }
                    else
                    {
                        return Json("No such user find.");
                    }
                    // End:
                }
                else
                {
                    return Json("No such user find.");
                }

            }
            else
            {
                // Shafi: Get user from mobile
                var getUser = await ApplicationContext.Users.Where(i => i.PhoneNumber == mobileEmail).FirstOrDefaultAsync();
                // End:

                if (getUser != null)
                {
                    // Shafi: Find user
                    var user = await UserManager.FindByIdAsync(getUser.Id);
                    // End:

                    // Shafi: Generate Link
                    if (user != null)
                    {
                        await SignInManager.SignInAsync(user, false);
                        return Json("Success");
                    }
                    else
                    {
                        return Json("No such user find.");
                    }
                    // End:
                }
                else
                {
                    return Json("No such user find.");
                }
            }
        }
    }
}
