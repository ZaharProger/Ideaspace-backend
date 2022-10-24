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
using Ideaspace_backend.Models.Http;

namespace Ideaspace_backend.Controllers
{
    [Route("ideaspace/api/[controller]")]
    [ApiController]
    public class UsersController : IdeaspaceController
    {
        private readonly SHA256 passwordEncryptor;

        public UsersController(IdeaspaceDBContext context) : base(context)
        {
            passwordEncryptor = SHA256.Create();
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
                .Where(user => user.user_login == authParams.Login)
                .ToArrayAsync();

            var passwordByteArray = Encoding.UTF8.GetBytes(authParams.Password);
            var hashedPassword = passwordEncryptor.ComputeHash(passwordByteArray);

            var messageForClient = ApiValues.AUTH_WRONG_LOGIN;
            var incorrectFieldType = ApiValues.LOGIN_FIELD_TYPE;
            var sessionId = Array.Empty<byte>();

            if (foundUser.Length != 0)
            {
                if (foundUser[0].user_password.SequenceEqual(hashedPassword))
                {
                    messageForClient = ApiValues.AUTH_SUCCESS;

                    var encryptionData = Encoding.UTF8.GetBytes($"{foundUser[0].user_id}-{foundUser[0].user_login}-{DateTime.Now.Second}");
                    sessionId = passwordEncryptor.ComputeHash(encryptionData);

                    //TODO: Добавить создание сессии
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
                .AnyAsync(user => user.user_login == authParams.Login);

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
    }
}
