using System.Security.Claims;

namespace Serwer.Services {
    public class UserService : IUserService {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserService(IHttpContextAccessor httpContextAccessor) {
            _httpContextAccessor = httpContextAccessor;
        }
        
        public string GetName() {
            string result = string.Empty;
            if(_httpContextAccessor.HttpContext != null) {
                result = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.Name) ?? string.Empty;
            }
            return result;
        }

        public string GetRole() {
            string result = string.Empty;
            if(_httpContextAccessor.HttpContext != null) {
                result = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
            }
            return result;
        }
    }
}
