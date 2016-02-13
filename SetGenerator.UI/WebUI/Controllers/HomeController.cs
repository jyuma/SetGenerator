using SetGenerator.Service;
using SetGenerator.WebUI.ViewModels;
using SetGenerator.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Newtonsoft.Json;
using Constants = SetGenerator.Service.Constants;

namespace SetGenerator.WebUI.Controllers
{
    public class HomeController : Controller
    {
        private const int DefaultBandId = 1;
        private readonly User _currentUser;

        private readonly IAccount _account;

        public HomeController(IAccount account)
        {
            _account = account;
        }

        // for testing
        public ActionResult About()
        {
            return View(LoadLogonViewModel());
        }

        // for testing
        public ActionResult Contact()
        {
            return View(LoadLogonViewModel());
        }

        public ActionResult Index(LogonViewModel model)
        {
            return View(LoadLogonViewModel());
        }

        public string GetCurrentSessionUser()
        {
            return System.Web.HttpContext.Current.User.Identity.Name;
        }

        // Bands

        [HttpPost]
        public ActionResult SetCurrentBand(int bandId)
        {
            Session["BandId"] = bandId;
            return Json(JsonRequestBehavior.AllowGet);
        }

        // Login 

        [HttpPost]
        public ActionResult Login(LogonViewModel model)
        {
            Response.Cache.SetCacheability(HttpCacheability.NoCache);

            // admin login
            if (model.UserName == ConfigurationManager.AppSettings["AdminUserName"])
            {
                var msg = AttemptToLoginAdmin(model.Password);

                if (msg == null)
                {
                    CreateLoginAuthenticationTicket(model.UserName);
                    return Json(new { success = true });
                }
                return Json(new { success = false, messages = msg });
            }

            // standard login

            var result = _account.ValidateLogin(model.UserName, model.Password);

            if (result == null)
            {
                // credentials are good, authenticate the user
                CreateLoginAuthenticationTicket(model.UserName);
                Session["UserName"] = model.UserName;
                var u = _account.GetUserByUserName(model.UserName);
                if (u != null) Session["UserId"] = u.Id;

                if (model.RememberMe)
                    CreateRememberMeCookie(model.UserName);

                return Json(new { success = true });
            }
            // if we got this far, something failed, return a list of problems
            var msgs = result.ToArray();
            return Json(new { success = false, messages = msgs });
        }

        [HttpPost]
        public JsonResult SetDisabled(string uname, bool disabled)
        {
            _account.UpdateUserDisabled(uname, disabled);
            var msgs = new List<string>();
            return Json(new { success = false, messages = msgs });
        }

        private void CreateLoginAuthenticationTicket(string username)
        {
            var roles = (username == "admin") ? "Admin" : ""; // _account.GetUserRoles(username);
            var cookieTimeoutMinutes = Convert.ToInt32(ConfigurationManager.AppSettings["CookieTimeoutMinutes"]);
            var ticket = new FormsAuthenticationTicket(1, username, DateTime.Now,
                                                       DateTime.Now.AddMinutes(cookieTimeoutMinutes), false, roles);
            var enticket = FormsAuthentication.Encrypt(ticket);
            var cname = FormsAuthentication.FormsCookieName;

            Response.Cookies.Add(new HttpCookie(cname, enticket));
        }

        // Admin Login
        private List<string> AttemptToLoginAdmin(string password)
        {
            var msgs = new List<string>();
            var count = Convert.ToInt32(Session["AdminLoginAttempts"]);
            var maxattempts = Convert.ToInt32(ConfigurationManager.AppSettings["MaxAdminLoginAttempts"]);
            //var ipaddress = System.Web.HttpContext.Current.Request.UserHostAddress;
            //var isdenied = (_account.IsAdminAccessDenied(ipaddress));

            //if (isdenied)
            //{
            //    msgs.Add("Admin access revoked.  Contact site webmaster.");
            //    return msgs;
            //}

            if (count < maxattempts)
            {
                var adminpassword = ConfigurationManager.AppSettings["AdminPassword"];
                if (password == adminpassword)
                {
                    CreateAdminCookie();
                    return null;
                }
                count++;
                Session["AdminLoginAttempts"] = count;
            }

            if (count < maxattempts)
            {
                msgs.Add("Invalid password.");
            }
            else
            {
                //_account.AddAdminAccessDenied(ipaddress);
                msgs.Add("Maximum Admin login attempts has been exceeded.");
            }

            return msgs;
        }

        // Register

        [HttpPost]
        public ActionResult Register(RegisterViewModel model)
        {
            var result = _account.ValidateRegister(model.UserName, model.EmailAddress, model.Password,
                                                           model.ConfirmPassword);
            if (result == null)
            {
                var ipaddress = System.Web.HttpContext.Current.Request.UserHostAddress;
                var browserinfo = System.Web.HttpContext.Current.Request.Browser.Browser + " Version " +
                                  System.Web.HttpContext.Current.Request.Browser.Version;

                // Create the user
                // TODO: Figure out how to default this to the appropriate BandID for the given user (currently using a constant)
                var newid = _account.CreateUser(model.UserName, model.EmailAddress, model.Password, DefaultBandId, ipaddress, browserinfo);

                Session["UserId"] = newid;
                //_account.AddUserRole(model.UserName, Constants.RoleIdCustomer);
                var emailBody = CreateEmailBody(model.UserName, null, model.EmailAddress, null, model.Password, Constants.EmailContextRegister);
                var emailError = SendEmail(emailBody);
                FormsAuthentication.SetAuthCookie(model.UserName, true);

                return Json(new { success = true, EmailError = emailError });
            }

            // If we got this far, something failed, return a list of problems
            var msgs = result.ToArray();
            return Json(new { success = false, messages = msgs });
        }

        // POST: /Home/LogOff

        [AllowAnonymous]
        public ActionResult LogOff()
        {
            FormsAuthentication.SignOut();
            ClearAdminCookie();
            return RedirectToAction("Index", "Home");
        }

        // profile 

        public ActionResult GetProfileData()
        {
            var vm = new ProfileEditViewModel();
            if (System.Web.HttpContext.Current.User.Identity.Name != null)
            {
                var username = System.Web.HttpContext.Current.User.Identity.Name;
                if (username.Length > 0)
                {
                    var u = _account.GetUserByUserName(username);
                    if (u != null) vm = LoadProfileEditViewModel(u);
                }
            }
            return Json(vm, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SaveProfile(string userProfile)
        {
            var profile = JsonConvert.DeserializeObject<ProfileEditViewModel>(userProfile);
            var result = _account.ValidateProfile(profile.OldUserName, profile.OldEmailAddress);

            if (result == null)
            {
                if (Session["UserId"] != null)
                {
                    var userid = Convert.ToInt32(Session["UserId"].ToString());
                    //_account.UpdateUserProfile(userid, profile.NewUserName, profile.NewEmailAddress);
                    Session["UserName"] = profile.NewUserName;
                    FormsAuthentication.SetAuthCookie(profile.NewUserName, true);
                    var emailBody = CreateEmailBody(profile.NewUserName, profile.OldUserName, profile.NewEmailAddress, profile.OldEmailAddress, null, Constants.EmailContextProfileEdited);
                    var emailError = SendEmail(emailBody);
                    return Json(new { success = true, EmailError = emailError });
                }
                result = new List<string> { "There was a problem updating your profile." };
                return Json(new { success = false, messages = result.ToArray() });
            }
            // If we got this far, something failed, return a list of problems
            var msgs = result.ToArray();
            return Json(new { success = false, messages = msgs });
        }

        public ActionResult SaveNewPassword(string userProfile)
        {
            var profile = JsonConvert.DeserializeObject<ProfileEditViewModel>(userProfile);
            var result = _account.ValidateChangePassword(profile.NewPassword, profile.ConfirmNewPassword);

            if (result == null)
            {
                if (Session["UserId"] != null)
                {
                    var userid = Convert.ToInt32(Session["UserId"].ToString());
                    //_account.UpdateUserPassword(userid, profile.NewPassword);
                    var emailBody = CreateEmailBody(null, profile.OldUserName, null, profile.OldEmailAddress, profile.NewPassword, Constants.EmailContextPasswordChanged);
                    var emailError = SendEmail(emailBody);
                    return Json(new { success = true, EmailError = emailError });
                }
                result = new List<string> { "There was a problem updating your password." };
                return Json(new { success = false, messages = result.ToArray() });
            }
            // If we got this far, something failed, return a list of problems
            var msgs = result.ToArray();
            return Json(new { success = false, messages = msgs });
        }

        // private methods

        private static ProfileEditViewModel LoadProfileEditViewModel(User u)
        {
            var vm = new ProfileEditViewModel
            {
                OldUserName = u.UserName,
                OldEmailAddress = u.Email
            };
            return vm;
        }

        private LogonViewModel LoadLogonViewModel()
        {
            var model = new LogonViewModel
            {
                UserName = GetCurrentSessionUser()
            };


            return model;
        }

        private void CreateRememberMeCookie(string username)
        {
            var cookie = Request.Cookies["userName"];
            if (cookie != null)
                cookie.Value = username;
            else
            {
                cookie = new HttpCookie("userName") { Value = username };
                Response.Cookies.Add(cookie);
            }

            // make sure there are no "admin" cookies lingering
            ClearAdminCookie();
        }

        private void ClearAdminCookie()
        {
            var cookie = Request.Cookies["admin"];
            if (cookie == null) return;
            cookie.Expires = DateTime.Now.AddDays(-1);
            Response.Cookies.Add(cookie);
        }

        private void CreateAdminCookie()
        {
            var cookie = Request.Cookies["admin"];
            if (cookie != null)
                cookie.Value = "admin";
            else
            {
                cookie = new HttpCookie("admin") { Value = "admin" };
                Response.Cookies.Add(cookie);
                cookie.Expires = DateTime.Now.AddHours(5);
            }
        }

        private static string SendEmail(string emailBody)
        {
            var sendError = "";

            try
            {
                var eMail = new MailMessage
                {
                    IsBodyHtml = true,
                    Body = emailBody,
                    From = new MailAddress(ConfigurationManager.AppSettings["SmtpCredentialsUserName"]),
                    Subject = ConfigurationManager.AppSettings["EmailSubject"]
                };

                eMail.To.Add(ConfigurationManager.AppSettings["AdminEmailAddress"]);

                using (var client = new SmtpClient())
                {
                    client.Credentials = new NetworkCredential(
                        ConfigurationManager.AppSettings["SmtpCredentialsUserName"],
                        ConfigurationManager.AppSettings["SmtpCredentialsPassword"]);

                    client.Host = ConfigurationManager.AppSettings["SmtpHost"];
                    client.Send(eMail);
                }
            }
            catch (Exception e)
            {
                sendError = e.InnerException.ToString();
            }
            return sendError;
        }

        private static string CreateEmailBody(string newUserName, string oldUserName, string newEmailAddress, string oldEmailAddress, string password, string emailContext)
        {
            var body = new StringBuilder();

            body.Append("<b>This is an automated message sent from predictabull.beefbooster.com.</b>");
            body.Append("<br>");
            body.Append("<br>");
            body.Append("Date: " + DateTime.Now.ToLongDateString());
            body.Append("<br>");
            body.Append("Time: " + DateTime.Now.ToLongTimeString());
            body.Append("<br>");
            body.Append("<br>");
            body.Append(emailContext + ":");
            body.Append("<br>");
            body.Append("<br>");

            switch (emailContext)
            {
                case Constants.EmailContextRegister:
                    {
                        body.Append("UserName: " + newUserName);
                        body.Append("<br>");
                        body.Append("Email: " + newEmailAddress);
                        body.Append("<br>");
                        body.Append("Password: " + password);
                        body.Append("<br>");
                    }
                    break;
                case Constants.EmailContextProfileEdited:
                    {
                        if (newUserName != oldUserName)
                        {
                            body.Append("Old UserName: " + oldUserName);
                            body.Append("<br>");
                            body.Append("New UserName: " + newUserName);
                            body.Append("<br>");
                        }
                        else
                        {
                            body.Append("UserName: " + oldUserName);
                            body.Append("<br>");
                        }

                        if (newEmailAddress != oldEmailAddress)
                        {
                            body.Append("Old Email: " + oldEmailAddress);
                            body.Append("<br>");
                            body.Append("New Email: " + newEmailAddress);
                            body.Append("<br>");
                        }
                        else
                        {
                            body.Append("Email: " + oldEmailAddress);
                            body.Append("<br>");
                        }
                    }
                    break;
                case Constants.EmailContextPasswordChanged:
                    {
                        body.Append("UserName: " + oldUserName);
                        body.Append("<br>");
                        body.Append("Email: " + oldEmailAddress);
                        body.Append("<br>");
                        body.Append("New Password: " + password);
                        body.Append("<br>");
                    }
                    break;
            }

            return body.ToString();
        }
    }
}
