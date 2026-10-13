using System.Web.Mvc;

namespace JobBoardSystem.Infrastructure
{
    public class AppAuthorizeAttribute : AuthorizeAttribute
    {
        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            if (filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                filterContext.Result = new ViewResult
                {
                    ViewName = "~/Views/Shared/Denied.cshtml"
                };
                return;
            }

            base.HandleUnauthorizedRequest(filterContext);
        }
    }
}
