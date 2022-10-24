namespace Ideaspace_backend.Models
{
    public class ApiValues
    {
        public static readonly string AUTH_SUCCESS = "Вы вошли в систему!";
        public static readonly string AUTH_WRONG_LOGIN = "Вы ввели несуществующий логин!";
        public static readonly string AUTH_WRONG_PASSWORD = "Вы ввели неверный пароль!";
        public static readonly string AUTH_REQUEST_TYPE = "auth";
        public static readonly string REG_REQUEST_TYPE = "reg";
        public static readonly string LOGIN_FIELD_TYPE = "login";
        public static readonly string PASSWORD_FIELD_TYPE = "password";
    }
}
