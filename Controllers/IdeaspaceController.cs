using Ideaspace_backend.Models;
using Microsoft.AspNetCore.Mvc;

namespace Ideaspace_backend.Controllers
{
    public class IdeaspaceController : ControllerBase
    {
        protected IdeaspaceDBContext context;

        protected bool CheckSession(string cookieKey)
        {
            var isSessionValid = true;
            if (HttpContext.Request.Cookies[cookieKey] != null)
            {
                try
                {
                    long.Parse(HttpContext.Request.Cookies[cookieKey]);
                }
                catch (FormatException)
                {
                    isSessionValid = false;
                }
            }
            else
            {
                isSessionValid = false;
            }

            return isSessionValid;
        }
    }
}
