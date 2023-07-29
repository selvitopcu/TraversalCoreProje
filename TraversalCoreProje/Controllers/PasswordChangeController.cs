using EntityLayer.Concrete;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MimeKit;
using Org.BouncyCastle.Bcpg;
using System.Threading.Tasks;
using TraversalCoreProje.Areas.Member.Models;
using TraversalCoreProje.Models;

namespace TraversalCoreProje.Controllers
{
    [AllowAnonymous]
    public class PasswordChangeController : Controller
    {
        private readonly UserManager<AppUser> _userManager;

        public PasswordChangeController(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public IActionResult ForgetPassword()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> ForgetPassword(ForgetPasswordViewModel forgetPasswordViewModel)
        {
            var user = await _userManager.FindByEmailAsync(forgetPasswordViewModel.Mail);
            string passwordResetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var passwordResetTokenLink = Url.Action("ResetPassword", "PasswordChange", new
            {
                userId = user.Id,
                token = passwordResetToken
            }, HttpContext.Request.Scheme);


            MimeMessage mimeMessage = new MimeMessage();
            //gönderen kişinin blgileri
            MailboxAddress mailboxAddress = new MailboxAddress("Admin", "maildeneme012@gmail.com");
            mimeMessage.From.Add(mailboxAddress);
            //Alacak kişi
            MailboxAddress mailboxAddressTo = new MailboxAddress("User", forgetPasswordViewModel.Mail);
            mimeMessage.To.Add(mailboxAddressTo);
            var bodyBuilder = new BodyBuilder();
            bodyBuilder.TextBody = passwordResetTokenLink;
            mimeMessage.Body = bodyBuilder.ToMessageBody();

            mimeMessage.Subject = "Şifre Değişiklik Talebi";
            SmtpClient client = new SmtpClient();
            client.Connect("smtp.gmail.com", 587, false);
            client.Authenticate("maildeneme012@gmail.com", "ogvfnjbwgdstvsqe");
            client.Send(mimeMessage);
            client.Disconnect(true);
            return View();

        }

        [HttpGet]
        public IActionResult ResetPassword(string userid,string token) 
        {
            TempData["userid"] = userid;
            TempData["token"] = token;
            return View(); 
        }
        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel resetPasswordViewModel) 
        {
            var userid = TempData["userid"];
            var token = TempData["token"];
            if(userid == null || token==null)
            {

            }
            var user = await _userManager.FindByIdAsync(userid.ToString());
            var result=await _userManager.ResetPasswordAsync(user,token.ToString(),resetPasswordViewModel.Password);
if(result.Succeeded)
            {
                return RedirectToAction("SignIn", "LogIn");
            }
            return View(); 
        }
    }
}
