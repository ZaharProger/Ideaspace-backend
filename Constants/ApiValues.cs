using Ideaspace_backend.Models.Http;

namespace Ideaspace_backend.Constants
{
    public class ApiValues
    {
        public static readonly string AUTH_SUCCESS = "Вы вошли в систему!";
        public static readonly string AUTH_WRONG_LOGIN = "Вы ввели несуществующий логин!";
        public static readonly string AUTH_WRONG_PASSWORD = "Вы ввели неверный пароль!";
        public static readonly string AUTH_REQUEST_TYPE = "auth";
        public static readonly string REG_REQUEST_TYPE = "reg";
        public static readonly string LOGIN_FIELD_TYPE = "login";
        public static readonly string SESSION_ID_KEY = "session_id";
        public static readonly string COOKIE_PATH = "/";
        public static readonly string PASSWORD_FIELD_TYPE = "password";
        public static readonly string SESSION_REMOVED = "Сессия успешно завершена!";
        public static readonly string SESSION_NOT_FOUND = "Ваша сессия просрочена!";
        public static readonly DateTime UNIX_START_DATE = new DateTime(1970, 1, 1);
    }
}
