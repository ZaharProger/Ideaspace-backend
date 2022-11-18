using Ideaspace_backend.Models;
using Microsoft.AspNetCore.Mvc;

namespace Ideaspace_backend.Controllers
{
    public class IdeaspaceController : ControllerBase
    {
        protected IdeaspaceDBContext context;

        protected long? CheckSession(string cookieKey)
        {
            long? sessionId = null;
            if (HttpContext.Request.Cookies[cookieKey] != null)
            {
                try
                {
                    sessionId = long.Parse(HttpContext.Request.Cookies[cookieKey]);
                }
                catch (FormatException)
                {}
            }

            return sessionId;
        }
    }
}
