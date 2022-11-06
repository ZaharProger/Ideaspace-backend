using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ideaspace_backend.Models;
using Ideaspace_backend.Models.Entities;
using Ideaspace_backend.Models.Http.Params;
using Ideaspace_backend.Models.Http.Responses;
using Ideaspace_backend.Constants;

namespace Ideaspace_backend.Controllers
{
    [Route("ideaspace/api/[controller]")]
    [ApiController]
    public class PostsController : IdeaspaceController
    {
        public PostsController(IdeaspaceDBContext context)
        {
            this.context = context;
        }

        // POST: /Posts
        [HttpPost]
        public async Task<JsonResult> CreatePost([FromForm] CreatePostParams postData)
        {
            var isUserExist = false;
            if (CheckSession(ApiValues.SESSION_ID_KEY))
            {
                var parsedSessionId = long.Parse(HttpContext.Request.Cookies[ApiValues.SESSION_ID_KEY]);

                try
                {
                    var foundSession = await context.Sessions
                        .FirstAsync(session => session.session_id == parsedSessionId);

                    await context.Posts.AddAsync(new Post()
                    {
                        user_id = foundSession.user_id,
                        creation_date = postData.CreationDate,
                        creation_time = postData.CreationTime,
                        content = postData.Content
                    });
                    await context.SaveChangesAsync();
                    isUserExist = true;
                }
                catch (InvalidOperationException) 
                { }
            }

            return new JsonResult(new BaseResponse()
            {
                Result = isUserExist,
                Message = isUserExist? "" : ApiValues.SESSION_NOT_FOUND
            });
        }
    }
}
