﻿using System.Security.Claims;
using System.Threading.Tasks;
using Grp.L2PSite.MobileApp.Models;
using Grp.L2PSite.MobileApp.Services;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Mvc;
using System.Threading;
using System;
using Microsoft.AspNet.Http;
using static Grp.L2PSite.MobileApp.Services.Tools;

namespace Grp.L2PSite.MobileApp.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly ISmsSender _smsSender;
        private readonly ApplicationDbContext _applicationDbContext;
        private static bool _databaseChecked;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IEmailSender emailSender,
            ISmsSender smsSender,
            ApplicationDbContext applicationDbContext)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _smsSender = smsSender;
            _applicationDbContext = applicationDbContext;
        }

        //
        // GET: /Account/Login
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            Tools.getAndSetUserToken(Request.Cookies, Context);
            if (!Tools.hasCookieToken)
            {
                //Init the Auth Process
                ViewData["L2PURL"] = L2PAPIClient.AuthenticationManager.StartAuthenticationProcessAsync().Result;                
            }
            else
            {
                return RedirectToAction(nameof(HomeController.MyCourses), "Home");
            }
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            EnsureDatabaseCreated(_applicationDbContext);
            ViewData["ReturnUrl"] = returnUrl;
             
            // Wait for authentication
            bool done = false;
            while (!done)  // so far, not authenticated
            {
                // Just wait 5 seconds - this is the recommended querying time for OAuth by ITC
                Thread.Sleep(5000);
                await L2PAPIClient.AuthenticationManager.CheckAuthenticationProgressAsync();

                done = (L2PAPIClient.AuthenticationManager.getState() == L2PAPIClient.AuthenticationManager.AuthenticationState.ACTIVE);
                if (!done)
                {
                    Console.WriteLine("App not authenticated right now...");
                }
                else
                {
                    //Add a Cookie
                    CookieOptions cOptions = new CookieOptions();
                    cOptions.Expires = DateTime.MaxValue;
                    Response.Cookies.Append("CRTID", Encryptor.Encrypt(L2PAPIClient.Config.getRefreshToken()), cOptions);
                    Response.Cookies.Append("CRAID", Encryptor.Encrypt(L2PAPIClient.Config.getAccessToken()), cOptions);
                    
                    //Set logged in to true
                    Context.Session.Set("LoggedIn", Tools.ObjectToByteArray(LoginStatus.LoggedIn));
                    return RedirectToLocal(returnUrl);
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult LogOff()
        {
            Context.Session.SetInt32("LoggedIn", 0);
            Response.Cookies.Delete("CRTID");
            Response.Cookies.Delete("CRAID");
            return RedirectToAction(nameof(HomeController.MyCourses), "Home");
        }

        #region Helpers

        // The following code creates the database and schema if they don't exist.
        // This is a temporary workaround since deploying database through EF migrations is
        // not yet supported in this release.
        // Please see this http://go.microsoft.com/fwlink/?LinkID=615859 for more information on how to do deploy the database
        // when publishing your application.
        private static void EnsureDatabaseCreated(ApplicationDbContext context)
        {
            if (!_databaseChecked)
            {
                _databaseChecked = true;
                context.Database.EnsureCreated();
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        private async Task<ApplicationUser> GetCurrentUserAsync()
        {
            return await _userManager.FindByIdAsync(Context.User.GetUserId());
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction(nameof(HomeController.MyCourses), "Home");
            }
        }

        #endregion
    }
}
