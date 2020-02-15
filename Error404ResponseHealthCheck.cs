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

namespace Umbraco.Web.HealthCheck.Checks.error404
{
    [HealthCheck("4CE5211A-942E-4925-A1BE-CC8F04E0E327", "404 Error Response", Description = "Check that a custom page is returned in the event of a 404 error.", Group = "Errors")]
    public class Error404ResponseHealthCheck : HealthCheck
    {
        private readonly ILocalizedTextService _textService;

        public Error404ResponseHealthCheck(ILocalizedTextService textService)
        {
            _textService = textService;
        }

        public override IEnumerable<HealthCheckStatus> GetStatus()
        {
            return new[] { CheckError404Response() };
        }

        public override HealthCheckStatus ExecuteAction(HealthCheckAction action)
        {
            switch (action.Alias)
            {
                case "checkError404Response":
                    return CheckError404Response();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private HealthCheckStatus CheckError404Response()
        {
            bool success = false;

            string html = string.Empty;

            string message = string.Empty;

            string scheme = HttpContext.Current.Request.Url.Scheme;

            string host = HttpContext.Current.Request.Url.Host;

            string port = HttpContext.Current.Request.Url.Port.ToString();

            string url = scheme + "://" + host + (host == "localhost" ? ":" + port : "") + "/DDBDDCAA-5C3C-4AB7-AFAE-F9EEA9008FF9"; //GUID used to get a 404 response

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

            try
            {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    
                }
            }
            catch(WebException e)
            {
                using(WebResponse response = e.Response)
                {
                    HttpWebResponse httpResponse = (HttpWebResponse)response;

                    StreamReader streamReader = new StreamReader(httpResponse.GetResponseStream());

                    HtmlDocument doc = new HtmlDocument();

                    doc.Load(streamReader);

                    HtmlNode content = doc.DocumentNode.SelectSingleNode("html");

                    success = !content.InnerHtml.Contains("This page can be replaced with a custom 404.");
                }
            }

            message = success ? _textService.Localize("error404ResponseHealthCheck/responseCheckSuccess") : _textService.Localize("error404ResponseHealthCheck/responseCheckFailed");

            return
                new HealthCheckStatus(message)
                {
                    ResultType = success ? StatusResultType.Success : StatusResultType.Error
                };
        }
    }
}