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
            var sessionId = CheckSession(ApiValues.SESSION_ID_KEY);
            if (sessionId != null)
            {
                try
                {
                    var foundSession = await context.Sessions
                        .FirstAsync(session => session.SessionId == sessionId);

                    await context.Posts.AddAsync(new Post()
                    {
                        UserId = foundSession.UserId,
                        CreationDate = postData.CreationDate,
                        CreationTime = postData.CreationTime,
                        Content = postData.Content
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

        // GET: /Posts?userLogin=
        [HttpGet]
        public async Task<JsonResult> GetUserPosts (string userLogin, int limit=30)
        {
            Post[]? foundPostsPortionArray = Array.Empty<Post>();
            Post[]? foundPostsArray = Array.Empty<Post>();
            var sessionId = CheckSession(ApiValues.SESSION_ID_KEY);

            if (sessionId != null)
            {
                try
                {
                    var foundUser = await context.Users
                        .FirstAsync(user => user.UserLogin.Equals(userLogin));

                    var foundPosts = await context.Posts
                        .Where(post => post.UserId == foundUser.UserId)
                        .Select(foundPost => new Post()
                        {
                            UserLogin = foundUser.UserLogin,
                            PostId = foundPost.PostId,
                            CreationDate = foundPost.CreationDate,
                            CreationTime = foundPost.CreationTime,
                            Content = foundPost.Content
                        })
                        .ToListAsync();

                    foundPosts.AddRange(await context.Reposts
                        .Where(repost => repost.UserId == foundUser.UserId)
                        .Join(context.Posts, repost => repost.PostId, post => post.PostId, (repost, post) => new Post()
                        {
                            UserId = post.UserId,
                            PostId = post.PostId,
                            CreationDate = post.CreationDate,
                            CreationTime = post.CreationTime,
                            Content = post.Content
                        })
                        .Join(context.Users, post => post.UserId, user => user.UserId, (post, user) => new Post()
                        {
                            UserLogin = user.UserLogin,
                            PostId = post.PostId,
                            CreationDate = post.CreationDate,
                            CreationTime = post.CreationTime,
                            Content = post.Content
                        })
                        .ToListAsync());

                    foundPostsArray = foundPosts.ToArray();
                    var foundPostsPortion = new List<Post>();
                    for (int i = 0; i < limit && i < foundPostsArray.Length; ++i)
                    {
                        foundPostsPortion.Add(foundPostsArray[i]);
                    }

                    foundPostsPortionArray = foundPostsPortion.ToArray();
                }
                catch (InvalidOperationException)
                { }
            }

            return new JsonResult(new PaginationResponse<Post>()
            {
                Result = foundPostsPortionArray.Length != 0,
                IsOver = limit >= foundPostsArray.Length,
                Message = "",
                Data = foundPostsPortionArray
            });
        }
    }
}
