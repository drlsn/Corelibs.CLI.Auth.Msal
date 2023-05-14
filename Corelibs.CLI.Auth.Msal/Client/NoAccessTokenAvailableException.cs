
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;

namespace Corelibs.CLI.Auth.Msal
{
    public sealed class NoAccessTokenAvailableException : AccessTokenNotAvailableException
    {
        private readonly string _message;

        public string RedirectURL { get; }

        public NoAccessTokenAvailableException(string message, string redirectUrl) : base(null, null, null)
        {
            _message = message;
            RedirectURL = redirectUrl;
        }

        public override string Message => _message;
    }
}
