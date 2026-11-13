using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ThriftSession = ThriftShop.Common.SessionExtensions;

namespace ThriftShop.Filters
{
    public class AdminAuthorizeAttribute : ActionFilterAttribute
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

            var req = httpContext.Request;
            bool isAjax = req.Headers["X-Requested-With"] == "XMLHttpRequest"
                       || req.Headers["Accept"].ToString().Contains("application/json")
                       || req.ContentType?.Contains("application/json") == true;

            // 1. Chưa đăng nhập
            if (!isAuthenticated)
            {
                if (isAjax)
                {
                    context.Result = new JsonResult(new
                    {
                        success = false,
                        requireLogin = true,
                        message = "Vui lòng đăng nhập tài khoản Quản Trị Viên!"
                    })
                    {
                        StatusCode = 401
                    };
                }
                else
                {
                    string returnUrl = req.Path + req.QueryString;
                    if (context.Controller is Controller controller)
                    {
                        controller.TempData["ErrorMessage"] = "Vui lòng đăng nhập tài khoản Ban Quản Trị (admin@thriftshop.vn) để truy cập!";
                    }
                    context.Result = new RedirectToActionResult("Login", "Account", new { returnUrl });
                }
                return;
            }

            // 2. Đã đăng nhập nhưng không phải Admin
            bool isAdmin = user.IsInRole("admin") 
                        || user.FindFirst(ClaimTypes.Email)?.Value == "admin@thriftshop.vn"
                        || ThriftSession.IsAdmin(session);

            if (!isAdmin)
            {
                if (isAjax)
                {
                    context.Result = new JsonResult(new
                    {
                        success = false,
                        message = "CẢNH BÁO: Bạn không có quyền Quản Trị Viên!"
                    })
                    {
                        StatusCode = 403
                    };
                }
                else
                {
                    if (context.Controller is Controller controller)
                    {
                        controller.TempData["ErrorMessage"] = "CẢNH BÁO: Bạn không có quyền Quản Trị Viên! Chỉ tài khoản Ban Quản Trị (admin@thriftshop.vn) mới được phép truy cập.";
                    }
                    context.Result = new RedirectToActionResult("Index", "Home", null);
                }
                return;
            }

            base.OnActionExecuting(context);
        }
    }
}
