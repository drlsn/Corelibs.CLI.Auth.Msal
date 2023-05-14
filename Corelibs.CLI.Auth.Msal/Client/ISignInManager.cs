using System.IdentityModel.Tokens.Jwt;

namespace Corelibs.CLI.Auth.Msal
{
    public interface ISignInManager
    {
        Task SignIn();
        Task SignOut();
        Task<JwtSecurityToken> GetAccessToken();

        Task<bool> IsSignedIn();

        event Action<bool> OnAuthenticatedStateChanged;
    }
}
