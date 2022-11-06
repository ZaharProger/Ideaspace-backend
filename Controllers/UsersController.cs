using System.Text;
using System.Security.Cryptography;
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
    public class UsersController : IdeaspaceController
    {
        private readonly SHA256 passwordEncryptor;
        private readonly Dictionary<ApiEnum, Func<string, Task<UserDataResponse>>> usersFuncs;

        public UsersController(IdeaspaceDBContext context)
        {
            this.context = context;
            passwordEncryptor = SHA256.Create();
            usersFuncs = new Dictionary<ApiEnum, Func<string, Task<UserDataResponse>>>()
            {
                { ApiEnum.SESSION_ID, GetUserBySessionId },
                { ApiEnum.USER_ID, GetUserById },
                { ApiEnum.SEARCH_STRING, GetUsersBySearchString }
            };
        }

        // POST: /Users
        [HttpPost]
        public async Task<IActionResult> AuthRegHandler([FromForm] AuthParams authParams)
        {
            var response = authParams.RequestType == ApiValues.AUTH_REQUEST_TYPE ?
                await AuthorizeUser(authParams) : await RegisterUser(authParams);

            return new JsonResult(response);
        }

        private async Task<BaseResponse> AuthorizeUser(AuthParams authParams)
        {
            var foundUser = await context.Users
                .Where(user => user.user_login.Equals(authParams.Login))
                .ToArrayAsync();

            var passwordByteArray = Encoding.UTF8.GetBytes(authParams.Password);
            var hashedPassword = passwordEncryptor.ComputeHash(passwordByteArray);

            var messageForClient = ApiValues.AUTH_WRONG_LOGIN;
            var incorrectFieldType = ApiValues.LOGIN_FIELD_TYPE;
            var sessionId = -1L;

            if (foundUser.Length != 0)
            {
                if (foundUser[0].user_password.SequenceEqual(hashedPassword))
                {
                    messageForClient = ApiValues.AUTH_SUCCESS;
                    sessionId = (long)DateTime.Now.Subtract(ApiValues.UNIX_START_DATE).TotalSeconds;

                    await context.Sessions.AddAsync(new Session()
                    {
                        session_id = sessionId,
                        user_id = foundUser[0].user_id
                    });
                    await context.SaveChangesAsync();

                    HttpContext.Response.Cookies.Append(ApiValues.SESSION_ID_KEY, sessionId.ToString(), new CookieOptions()
                    {
                        Path = ApiValues.COOKIE_PATH,
                        Secure = true,
                        HttpOnly = true,
                        SameSite = SameSiteMode.Strict,
                        Expires = DateTimeOffset.Now.AddDays(7)
                    });
                }
                else
                {
                    messageForClient = ApiValues.AUTH_WRONG_PASSWORD;
                    incorrectFieldType = ApiValues.PASSWORD_FIELD_TYPE;
                }
            }

            return new RegAuthResponse()
            {
                Result = messageForClient == ApiValues.AUTH_SUCCESS,
                Message = messageForClient,
                FieldType = incorrectFieldType
            };
        }

        private async Task<BaseResponse> RegisterUser(AuthParams authParams)
        {
            var isUserExist = await context.Users
                .AnyAsync(user => user.user_login.Equals(authParams.Login));

            if (!isUserExist)
            {
                var passwordByteArray = Encoding.UTF8.GetBytes(authParams.Password);
                var hashedPassword = passwordEncryptor.ComputeHash(passwordByteArray);

                await context.Users.AddAsync(new User()
                {
                    user_login = authParams.Login,
                    user_password = hashedPassword
                });
                await context.SaveChangesAsync();
            }

            return new RegAuthResponse()
            {
                Result = !isUserExist,
                Message = !isUserExist ? "Вы успешно зарегистрировались!" : "Пользователь с введеным логином уже существует!",
                FieldType = ApiValues.LOGIN_FIELD_TYPE
            };
        }

        // GET: /Users?userId=&searchString=
        [HttpGet]
        public async Task<JsonResult> UsersHandler([FromQuery] GetUsersParams getUsersParams)
        {
            var isSessionValid = CheckSession(ApiValues.SESSION_ID_KEY);
            var response = new UserDataResponse()
            {
                Result = false,
                Message = ApiValues.SESSION_NOT_FOUND,
                Data = Array.Empty<User>()
            };

            if (isSessionValid)
            {
                var sessionId = HttpContext.Request.Cookies[ApiValues.SESSION_ID_KEY];
                var queryParams = new Dictionary<ApiEnum, string>()
                {
                    { ApiEnum.USER_ID, getUsersParams.UserId },
                    { ApiEnum.SEARCH_STRING, getUsersParams.SearchString }
                };

                try
                {
                    var nonEmptyParam = queryParams
                        .First(queryParam => !queryParam.Value.Equals(""));

                    response = await usersFuncs[nonEmptyParam.Key](nonEmptyParam.Value);
                }
                catch (InvalidOperationException)
                {
                    response = await usersFuncs[ApiEnum.SESSION_ID](sessionId);
                }
            }

            return new JsonResult(response);
        }

        private async Task<UserDataResponse> GetUserBySessionId(string sessionId)
        {
            User[]? foundData = Array.Empty<User>();
            var parsedSessionId = long.Parse(sessionId);

            foundData = await context.Sessions
                .Join(context.Users, session => session.user_id, user => user.user_id, (session, user) => new
                {
                    sessionId = session.session_id,
                    userData = new User()
                    {
                        user_login = user.user_login,
                        user_birthday = user.user_birthday,
                        user_status = user.user_status
                    }
                })
                .Where(joinItem => joinItem.sessionId == parsedSessionId)
                .Select(foundItem => foundItem.userData)
                .ToArrayAsync();

            return new UserDataResponse()
            {
                Result = foundData.Length != 0,
                Message = foundData.Length != 0 ? "" : ApiValues.SESSION_NOT_FOUND,
                Data = foundData
            };
        }

        private async Task<UserDataResponse> GetUserById(string userId)
        {
            return new UserDataResponse();
        }

        private async Task<UserDataResponse> GetUsersBySearchString(string searchString)
        {
            return new UserDataResponse();
        }

        // DELETE: /Users
        [HttpDelete]
        public async Task<JsonResult> LogOutHandler()
        {
            Session? sessionToRemove = null;
            if (CheckSession(ApiValues.SESSION_ID_KEY))
            {
                try
                {   
                    var parsedSessionId = long.Parse(HttpContext.Request.Cookies[ApiValues.SESSION_ID_KEY]);

                    sessionToRemove = await context.Sessions
                        .FirstAsync(session => session.session_id == parsedSessionId);
                }
                catch (Exception)
                {}

                if (sessionToRemove != null)
                {
                    HttpContext.Response.Cookies.Delete(ApiValues.SESSION_ID_KEY);

                    context.Sessions.Remove(sessionToRemove);
                    await context.SaveChangesAsync();
                }
            }
            
            return new JsonResult(new BaseResponse()
            {
                Result = sessionToRemove != null,
                Message = sessionToRemove != null? ApiValues.SESSION_REMOVED : ApiValues.SESSION_NOT_FOUND
            });
        }

        [HttpPut]
        public async Task<JsonResult> EditUser([FromForm] EditUserParams newUserData)
        {
            User? foundUser = null;
            if (CheckSession(ApiValues.SESSION_ID_KEY))
            {
                var parsedSessionId = long.Parse(HttpContext.Request.Cookies[ApiValues.SESSION_ID_KEY]);

                try
                {
                    var foundSession = await context.Sessions
                        .FirstAsync(session => session.session_id == parsedSessionId);

                    foundUser = await context.Users
                        .FirstAsync(user => user.user_id == foundSession.user_id);

                    foundUser.user_birthday = newUserData.UserBirthday;
                    foundUser.user_status = newUserData.UserStatus;

                    await context.SaveChangesAsync();
                }
                catch (InvalidOperationException)
                {}
            }

            return new JsonResult(new BaseResponse()
            {
                Result = foundUser != null,
                Message = ""
            });
        }
    }
}
