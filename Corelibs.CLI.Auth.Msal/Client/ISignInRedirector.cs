namespace Corelibs.CLI.Auth.Msal
{
    public interface ISignInRedirector
    {
        void Redirect(Exception exception);
    }
}
