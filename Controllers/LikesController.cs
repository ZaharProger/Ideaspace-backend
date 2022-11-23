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
    public class LikesController : IdeaspaceController
    {
        public LikesController(IdeaspaceDBContext context)
        {
            this.context = context;
        }

        // POST: /api/Likes?postId=
        [HttpPost]
        public async Task<JsonResult> AddLikeHandler(long postId)
        {
            var isSuccessfulу = false;
            var sessionId = CheckSession(ApiValues.SESSION_ID_KEY);

            if (sessionId != null)
            {
                try
                {
                    var foundSession = await context.Sessions
                        .FirstAsync(session => session.SessionId == sessionId);

                    await context.Likes.AddAsync(new Like()
                    {
                        UserId = foundSession.UserId,
                        PostId = postId
                    });
                    await context.SaveChangesAsync();

                    isSuccessfulу = true;
                }
                catch(InvalidOperationException)
                { }
            }

            return new JsonResult(new BaseResponse()
            {
                Result = isSuccessfulу,
                Message = sessionId != null? "" : ApiValues.SESSION_NOT_FOUND
            });
        }

        // DELETE: /api/Likes?postId=
        [HttpDelete]
        public async Task<JsonResult> RemoveLikeHandler(long postId)
        {
            var isSuccessfuly = false;
            var sessionId = CheckSession(ApiValues.SESSION_ID_KEY);

            if (sessionId != null)
            {
                try
                {
                    var foundSession = await context.Sessions
                        .FirstAsync(session => session.SessionId == sessionId);

                    var likeToRemove = await context.Likes
                        .FirstAsync(like => like.PostId == postId && like.UserId == foundSession.UserId);

                    context.Likes.Remove(likeToRemove);
                    await context.SaveChangesAsync();

                    isSuccessfuly = true;
                }
                catch (InvalidOperationException)
                { }
            }

            return new JsonResult(new BaseResponse()
            {
                Result = isSuccessfuly,
                Message = sessionId != null? "" : ApiValues.SESSION_NOT_FOUND
            });
        }
    }
}
