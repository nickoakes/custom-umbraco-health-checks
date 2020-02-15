using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Web;
using System.Web.Hosting;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web.Composing;

namespace Umbraco.Web.HealthCheck.Checks.SiteMap
{
    [HealthCheck("D324894F-4480-4019-8750-13192B50F88F", "Site Map", Description = "Checks that an XML site map exists at '/sitemap'.", Group = "Site Map")]
    public class SiteMapHealthCheck : HealthCheck
    {
        private readonly ILocalizedTextService _textService;

        public SiteMapHealthCheck(ILocalizedTextService textService)
        {
            _textService = textService;
        }

        public override IEnumerable<HealthCheckStatus> GetStatus()
        {
            return new[] { CheckForSiteMap() };
        }

        public override HealthCheckStatus ExecuteAction(HealthCheckAction action)
        {
            switch (action.Alias)
            {
                case "checkForSiteMap":
                    return CheckForSiteMap();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private HealthCheckStatus CheckForSiteMap()
        {
            bool success = false;

            string html = string.Empty;

            string message = string.Empty;

            string scheme = HttpContext.Current.Request.Url.Scheme;

            string host = HttpContext.Current.Request.Url.Host;

            string port = HttpContext.Current.Request.Url.Port.ToString();

            string url = scheme + "://" + host + (host ==  "localhost" ? ":" + port : "") + "/sitemap";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

            try
            {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                    {
                        if ((int)response.StatusCode >= 200 && (int)response.StatusCode < 300 && response.ContentType.ToString().Contains("xml"))
                        {
                            success = true;

                            message = _textService.Localize("siteMapHealthCheck/siteMapCheckSuccess");
                        }
                        else if((int)response.StatusCode >= 200 && (int)response.StatusCode < 300 && !response.ContentType.ToString().Contains("xml"))
                        {
                            success = false;

                            message = _textService.Localize("siteMapHealthCheck/siteMapNotXml");
                        }
                    }
            }
            catch
            {
                success = false;

                message =_textService.Localize("siteMapHealthCheck/siteMapCheckFailed");
            }

            return
                new HealthCheckStatus(message)
                {
                    ResultType = success ? StatusResultType.Success : StatusResultType.Error
                };
        }
    }
}