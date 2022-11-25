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
        public async Task<JsonResult> GetPostsHandler([FromQuery] GetParams getPostParams)
        {
            var foundPostsArray = Array.Empty<Post>();
            var foundPosts = Array.Empty<Post>();
            var sessionId = CheckSession(ApiValues.SESSION_ID_KEY);

            if (sessionId != null)
            {
                try
                {
                    var foundSession = await context.Sessions
                        .FirstAsync(session => session.SessionId == sessionId);

                    var currentUser = await context.Users
                        .FirstAsync(user => user.UserId == foundSession.UserId);
                    var foundUser = await context.Users
                        .FirstAsync(user => user.UserLogin.Equals(getPostParams.Key));

                    var sameUsers = currentUser.UserId == foundUser.UserId;

                    foundPosts = await GetPosts(sameUsers? currentUser : foundUser, sameUsers? null : currentUser);
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
                Message = sessionId != null? "" : ApiValues.SESSION_NOT_FOUND,
                Data = foundPostsArray
            });
        }

        private async Task<Post[]> GetPosts(User? foundUser, User? currentUser=null)
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

            var repostedPosts = context.Reposts
                .Where(repost => currentUser == null ? repost.UserId == foundUser.UserId : 
                repost.UserId == foundUser.UserId || repost.UserId == currentUser.UserId)
                .Join(context.Posts, repost => repost.PostId, post => post.PostId, (repost, post) => new Post()
                {
                    ParentUserId = repost.UserId,
                    UserId = post.UserId,
                    PostId = post.PostId,
                    CreationDate = repost.RepostDate,
                    CreationTime = repost.RepostTime,
                    Content = post.Content
                })
                .Join(context.Users, post => post.UserId, user => user.UserId, (post, user) => new Post()
                {
                    ParentUserId = post.ParentUserId,
                    IsReposted = true,
                    UserLogin = user.UserLogin,
                    PostId = post.PostId,
                    CreationDate = post.CreationDate,
                    CreationTime = post.CreationTime,
                    Content = post.Content
                });

            var likedPosts = context.Likes
                .Where(like => like.UserId == (currentUser == null ? foundUser.UserId : currentUser.UserId))
                .Join(context.Posts, like => like.PostId, post => post.PostId, (like, post) => new Post()
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
                    IsLiked = true,
                    CreationDate = post.CreationDate,
                    CreationTime = post.CreationTime,
                    Content = post.Content
                });

            List<Post>? currentUserReposts = null;
            if (currentUser != null)
            {
                currentUserReposts = repostedPosts
                    .Where(repostedPost => repostedPost.ParentUserId == currentUser.UserId)
                    .ToList();

                repostedPosts = repostedPosts
                    .Where(repostedPost => repostedPost.ParentUserId == foundUser.UserId);
            }

            var repostedPostsList = await repostedPosts.ToListAsync();
            var likedPostsList = await likedPosts.ToListAsync();

            SyncPosts(foundPosts, currentUserReposts, likedPostsList);
            SyncPosts(repostedPostsList, currentUserReposts, likedPostsList);

            foundPosts.AddRange(repostedPostsList);
            if (currentUser == null)
            {
                foundPosts.AddRange(likedPostsList);
            }

            foundPosts.ForEach(post =>
            {
                post.LikesAmount = context.Likes
                    .Count(like => like.PostId == post.PostId);

                post.RepostsAmount = context.Reposts
                    .Count(repost => repost.PostId == post.PostId);
            });

            return foundPosts
                .OrderByDescending(post => post.CreationDate + post.CreationTime)
                .ToArray();
        }

        private void SyncPosts(List<Post>? mainPosts, List<Post>? repostsToSync, List<Post>? likesToSync)
        {
            mainPosts.ForEach(mainPost =>
            {
                if (repostsToSync != null)
                {
                    mainPost.IsReposted = repostsToSync
                        .Exists(repostedPost => repostedPost.PostId == mainPost.PostId);
                }

                mainPost.IsLiked = likesToSync
                    .Exists(likedPost => likedPost.PostId == mainPost.PostId);

                if (mainPost.IsLiked)
                {
                    likesToSync.RemoveAll(likedPost => likedPost.PostId == mainPost.PostId);
                }
            });
        }
    }
}
