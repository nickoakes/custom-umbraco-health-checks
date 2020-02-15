using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Umbraco.Core.Services;
using Umbraco.Web.Composing;
using Umbraco.Core.Models;

namespace Umbraco.Web.HealthCheck.Checks.RedirectUrlManagement
{
    [HealthCheck("F2E8C97D-B1BB-4984-95ED-6590604AA615", "Redirect URL Management", Description = "Checks that all unused links are removed from redirect URL management.", Group = "Redirect URL Management")]
    public class RedirectUrlManagementHealthCheck : HealthCheck
    {
        private readonly ILocalizedTextService _textService;

        public RedirectUrlManagementHealthCheck(ILocalizedTextService textService)
        {
            _textService = textService;
        }

        public override IEnumerable<HealthCheckStatus> GetStatus()
        {
            return new[] { CheckRedirectUrlManagement() };
        }

        public override HealthCheckStatus ExecuteAction(HealthCheckAction action)
        {
            switch (action.Alias)
            {
                case "checkRedirectUrlManagement":
                    return CheckRedirectUrlManagement();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private HealthCheckStatus CheckRedirectUrlManagement()
        {
            bool success;

            string message = string.Empty;

            IRedirectUrlService redirectUrlService = Current.Services.RedirectUrlService;

            IEnumerable<IRedirectUrl> redirectUrls = redirectUrlService.GetAllRedirectUrls(0, 1000, out long total);

            if (redirectUrls.Any())
            {
                success = false;

                message = _textService.Localize("redirectUrlManagementHealthCheck/redirectUrlsPresent");
            }
            else
            {
                success = true;

                message = _textService.Localize("redirectUrlManagementHealthCheck/noRedirectUrlsPresent");
            }

            return
                new HealthCheckStatus(message)
                {
                    ResultType = success ? StatusResultType.Success : StatusResultType.Info
                };
        }
    }
}