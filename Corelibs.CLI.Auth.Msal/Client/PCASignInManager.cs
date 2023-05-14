using Microsoft.Identity.Client;
using System.IdentityModel.Tokens.Jwt;

namespace Corelibs.CLI.Auth.Msal
{
    internal class PCASignInManager : ISignInManager
    {
        public static readonly string AccessTokenKey = "AccessToken";

        private readonly IPCAWrapper _pcaWrapper;
        private readonly ISecureStorage _secureStorage;

        private bool _loaded;
        private JwtSecurityToken _accessToken;

        public event Action<bool> OnAuthenticatedStateChanged;

        public PCASignInManager(
            IPCAWrapper pcaWrapper,
            ISecureStorage secureStorage)
        {
            _pcaWrapper = pcaWrapper;
            _secureStorage = secureStorage;
        }

        public async Task SignIn()
        {
            _accessToken = await GetAccessToken();
            var now = DateTime.UtcNow;
            if (!_accessToken.IsExpiredToken(now))
                return;

            try
            {
                var authResult = await _pcaWrapper.AcquireTokenSilentAsync(_pcaWrapper.Scopes).ConfigureAwait(false);
                _accessToken = new JwtSecurityToken(authResult.AccessToken);
                await _secureStorage.SetAsync(AccessTokenKey, _accessToken.RawData);
            }
            catch (MsalUiRequiredException) // A MsalUiRequiredException will be thrown, if this is the first attempt to login, or after logging out.
            {
                try
                {
                    var authResult = await _pcaWrapper.AcquireTokenInteractiveAsync(_pcaWrapper?.Scopes).ConfigureAwait(false);
                    _accessToken = new JwtSecurityToken(authResult.AccessToken);
                    await _secureStorage.SetAsync(AccessTokenKey, _accessToken.RawData);
                }
                catch (Exception) { }
            }
            catch
            {
                ResetToken();
            }

            OnAuthenticatedStateChanged?.Invoke(true);
        }

        public async Task SignOut()
        {
            await _pcaWrapper.SignOutAsync().ConfigureAwait(false);
            ResetToken();
            OnAuthenticatedStateChanged?.Invoke(false);
        }

        public async Task<JwtSecurityToken> GetAccessToken()
        {
            _accessToken = await GetTokenCached();
            if (!_accessToken.IsExpiredToken(DateTime.UtcNow))
                return _accessToken;

            try
            {
                return await GetTokenSilent();
            }
            catch (MsalUiRequiredException)
            {
                return default;
            }
        }

        private async Task<JwtSecurityToken> GetTokenCached()
        {
            if (!_loaded)
            {
                await LoadToken();
                _loaded = true;
            }

            return _accessToken;
        }

        private async Task<JwtSecurityToken> GetTokenSilent()
        {
            var authResult = await _pcaWrapper.AcquireTokenSilentAsync(_pcaWrapper.Scopes);
            _accessToken = new JwtSecurityToken(authResult.AccessToken);
            await _secureStorage.SetAsync(AccessTokenKey, _accessToken.RawData);

            return _accessToken;
        }

        public async Task<bool> IsSignedIn()
        {
            var token = await GetAccessToken();
            return token != null;
        }

        private void ResetToken()
        {
            _secureStorage.Remove(AccessTokenKey);
            _accessToken = null;
        }

        private async Task LoadToken()
        {
            var storedTokenStr = await _secureStorage.GetAsync(AccessTokenKey);
            if (string.IsNullOrEmpty(storedTokenStr))
                return;

            _accessToken = new JwtSecurityToken(storedTokenStr);
        }
    }

    public static class JwtTokenExtensions
    {
        public static bool IsExpiredToken(this JwtSecurityToken token, DateTime utcTimeNow)
        {
            if (token == null)
                return true;

            if (utcTimeNow > token.ValidFrom && utcTimeNow < token.ValidTo)
                return false;

            return true;
        }
    }
}
