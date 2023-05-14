namespace Corelibs.CLI.Auth.Msal
{
    public class MsalNativeAuthUser : IAuthUser
    {
        private readonly ISignInManager _signInManager;

        public Task<bool> IsSignedIn() => _signInManager.IsSignedIn();

        public string Name => string.Empty;

        public event Action<bool> OnAuthenticatedStateChanged
        {
            add
            {
                _signInManager.OnAuthenticatedStateChanged += value;
            }
            remove
            {
                _signInManager.OnAuthenticatedStateChanged -= value;
            }
        }

        public MsalNativeAuthUser(ISignInManager signInManager)
        {
            _signInManager = signInManager;
        }

        public Task SignIn() => _signInManager.SignIn();
        public Task SignOut() => _signInManager.SignOut();
    }
}
