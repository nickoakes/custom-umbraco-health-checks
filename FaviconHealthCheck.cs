using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Hosting;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web.Composing;

namespace Umbraco.Web.HealthCheck.Checks.Html
{
    [HealthCheck("9DADA888-DEB5-468D-B675-997A93CB7D57", "Favicon", Description = "Checks whether the site has a favicon.", Group = "SEO")]
    public class FaviconHealthCheck : HealthCheck
    {
        private readonly ILocalizedTextService _textService;

        public FaviconHealthCheck(ILocalizedTextService textService)
        {
            _textService = textService;
        }

        public override IEnumerable<HealthCheckStatus> GetStatus()
        {
            return new[] { CheckForFavicon() };
        }

        public override HealthCheckStatus ExecuteAction(HealthCheckAction action)
        {
            switch (action.Alias)
            {
                case "checkForFavicon":
                    return CheckForFavicon();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private HealthCheckStatus CheckForFavicon()
        {
            bool success = false;

            string html = string.Empty;

            string message = string.Empty;

            string scheme = HttpContext.Current.Request.Url.Scheme;

            string host = HttpContext.Current.Request.Url.Host;

            string port = HttpContext.Current.Request.Url.Port.ToString();

            string url = scheme + "://" + host + (host == "localhost" ? ":" + port : "");

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                StreamReader streamReader = new StreamReader(response.GetResponseStream());

                HtmlDocument doc = new HtmlDocument();

                doc.Load(streamReader);

                HtmlNode htmlNode = doc.DocumentNode.SelectSingleNode("html");

                if (htmlNode.InnerHtml.ToLower().Contains("favicon"))
                {
                    success = true;
                }
            }

            message = success ? _textService.Localize("faviconHealthCheck/faviconCheckSuccess") : _textService.Localize("faviconHealthCheck/faviconCheckFailed");

            return
                new HealthCheckStatus(message)
                {
                    ResultType = success ? StatusResultType.Success : StatusResultType.Error
                };
        }
    }
}