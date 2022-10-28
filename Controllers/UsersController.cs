using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Http;
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

        public UsersController(IdeaspaceDBContext context) : base(context)
        {
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
        public async Task<JsonResult> AuthRegHandler([FromForm] AuthParams authParams)
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
                }
                else
                {
                    messageForClient = ApiValues.AUTH_WRONG_PASSWORD;
                    incorrectFieldType = ApiValues.PASSWORD_FIELD_TYPE;
                }
            }

            return new AuthResponse()
            {
                Result = messageForClient == ApiValues.AUTH_SUCCESS,
                Message = messageForClient,
                FieldType = incorrectFieldType,
                SessionId = sessionId
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

        // GET: /Users?sessionId=&userId=&searchString=&
        [HttpGet]
        public async Task<JsonResult> UsersHandler([FromQuery] GetUsersParams getUsersParams)
        {
            var queryParams = new Dictionary<ApiEnum, string>()
            {
                { ApiEnum.SESSION_ID, getUsersParams.SessionId },
                { ApiEnum.USER_ID, getUsersParams.UserId },
                { ApiEnum.SEARCH_STRING, getUsersParams.SearchString }
            };

            var nonEmptyParam = queryParams
                .First(queryParam => !queryParam.Value.Equals(""));

            var response = await usersFuncs[nonEmptyParam.Key](nonEmptyParam.Value);

            return new JsonResult(response);
        }

        private async Task<UserDataResponse> GetUserBySessionId(string sessionId)
        {
            User[]? foundData = null;
            try
            {
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
            }
            catch(Exception){
                foundData = Array.Empty<User>();
            }

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
    }
}
