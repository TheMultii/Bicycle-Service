namespace Serwer {
    public class User {
        public string UID { get; set; } = string.Empty;
        public string Login { get; set; } = string.Empty;
        public byte[]? PasswordHash { get; set; }
        public byte[]? PasswordSalt { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Surname { get; set; } = string.Empty;
        public string AccountType { get; set; } = string.Empty;
    }
}
