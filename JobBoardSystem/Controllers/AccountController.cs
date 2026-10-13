using System.Linq;
using System.Web;
using System.Web.Mvc;
using JobBoardSystem.Infrastructure;
using JobBoardSystem.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;

namespace JobBoardSystem.Controllers
{
    public class AccountController : AppController
    {
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View(new LoginViewModel());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = UserManager.FindByEmail(model.Email);
            if (user != null && !user.IsActive)
            {
                ModelState.AddModelError("", "This account has been disabled.");
                return View(model);
            }

            var result = SignInManager.PasswordSignIn(model.Email, model.Password, model.RememberMe, shouldLockout: true);
            if (result == SignInStatus.Success)
            {
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                return RedirectToAction("Index", "Home");
            }

            if (result == SignInStatus.LockedOut)
            {
                ModelState.AddModelError("", "This account is temporarily locked.");
            }
            else
            {
                ModelState.AddModelError("", "Invalid email or password.");
            }

            return View(model);
        }

        [AllowAnonymous]
        public ActionResult Register()
        {
            return View(BuildRegisterModel(null));
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterViewModel model)
        {
            if (!RoleNames.Registerable.Contains(model.Role))
            {
                ModelState.AddModelError("Role", "Select a valid role.");
            }

            if (!ModelState.IsValid)
            {
                model.Roles = RoleOptions(model.Role);
                return View(model);
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName.Trim(),
                PhoneNumber = model.Phone,
                IsActive = true,
                EmailConfirmed = true,
                CreatedAt = System.DateTime.Now
            };
            var result = UserManager.Create(user, model.Password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error);
                }

                model.Roles = RoleOptions(model.Role);
                return View(model);
            }

            UserManager.AddToRole(user.Id, model.Role);
            SignInManager.SignIn(user, isPersistent: false, rememberBrowser: false);
            if (model.Role == RoleNames.Employer)
            {
                return RedirectToAction("Company", "Employer");
            }

            return RedirectToAction("Profile", "Candidate");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        public ActionResult Edit()
        {
            var user = UserManager.FindById(CurrentUserId);
            return View(new EditAccountViewModel
            {
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.PhoneNumber
            });
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(EditAccountViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = UserManager.FindById(CurrentUserId);
            user.FullName = model.FullName.Trim();
            user.PhoneNumber = model.Phone;
            if (!string.Equals(user.Email, model.Email, System.StringComparison.OrdinalIgnoreCase))
            {
                user.Email = model.Email;
                user.UserName = model.Email;
            }

            var result = UserManager.Update(user);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error);
                }

                return View(model);
            }

            TempData["Message"] = "Your account information has been updated.";
            return RedirectToAction("Edit");
        }

        [Authorize]
        public ActionResult ChangePassword()
        {
            return View(new ChangePasswordViewModel());
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = UserManager.ChangePassword(CurrentUserId, model.CurrentPassword, model.NewPassword);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error);
                }

                return View(model);
            }

            var user = UserManager.FindById(CurrentUserId);
            SignInManager.SignIn(user, isPersistent: false, rememberBrowser: false);
            TempData["Message"] = "Your password has been changed.";
            return RedirectToAction("Edit");
        }

        private ApplicationSignInManager SignInManager
        {
            get { return HttpContext.GetOwinContext().Get<ApplicationSignInManager>(); }
        }

        private RegisterViewModel BuildRegisterModel(string role)
        {
            return new RegisterViewModel { Roles = RoleOptions(role) };
        }

        private static System.Collections.Generic.IList<SelectListItem> RoleOptions(string selected)
        {
            return RoleNames.Registerable.Select(role => new SelectListItem
            {
                Value = role,
                Text = RoleNames.Display(role),
                Selected = role == selected
            }).ToList();
        }
    }
}
