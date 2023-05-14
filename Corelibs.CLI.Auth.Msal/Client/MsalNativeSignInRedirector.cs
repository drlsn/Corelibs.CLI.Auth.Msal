namespace Corelibs.CLI.Auth.Msal
{
    public class MsalNativeSignInRedirector : ISignInRedirector
    {
        private readonly ISignInManager _signInManager;

        public MsalNativeSignInRedirector(ISignInManager signInManager)
        {
            _signInManager = signInManager;
        }

        public async void Redirect(Exception exception)
        {
            if (exception is NoAccessTokenAvailableException)
            {
                await _signInManager.SignIn();
            }
        }
    }
}
