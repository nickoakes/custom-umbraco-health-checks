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
    [HealthCheck("80C42A0E-62A8-4C10-A8A9-5CE4D5938DE6", "HTML Language Attribute", Description = "Checks that the <html> tag has the language attribute set.", Group = "HTML")]
    public class HtmlLanguageAttributeHealthCheck : HealthCheck
    {
        private readonly ILocalizedTextService _textService;

        public HtmlLanguageAttributeHealthCheck(ILocalizedTextService textService)
        {
            _textService = textService;
        }

        public override IEnumerable<HealthCheckStatus> GetStatus()
        {
            return new[] { CheckForHtmlLanguageAttribute() };
        }

        public override HealthCheckStatus ExecuteAction(HealthCheckAction action)
        {
            switch (action.Alias)
            {
                case "checkForHtmlLanguageAttribute":
                    return CheckForHtmlLanguageAttribute();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private HealthCheckStatus CheckForHtmlLanguageAttribute()
        {
            bool success = false;

            string html = string.Empty;

            string message = string.Empty;

            string scheme = HttpContext.Current.Request.Url.Scheme;

            string host = HttpContext.Current.Request.Url.Host;

            string port = HttpContext.Current.Request.Url.Port.ToString();

            string url = scheme + "://" + host + (host == "localhost" ? ":" + port : "");

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

            try
            {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    StreamReader streamReader = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding(response.CharacterSet));

                    HtmlDocument doc = new HtmlDocument();

                    doc.Load(streamReader);

                    HtmlNode htmlNode = doc.DocumentNode.SelectSingleNode("html");

                    HtmlAttributeCollection htmlNodeAttributes = htmlNode.Attributes;

                    if(htmlNodeAttributes.Contains("lang"))
                    {
                        success = true;

                        message = _textService.Localize("htmlLanguageAttributeHealthCheck/attributeCheckSuccess") + '"' + htmlNode.GetAttributeValue("lang", "(empty)") + '"';
                    }
                    else
                    {
                        message = _textService.Localize("htmlLanguageAttributeHealthCheck/attributeCheckFailed");
                    }
                }
            }
            catch
            {
                success = false;

                message = _textService.Localize("htmlLanguageAttributeHealthCheck/attributeCheckFailed");
            }

            return
                new HealthCheckStatus(message)
                {
                    ResultType = success ? StatusResultType.Success : StatusResultType.Error
                };
        }
    }
}