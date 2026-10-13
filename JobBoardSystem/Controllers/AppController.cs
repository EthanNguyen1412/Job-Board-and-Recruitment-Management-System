using System.Web;
using System.Web.Mvc;
using JobBoardSystem.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;

namespace JobBoardSystem.Controllers
{
    public class AppController : Controller
    {
        protected ApplicationDbContext Db
        {
            get { return HttpContext.GetOwinContext().Get<ApplicationDbContext>(); }
        }

        protected ApplicationUserManager UserManager
        {
            get { return HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>(); }
        }

        protected IAuthenticationManager AuthenticationManager
        {
            get { return HttpContext.GetOwinContext().Authentication; }
        }

        protected string CurrentUserId
        {
            get { return User.Identity.GetUserId(); }
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (User.Identity.IsAuthenticated)
            {
                var user = Db.Users.Find(CurrentUserId);
                if (user == null || !user.IsActive)
                {
                    AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
                    filterContext.Result = new RedirectResult("~/Account/Login");
                    return;
                }

                ViewBag.FullName = user.FullName;
            }

            base.OnActionExecuting(filterContext);
        }
    }
}
