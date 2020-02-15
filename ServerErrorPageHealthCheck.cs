using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.Hosting;
using Umbraco.Core.Logging;
using Umbraco.Core.Services;

namespace Umbraco.Web.HealthCheck.Checks.Errors
{
    [HealthCheck("70D5B534-EE38-4ED2-9ED7-9BE89947E177", "500 Error Page", Description = "Check for the existence of a 500.html file for use in the case of internal server errors.", Group = "Errors")]
    public class ServerErrorPageHealthCheck : HealthCheck
    {
        private readonly ILocalizedTextService _textService;

        public ServerErrorPageHealthCheck(ILocalizedTextService textService)
        {
            _textService = textService;
        }

        public override IEnumerable<HealthCheckStatus> GetStatus()
        {
            return new[] { CheckForServerErrorPage() };
        }

        public override HealthCheckStatus ExecuteAction(HealthCheckAction action)
        {
            switch (action.Alias)
            {
                case "addServerErrorPage":
                    return AddServerErrorPage();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private HealthCheckStatus CheckForServerErrorPage()
        {
            var success = File.Exists(HttpContext.Current.Server.MapPath("~/500.html"));

            var message = success
                ? _textService.Localize("serverErrorPageHealthCheck/serverErrorPageCheckSuccess")
                : _textService.Localize("serverErrorPageHealthCheck/serverErrorPageCheckFailed");

            var actions = new List<HealthCheckAction>();

            if (success == false)
                actions.Add(new HealthCheckAction("addServerErrorPage", Id)
                { Name = _textService.Localize("serverErrorPageHealthCheck/serverErrorPageRectifyButtonName"), Description = _textService.Localize("serverErrorPageHealthCheck/serverErrorPageRectifyDescription") });

            return
                new HealthCheckStatus(message)
                {
                    ResultType = success ? StatusResultType.Success : StatusResultType.Error,
                    Actions = actions
                };
        }

        private HealthCheckStatus AddServerErrorPage()
        {
            var success = false;

            var message = string.Empty;

            const string content =
                "<!doctype html><html><head><title>500 Error</title></head><body><div class='text-center'><h1>An internal server error has occurred</h1></div></body></html>";

            File.WriteAllText(HostingEnvironment.MapPath("~/500.html"), content);

            success = true;

            message = _textService.Localize("serverErrorPageHealthCheck/serverErrorPageCheckSuccess");

            return
                new HealthCheckStatus(message)
                {
                    ResultType = success ? StatusResultType.Success : StatusResultType.Error,
                    Actions = new List<HealthCheckAction>()
                };
        }
    }
}