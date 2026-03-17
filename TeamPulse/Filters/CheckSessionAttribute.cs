using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Http;

namespace TeamPulse.Filters
{
    // Ye hamara custom Security Guard hai
    public class CheckSessionAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // Check karo ki session me UserID hai ya nahi (matlab login hai ya nahi)
            var userId = context.HttpContext.Session.GetInt32("UserID");

            if (userId == null)
            {
                // Agar login nahi hai, to chup-chap Account/Login par bhej do
                context.Result = new RedirectToActionResult("Login", "Account", null);
            }

            base.OnActionExecuting(context);
        }
    }
}