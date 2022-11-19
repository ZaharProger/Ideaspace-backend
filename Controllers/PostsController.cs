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
        public async Task<JsonResult> GetPostsHandler([FromQuery] GetPostParams getPostParams)
        {
            var foundPostsArray = Array.Empty<Post>();
            var foundPosts = Array.Empty<Post>();
            var sessionId = CheckSession(ApiValues.SESSION_ID_KEY);

            if (sessionId != null)
            {
                try
                {
                    var foundUser = await context.Users
                        .FirstAsync(user => user.UserLogin.Equals(getPostParams.Key));

                    foundPosts = getPostParams.Likes == 1 ? await GetLikedPosts(foundUser) : await GetPosts(foundUser);
                    var foundDataPortion = new List<Post>();
                    for (int i = 0; i < getPostParams.Limit && i < foundPosts.Length; ++i)
                    {
                        foundDataPortion.Add(foundPosts[i]);
                    }

                    foundPostsArray = foundDataPortion.ToArray();
                }
                catch (InvalidOperationException)
                { }
            }

            return new JsonResult(new PaginationResponse<Post>()
            {
                Result = foundPostsArray.Length != 0,
                IsOver = getPostParams.Limit >= foundPosts.Length,
                Message = "",
                Data = foundPostsArray
            });
        }

        private async Task<Post[]> GetPosts(User? foundUser)
        {
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

            return foundPosts.ToArray();
        }

        private async Task<Post[]> GetLikedPosts(User? foundUser)
        {
            var foundPosts = await context.Likes
                .Where(like => like.UserId == foundUser.UserId)
                .Join(context.Posts, like => like.PostId, post => post.PostId, (like, post) => new Post()
                {
                    UserLogin = foundUser.UserLogin,
                    PostId = post.PostId,
                    CreationDate = post.CreationDate,
                    CreationTime = post.CreationTime,
                    Content = post.Content
                })
                .ToListAsync();

            return foundPosts.ToArray();
        }
    }
}
