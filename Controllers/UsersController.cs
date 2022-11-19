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
        public UsersController(IdeaspaceDBContext context)
        {
            this.context = context;
        }

        // POST: /Users
        [HttpPost]
        public async Task<IActionResult> AuthRegHandler([FromForm] AuthParams authParams)
        {
            var passwordEncryptor = SHA256.Create();

            var response = authParams.RequestType == ApiValues.AUTH_REQUEST_TYPE ?
                await AuthorizeUser(authParams, passwordEncryptor) : await RegisterUser(authParams, passwordEncryptor);

            return new JsonResult(response);
        }

        private async Task<BaseResponse> AuthorizeUser(AuthParams authParams, SHA256? passwordEncryptor)
        {
            var foundUser = await context.Users
                .Where(user => user.UserLogin.Equals(authParams.Login))
                .ToArrayAsync();

            var passwordByteArray = Encoding.UTF8.GetBytes(authParams.Password);
            var hashedPassword = passwordEncryptor.ComputeHash(passwordByteArray);

            var messageForClient = ApiValues.AUTH_WRONG_LOGIN;
            var incorrectFieldType = ApiValues.LOGIN_FIELD_TYPE;
            var sessionId = -1L;

            if (foundUser.Length != 0)
            {
                if (foundUser[0].UserPassword.SequenceEqual(hashedPassword))
                {
                    messageForClient = ApiValues.AUTH_SUCCESS;
                    sessionId = (long)DateTime.Now.Subtract(ApiValues.UNIX_START_DATE).TotalSeconds;

                    await context.Sessions.AddAsync(new Session()
                    {
                        SessionId = sessionId,
                        UserId = foundUser[0].UserId
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

        private async Task<BaseResponse> RegisterUser(AuthParams authParams, SHA256? passwordEncryptor)
        {
            var isUserExist = await context.Users
                .AnyAsync(user => user.UserLogin.Equals(authParams.Login));

            if (!isUserExist)
            {
                var passwordByteArray = Encoding.UTF8.GetBytes(authParams.Password);
                var hashedPassword = passwordEncryptor.ComputeHash(passwordByteArray);

                await context.Users.AddAsync(new User()
                {
                    UserLogin = authParams.Login,
                    UserPassword = hashedPassword
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
        public async Task<JsonResult> GetUsersHandler([FromQuery] GetParams getUsersParams)
        {
            var sessionId = CheckSession(ApiValues.SESSION_ID_KEY);
            var response = new DataResponse<User>()
            {
                Result = false,
                Message = ApiValues.SESSION_NOT_FOUND,
                Data = Array.Empty<User>()
            };

            if (sessionId != null)
            {
                if (!getUsersParams.Key.Equals(""))
                {
                    response = getUsersParams.Limit > 0 ? await GetUsersBySearchString(getUsersParams.Key, getUsersParams.Limit) 
                        : await GetUserByLogin(getUsersParams.Key);
                }
                else
                {
                    response = await GetUserBySessionId(sessionId.ToString());
                }
            }

            return new JsonResult(response);
        }

        private async Task<DataResponse<User>> GetUserBySessionId(string sessionId)
        {
            User[]? foundData = Array.Empty<User>();
            var parsedSessionId = long.Parse(sessionId);

            foundData = await context.Sessions
                .Where(session => session.SessionId == parsedSessionId)
                .Join(context.Users, session => session.UserId, user => user.UserId, (session, user) => new
                {
                    sessionId = session.SessionId,
                    userData = new User()
                    {
                        UserLogin = user.UserLogin,
                        UserBirthday = user.UserBirthday,
                        UserStatus = user.UserStatus
                    }
                })
                .Select(foundItem => foundItem.userData)
                .ToArrayAsync();

            return new DataResponse<User>()
            {
                Result = foundData.Length != 0,
                Message = foundData.Length != 0 ? "" : ApiValues.SESSION_NOT_FOUND,
                Data = foundData
            };
        }

        private async Task<DataResponse<User>> GetUserByLogin(string userLogin)
        {
            User[]? foundData = Array.Empty<User>();

            foundData = await context.Users
                .Where(user => user.UserLogin.Equals(userLogin))
                .Select(foundUser => new User()
                {
                    UserLogin = foundUser.UserLogin,
                    UserBirthday = foundUser.UserBirthday,
                    UserStatus = foundUser.UserStatus
                })
                .ToArrayAsync();

            return new DataResponse<User>()
            {
                Result = foundData.Length != 0,
                Message = "",
                Data = foundData
            };
        }

        private async Task<DataResponse<User>> GetUsersBySearchString(string searchString, int limit=30)
        {
            var foundUsersPortion = new List<User>();
            var usersArray = await context.Users
                .Where(user => user.UserLogin.Contains(searchString))
                .Select(foundUser => new User()
                {
                    UserLogin = foundUser.UserLogin
                })
                .ToArrayAsync();

            for (int i = 0; i < limit && i < usersArray.Length; ++i)
            {
                foundUsersPortion.Add(usersArray[i]);
            }

            var foundUsersArray = foundUsersPortion.ToArray();

            return new PaginationResponse<User>()
            {
                Result = foundUsersArray.Length != 0,
                IsOver = limit >= usersArray.Length,
                Message = "",
                Data = foundUsersArray
            };
        }

        // DELETE: /Users
        [HttpDelete]
        public async Task<JsonResult> LogOutHandler()
        {
            Session? sessionToRemove = null;
            var sessionId = CheckSession(ApiValues.SESSION_ID_KEY);
            if (sessionId != null)
            {
                try
                {   
                    sessionToRemove = await context.Sessions
                        .FirstAsync(session => session.SessionId == sessionId);
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

        // PUT: /Users
        [HttpPut]
        public async Task<JsonResult> EditUser([FromForm] EditUserParams newUserData)
        {
            User? foundUser = null;
            var sessionId = CheckSession(ApiValues.SESSION_ID_KEY);

            if (sessionId != null)
            {
                try
                {
                    var foundSession = await context.Sessions
                        .FirstAsync(session => session.SessionId == sessionId);

                    foundUser = await context.Users
                        .FirstAsync(user => user.UserId == foundSession.UserId);

                    foundUser.UserBirthday = newUserData.UserBirthday;
                    foundUser.UserStatus = newUserData.UserStatus;

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
