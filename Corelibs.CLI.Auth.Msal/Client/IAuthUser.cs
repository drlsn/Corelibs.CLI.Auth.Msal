namespace Corelibs.CLI.Auth.Msal
{
    public interface IAuthUser
    {
        Task SignIn();
        Task SignOut();

        Task<bool> IsSignedIn();
        string Name { get; }

        event Action<bool> OnAuthenticatedStateChanged;
    }
}
