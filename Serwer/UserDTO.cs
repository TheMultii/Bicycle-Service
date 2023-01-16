namespace Serwer {
    public class UserLoginDTO {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
    public class UserRegisterDTO {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Surname { get; set; } = string.Empty;
    }
    public class UserMyDataDTO {
        public long UID { get; set; } = 0;
        public string Username { get; set; } = string.Empty;
        public string Permission { get; set; } = string.Empty;
    }
}
