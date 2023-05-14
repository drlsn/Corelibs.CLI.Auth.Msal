namespace Corelibs.CLI.Auth.Msal
{
    public class MsalClientScope
    {
        public string Value { get; set; } = null;
    }

    public static class MsalClientScopeExtensions
    {
        public static string[] ToStringArray(this MsalClientScope[] scopes)
        {
            var result = new string[scopes.Length];

            for (int i = 0; i < scopes.Length; i++)
                result[i] = scopes[i].Value;

            return result!;
        }
    }
}
