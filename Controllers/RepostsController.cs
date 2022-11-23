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
    public class RepostsController : IdeaspaceController
    {
        public RepostsController(IdeaspaceDBContext context)
        {
            this.context = context;
        }

        // POST: /api/Reposts?postId=
        [HttpPost]
        public async Task<JsonResult> AddRepostHandler(long postId, long date)
        {
            var isSuccessfulу = false;
            var sessionId = CheckSession(ApiValues.SESSION_ID_KEY);

            if (sessionId != null)
            {
                try
                {
                    var foundSession = await context.Sessions
                        .FirstAsync(session => session.SessionId == sessionId);

                    var currentDateTime = DateTime.Now;

                    await context.Reposts.AddAsync(new Repost()
                    {
                        UserId = foundSession.UserId,
                        PostId = postId,
                        RepostDate = date,
                        RepostTime = currentDateTime.Hour * 3600 + currentDateTime.Minute * 60
                    });
                    await context.SaveChangesAsync();

                    isSuccessfulу = true;
                }
                catch (InvalidOperationException)
                { }
            }

            return new JsonResult(new BaseResponse()
            {
                Result = isSuccessfulу,
                Message = sessionId != null ? "" : ApiValues.SESSION_NOT_FOUND
            });
        }

        // DELETE: /api/Reposts?postId=
        [HttpDelete]
        public async Task<JsonResult> RemoveRepostHandler(long postId)
        {
            var isSuccessfuly = false;
            var sessionId = CheckSession(ApiValues.SESSION_ID_KEY);

            if (sessionId != null)
            {
                try
                {
                    var foundSession = await context.Sessions
                        .FirstAsync(session => session.SessionId == sessionId);

                    var repostToRemove = await context.Reposts
                        .FirstAsync(repost => repost.PostId == postId && repost.UserId == foundSession.UserId);

                    context.Reposts.Remove(repostToRemove);
                    await context.SaveChangesAsync();

                    isSuccessfuly = true;
                }
                catch (InvalidOperationException)
                { }
            }

            return new JsonResult(new BaseResponse()
            {
                Result = isSuccessfuly,
                Message = sessionId != null ? "" : ApiValues.SESSION_NOT_FOUND
            });
        }
    }
}
