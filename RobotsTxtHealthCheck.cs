using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.Hosting;
using Umbraco.Core.Logging;
using Umbraco.Core.Services;

namespace Umbraco.Web.HealthCheck.Checks.SEO
{
    [HealthCheck("3A482719-3D90-4BC1-B9F8-910CD9CF5B32", "robots.txt", Description ="Check for the existence of a robots.txt file.", Group ="SEO")]
    public class RobotsTxtHealthCheck : HealthCheck
    {
        private readonly ILocalizedTextService _textService;
        public RobotsTxtHealthCheck(ILocalizedTextService textService)
        {
            _textService = textService;
        }

        public override IEnumerable<HealthCheckStatus> GetStatus()
        {
            return new[] { CheckForRobotsTxtFile() };
        }

        public override HealthCheckStatus ExecuteAction(HealthCheckAction action)
        {
            switch (action.Alias)
            {
                case "addDefaultRobotsTxtFile":
                    return AddDefaultRobotsTxtFile();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private HealthCheckStatus CheckForRobotsTxtFile()
        {
            var success = File.Exists(HttpContext.Current.Server.MapPath("~/robots.txt"));

            var message = success
                ? _textService.Localize("robotsHealthCheck/seoRobotsCheckSuccess")
                : _textService.Localize("robotsHealthCheck/seoRobotsCheckFailed");

            var actions = new List<HealthCheckAction>();

            if (success == false)
                actions.Add(new HealthCheckAction("addDefaultRobotsTxtFile", Id)
                { Name = _textService.Localize("robotsHealthCheck/seoRobotsRectifyButtonName"), Description = _textService.Localize("robotsHealthCheck/seoRobotsRectifyDescription") });

            return
                new HealthCheckStatus(message)
                {
                    ResultType = success ? StatusResultType.Success : StatusResultType.Error,
                    Actions = actions
                };
        }
        private HealthCheckStatus AddDefaultRobotsTxtFile()
        {
            var success = false;

            var message = string.Empty;

            const string content = 
            @"# robots.txt for Umbraco
                User-agent: * 
                Disallow: /umbraco/
                Disallow: /App_Browsers/
                Disallow: /App_Code/
                Disallow: /App_Plugins/
                Disallow: /bin/
                Disallow: /Config/
                Disallow: /Service References/
                Disallow: /umbraco/
                Disallow: /umbraco_client/
                Disallow: /Views/";

            File.WriteAllText(HostingEnvironment.MapPath("~/robots.txt"), content);

            success = true;

            message = _textService.Localize("robotsHealthCheck/seoRobotsCheckSuccess");

            return
                new HealthCheckStatus(message)
                {
                    ResultType = success ? StatusResultType.Success : StatusResultType.Error,
                    Actions = new List<HealthCheckAction>()
                };
        }
    }
}