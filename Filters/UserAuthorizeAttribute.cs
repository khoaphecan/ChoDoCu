using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ThriftSession = ThriftShop.Common.SessionExtensions;

namespace ThriftShop.Filters
{
    public class UserAuthorizeAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var httpContext = context.HttpContext;
            var session = httpContext.Session;
            var user = httpContext.User;

            bool isAuthenticated = user.Identity?.IsAuthenticated == true || ThriftSession.IsLoggedIn(session);

            // Tự động bù đắp (re-hydrate) session từ cookie claims nếu vừa restart server
            if (user.Identity?.IsAuthenticated == true && !ThriftSession.IsLoggedIn(session))
            {
                var idClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(idClaim, out int uid))
                {
                    session.SetInt32(ThriftSession.SessionIdKey, uid);
                    session.SetString(ThriftSession.SessionNameKey, user.Identity?.Name ?? "");
                    session.SetString(ThriftSession.SessionEmailKey, user.FindFirst(ClaimTypes.Email)?.Value ?? "");
                    session.SetString(ThriftSession.SessionRoleKey, user.FindFirst(ClaimTypes.Role)?.Value ?? "customer");
                    session.SetString(ThriftSession.SessionAvatarKey, user.FindFirst("Avatar")?.Value ?? "");
                }
            }

            if (!isAuthenticated)
            {
                var req = httpContext.Request;
                bool isAjax = req.Headers["X-Requested-With"] == "XMLHttpRequest"
                           || req.Headers["Accept"].ToString().Contains("application/json")
                           || req.ContentType?.Contains("application/json") == true;

                if (isAjax)
                {
                    context.Result = new JsonResult(new
                    {
                        success = false,
                        requireLogin = true,
                        message = "Phiên làm việc hết hạn hoặc bạn chưa đăng nhập!"
                    })
                    {
                        StatusCode = 401
                    };
                }
                else
                {
                    string returnUrl = req.Path + req.QueryString;
                    if (string.IsNullOrWhiteSpace(returnUrl))
                    {
                        returnUrl = "/";
                    }

                    if (context.Controller is Controller controller)
                    {
                        controller.TempData["ErrorMessage"] = "Vui lòng đăng nhập tài khoản để tiếp tục!";
                    }

                    context.Result = new RedirectToActionResult("Login", "Account", new { returnUrl });
                }
                return;
            }

            base.OnActionExecuting(context);
        }
    }
}
