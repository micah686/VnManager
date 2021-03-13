using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using Sentry;

namespace VnManager.Helpers
{
    public static class SentryHelper
    {
#nullable enable
        public static void SendException(Exception ex, string? data, SentryLevel sentryLevel)
        {

            
            string userIdHash;
            using (var md5 = MD5.Create())
            {
                NTAccount f = new NTAccount(Environment.UserName);
                SecurityIdentifier s = (SecurityIdentifier)f.Translate(typeof(SecurityIdentifier));
                var sidString = s.ToString();
                var hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(sidString));
                userIdHash = BitConverter.ToString(hashBytes).Replace("-", "").ToUpperInvariant();
            }

            SentrySdk.WithScope(scope =>
            {
#if DEBUG
                scope.Environment = $@"VnManager-{App.VersionString}-DEBUG";
#else
                scope.Environment = $@"VnManager-{App.VersionString}";
#endif
                if (data != null)
                {
                    scope.SetTag("USER_ID", userIdHash);

                    scope.AddAttachment(Encoding.ASCII.GetBytes(data), "data.txt");
                    scope.Level = sentryLevel;
                    scope.Release = App.VersionString;
                    scope.AddBreadcrumb("Logic Failed!");
                    SentrySdk.CaptureException(ex);
                }

            });

        }
#nullable restore
    }
}
