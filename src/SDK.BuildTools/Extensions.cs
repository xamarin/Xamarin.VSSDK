using Microsoft.Build.Utilities;

namespace Xamarin.VsSDK
{
    static class Extensions
    {
        public static void LogErrorCode(this TaskLoggingHelper log, string code, string message, params object[] messageArgs) =>
            log.LogError(string.Empty, code, string.Empty, string.Empty, 0, 0, 0, 0, message, messageArgs);

        public static void LogWarningCode(this TaskLoggingHelper log, string code, string file, string message, params object[] messageArgs) =>
            log.LogWarning(string.Empty, code, string.Empty, file, 0, 0, 0, 0, message, messageArgs);
    }
}
