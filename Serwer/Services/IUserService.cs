namespace Serwer.Services {
    public interface IUserService {
        string GetName();
        string GetRole();
        DateTime GetExpirationDate();
    }
}
