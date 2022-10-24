using Ideaspace_backend.Models;
using Microsoft.AspNetCore.Mvc;

namespace Ideaspace_backend.Controllers
{
    public class IdeaspaceController : ControllerBase
    {
        protected readonly IdeaspaceDBContext context;

        public IdeaspaceController(IdeaspaceDBContext context)
        {
            this.context = context;
        }
    }
}
