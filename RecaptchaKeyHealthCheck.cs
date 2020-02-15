using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Web;
using System.Web.Hosting;
using Umbraco.Core.Logging;
using Umbraco.Core.Services;
using Umbraco.Web.HealthCheck.Checks.Config;
using System.Xml;
using System.Web.Configuration;
using System.Linq;

namespace Umbraco.Web.HealthCheck.Checks.ReCaptcha
{
    [HealthCheck("F2A2BA5D-DC5B-432E-B29F-BC2790A4F671", "Web.config ReCaptcha Key", Description = "Checks that a Google ReCaptcha key has been added to Web.config (ignore if the site does either has no contact form, or does not need ReCaptcha protection).", Group = "ReCaptcha")]
    public class ReCaptchaKeyHealthCheck : HealthCheck
    {
        private readonly ILocalizedTextService _textService;
        public ReCaptchaKeyHealthCheck(ILocalizedTextService textService)
        {
            _textService = textService;
        }

        public override IEnumerable<HealthCheckStatus> GetStatus()
        {
            return new[] { CheckForReCaptchaKey() };
        }

        public override HealthCheckStatus ExecuteAction(HealthCheckAction action)
        {
            switch (action.Alias)
            {
                case "addReCaptchaKey":
                    return addReCaptchaKey();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private HealthCheckStatus CheckForReCaptchaKey()
        {
            var webConfig = WebConfigurationManager.OpenWebConfiguration("/");

            var success = false;

            if (webConfig.AppSettings.Settings.AllKeys.Contains("Google.ReCaptcha.Secret"))
            {
                webConfig.AppSettings.Settings.Add("Google.ReCaptcha.Secret", "INSERT_SECRET_KEY_HERE");
                success = true;
            }


            var message = success
                ? _textService.Localize("reCaptchaHealthCheck/reCaptchaKeyCheckSuccessMessage")
                : _textService.Localize("reCaptchaHealthCheck/reCaptchaKeyCheckErrorMessage");

            var actions = new List<HealthCheckAction>();

            if (success == false)
                actions.Add(new HealthCheckAction("addReCaptchaKey", Id)
                { Name = _textService.Localize("reCaptchaHealthCheck/reCaptchaKeyRectifyMessage"), Description = _textService.Localize("reCaptchaHealthCheck/reCaptchaKeyRectifyDescription") });

            return
                new HealthCheckStatus(message)
                {
                    ResultType = success ? StatusResultType.Success : StatusResultType.Error,
                    Actions = actions
                };
        }
        private HealthCheckStatus addReCaptchaKey()
        {
            var success = false;
            var message = string.Empty;

            var webConfig = WebConfigurationManager.OpenWebConfiguration("/");

            if (!webConfig.AppSettings.Settings.AllKeys.Contains("Google.ReCaptcha.Secret"))
            {
                webConfig.AppSettings.Settings.Add("Google.ReCaptcha.Secret", "INSERT_SECRET_KEY_HERE");
            }

            webConfig.Save(ConfigurationSaveMode.Minimal);

            success = true;

            return
                new HealthCheckStatus(message)
                {
                    ResultType = success ? StatusResultType.Success : StatusResultType.Error,
                    Actions = new List<HealthCheckAction>()
                };
        }
    }
}