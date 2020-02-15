using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Hosting;
using System.Xml;
using Umbraco.Core.Logging;
using Umbraco.Core.Services;

namespace Umbraco.Web.HealthCheck.Checks.Config
{
    [HealthCheck("08E5F0B4-E6B5-45FE-A61D-70618EEE9F65", "Custom Errors Config", Description = "Checks that custom errors are configured correctly in Web.config and umbracoSettings.config", Group = "Errors")]
    public class CustomErrorsModeHealthCheck : HealthCheck
    {
        private readonly ILocalizedTextService _textService;
        public CustomErrorsModeHealthCheck(ILocalizedTextService textService)
        {
            _textService = textService;
        }

        public override IEnumerable<HealthCheckStatus> GetStatus()
        {
            return new[] { CheckCustomErrors(), CheckErrorElements(), CheckHttpErrors(), CheckUmbracoSettings404NodeId() };
        }

        public override HealthCheckStatus ExecuteAction(HealthCheckAction action)
        {
            switch (action.Alias)
            {
                case "fixCustomErrors":
                    return FixCustomErrors();
                case "fixMissing404Element":
                    return FixMissing404Element();
                case "fixMissing500Element":
                    return FixMissing500Element();
                case "fixMissingHttpErrorsElement":
                    return FixMissingHttpErrorsElement();
                case "fixMissingHttpErrorsErrorMode":
                    return FixMissingHttpErrorsErrorMode();
                case "fixMissingRemove404":
                    return FixMissingRemove404();
                case "fixMissingRemove500":
                    return FixMissingRemove500();
                case "fixMissingHttpError404":
                    return FixMissingHttpError404();
                case "fixMissingHttpError500":
                    return FixMissingHttpError500();
                case "fixAllMissingHttpErrorsElements":
                    return FixAllMissingHttpErrorsElements();
                case "checkHttpErrorsChildNodes":
                    return CheckHttpErrorsChildNodes();
                case "fixMissingUmbracoSettings404Node":
                    return FixMissingUmbracoSettings404Node();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /***************************************************************************************
        *Check whether <customErrors mode="On" defaultRedirect="500.html"> exists in Web.config*
        ***************************************************************************************/

        private HealthCheckStatus CheckCustomErrors()
        {
            bool success;

            XmlDocument doc = new XmlDocument();

            doc.Load(HttpContext.Current.Server.MapPath("~/Web.config"));

            XmlNode customErrors = doc.SelectSingleNode("/configuration/system.web/customErrors");

            if(customErrors != null)
            {
                XmlAttribute customErrorsMode = customErrors.Attributes["mode"];

                XmlAttribute customErrorsDefaultRedirect = customErrors.Attributes["defaultRedirect"];

                if(customErrorsMode != null && customErrorsDefaultRedirect != null)
                {
                    success = (customErrorsMode.Value == "On" && customErrorsDefaultRedirect.Value == "~/500.html");
                }
                else
                {
                    success = false;
                }
            }
            else
            {
                success = false;
            }

            var message = success
                ? _textService.Localize("customErrorsHealthCheck/customErrorsCheckSuccess")
                : _textService.Localize("customErrorsHealthCheck/customErrorsCheckFailed");

            var actions = new List<HealthCheckAction>();

            if (success == false)
                actions.Add(new HealthCheckAction("fixCustomErrors", Id)
                { Name = _textService.Localize("customErrorsHealthCheck/customErrorsRectifyMessage"), Description = _textService.Localize("customErrorsHealthCheck/customErrorsRectifyDescription") });

            return
                new HealthCheckStatus(message)
                {
                    ResultType = success ? StatusResultType.Success : StatusResultType.Error,
                    Actions = actions
                };
        }

        /**********************************************************************
        *Add <customErrors mode="On" defaultRedirect="500.html"> to Web.config*
        **********************************************************************/

        private HealthCheckStatus FixCustomErrors()
        {
            XmlDocument doc = new XmlDocument();

            doc.Load(HttpContext.Current.Server.MapPath("~/Web.config"));

            XmlNode customErrors = doc.SelectSingleNode("/configuration/system.web/customErrors");

            if(customErrors == null)
            {
                customErrors = doc.CreateNode(XmlNodeType.Element, "customErrors", "");
            }

            XmlAttribute customErrorsMode = customErrors.Attributes["mode"];

            XmlAttribute customErrorsDefaultRedirect = customErrors.Attributes["defaultRedirect"];

            if (customErrorsMode.Value != null)
            {
                customErrorsMode.Value = "On";
            }
            
            if(customErrorsDefaultRedirect != null)
            {
                customErrorsDefaultRedirect.Value = "~/500.html";
            }
            else
            {
                XmlAttribute newDefaultRedirect = doc.CreateAttribute("defaultRedirect");

                newDefaultRedirect.Value = "~/500.html";

                customErrors.Attributes.SetNamedItem(newDefaultRedirect);
            }

            doc.Save(HttpContext.Current.Server.MapPath("~/Web.config"));

            var message = _textService.Localize("customErrorsHealthCheck/customErrorsCheckSuccess");

            return
                new HealthCheckStatus(message)
                {
                    ResultType = StatusResultType.Success,
                    Actions = new List<HealthCheckAction>()
                };
        }

        /**************************************************************************************
        *Check whether <error> elements exist under <customErrors> for both 404 and 500 errors*
        **************************************************************************************/

        private HealthCheckStatus CheckErrorElements()
        {
            bool success = false;

            bool tag404Present = false;

            bool tag500Present = false;

            var message = string.Empty;

            XmlDocument doc = new XmlDocument();

            doc.Load(HttpContext.Current.Server.MapPath("~/Web.config"));

            XmlNode customErrors = doc.SelectSingleNode("/configuration/system.web/customErrors");

            if (customErrors.HasChildNodes)
            {
                foreach(XmlNode node in customErrors.ChildNodes)
                {
                    XmlAttribute statusCode = node.Attributes["statusCode"];

                    XmlAttribute redirect = node.Attributes["redirect"];

                    if(statusCode != null && redirect != null && statusCode.Value == "404")
                    {
                        tag404Present = true;
                    }
                    else if(statusCode != null && redirect != null && statusCode.Value == "500" && redirect.Value == "~/500.html")
                    {
                        tag500Present = true;
                    }
                }
            }

            var actions = new List<HealthCheckAction>();

            if (tag500Present && tag404Present)
            {
                success = true;

                message = _textService.Localize("customErrorsHealthCheck/errorElementsCheckSuccess");
            }
            else if(tag404Present && !tag500Present)
            {
                message = _textService.Localize("customErrorsHealthCheck/error500ElementCheckFailed");

                actions.Add(new HealthCheckAction("fixMissing500Element", Id)
                { Name = _textService.Localize("customErrorsHealthCheck/missing500ElementRectifyMessage"), Description = _textService.Localize("customErrorsHealthCheck/missing500ElementRectifyDescription") });
            }
            else if(!tag404Present && tag500Present)
            {
                message = _textService.Localize("customErrorsHealthCheck/error404ElementCheckFailed");

                actions.Add(new HealthCheckAction("fixMissing404Element", Id)
                { Name = _textService.Localize("customErrorsHealthCheck/missing404ElementRectifyMessage"), Description = _textService.Localize("customErrorsHealthCheck/missing404ElementRectifyDescription") });
            }
            else
            {
                message = _textService.Localize("customErrorsHealthCheck/errorElementsCheckFailed");

                actions.Add(new HealthCheckAction("fixMissing404Element", Id)
                { Name = _textService.Localize("customErrorsHealthCheck/missing404ElementRectifyMessage"), Description = _textService.Localize("customErrorsHealthCheck/missing404ElementRectifyDescription") });
            }                

            return
                new HealthCheckStatus(message)
                {
                    ResultType = success ? StatusResultType.Success : StatusResultType.Error,
                    Actions = actions
                };
        }

        /*****************************************************************************************************
         * Add <error statusCode="404" redirect="YOUR_ERROR404_VIEW_HERE"> under <customErrors> in Web.config*
         ****************************************************************************************************/

        private HealthCheckStatus FixMissing404Element()
        {
            var actions = new List<HealthCheckAction>();

            XmlDocument doc = new XmlDocument();

            doc.Load(HttpContext.Current.Server.MapPath("~/Web.config"));

            XmlNode customErrors = doc.SelectSingleNode("/configuration/system.web/customErrors");

            if(doc.SelectSingleNode("/configuration/system.web/customErrors/error[@statusCode='500']") == null)
            {
                actions.Add(new HealthCheckAction("fixMissing500Element", Id)
                { Name = _textService.Localize("customErrorsHealthCheck/missing500ElementRectifyMessage"), Description = _textService.Localize("customErrorsHealthCheck/missing500ElementRectifyDescription") });
            }

            XmlElement error404Element = doc.CreateElement("error");

            error404Element.SetAttribute("statusCode", "404");

            error404Element.SetAttribute("redirect", "YOUR_ERROR404_VIEW_HERE");

            customErrors.AppendChild(error404Element);

            doc.Save(HttpContext.Current.Server.MapPath("~/Web.config"));

            string message = _textService.Localize("customErrorsHealthCheck/error404ElementsCheckSuccess");

            return
                new HealthCheckStatus(message)
                {
                    ResultType = StatusResultType.Success,
                    Actions = actions
                };
        }

        /****************************************************************************************
         * Add <error statusCode="500" redirect="~/500.html"> under <customErrors> in Web.config*
         ***************************************************************************************/

        private HealthCheckStatus FixMissing500Element()
        {
            var actions = new List<HealthCheckAction>();

            XmlDocument doc = new XmlDocument();

            doc.Load(HttpContext.Current.Server.MapPath("~/Web.config"));

            XmlNode customErrors = doc.SelectSingleNode("/configuration/system.web/customErrors");

            if (doc.SelectSingleNode("/configuration/system.web/customErrors/error[@statusCode='404']") == null)
            {
                actions.Add(new HealthCheckAction("fixMissing404Element", Id)
                { Name = _textService.Localize("customErrorsHealthCheck/missing404ElementRectifyMessage"), Description = _textService.Localize("customErrorsHealthCheck/missing404ElementRectifyDescription") });
            }

            XmlElement error500Element = doc.CreateElement("error");

            error500Element.SetAttribute("statusCode", "500");

            error500Element.SetAttribute("redirect", "~/500.html");

            customErrors.AppendChild(error500Element);

            doc.Save(HttpContext.Current.Server.MapPath("~/Web.config"));

            string message = _textService.Localize("customErrorsHealthCheck/error500ElementsCheckSuccess");

            return
                new HealthCheckStatus(message)
                {
                    ResultType = StatusResultType.Success,
                    Actions = actions
                };
        }

        /***********************************************************************************
         * Check for <httpErrors errorMode="Custom"> under <system.webServer> in Web.config*
         **********************************************************************************/

        private HealthCheckStatus CheckHttpErrors()
        {
            bool success;

            string message = string.Empty;

            var actions = new List<HealthCheckAction>();

            XmlDocument doc = new XmlDocument();

            doc.Load(HttpContext.Current.Server.MapPath("~/Web.config"));

            XmlNode httpErrors = doc.SelectSingleNode("/configuration/system.webServer/httpErrors");

            if(httpErrors == null)
            {
                success = false;
                message = _textService.Localize("customErrorsHealthCheck/httpErrorsCheckFailed");

                actions.Add(new HealthCheckAction("fixMissingHttpErrorsElement", Id)
                { Name = _textService.Localize("customErrorsHealthCheck/missingHttpErrorsRectifyMessage"), Description = _textService.Localize("customErrorsHealthCheck/missingHttpErrorsRectifyDescription") });
            }
            else if(httpErrors.Attributes["errorMode"] == null || httpErrors.Attributes["errorMode"].Value != "Custom")
            {
                success = false;

                message = _textService.Localize("customErrorsHealthCheck/httpErrorsErrorModeCheckFailed");

                actions.Add(new HealthCheckAction("fixMissingHttpErrorsErrorMode", Id)
                { Name = _textService.Localize("customErrorsHealthCheck/httpErrorsErrorModeRectifyMessage"), Description = _textService.Localize("customErrorsHealthCheck/httpErrorsErrorModeRectifyDescription") });
            }
            else
            {
                success = true;
                message = _textService.Localize("customErrorsHealthCheck/httpErrorsCheckSuccess");

                actions.Add(new HealthCheckAction("checkHttpErrorsChildNodes", Id)
                { Name = "Check httpErrors child elements", Description = "" });
            }

            message = success == true ? _textService.Localize("httpErrorsErrorModeCheckSuccess") : message;

            return
                new HealthCheckStatus(message)
                {
                    ResultType = success ? StatusResultType.Success : StatusResultType.Error,
                    Actions = actions
                };
        }

        /************************************************************************************************************
         * Check that <remove> and <error> elements exist for both 404 and 500 errors under httpErrors in Web.config*
         ***********************************************************************************************************/

        private HealthCheckStatus CheckHttpErrorsChildNodes()
        {
            var actions = new List<HealthCheckAction>();

            bool success = false;

            string message = string.Empty;

            XmlDocument doc = new XmlDocument();

            doc.Load(HttpContext.Current.Server.MapPath("~/Web.config"));

            XmlNode httpErrors = doc.SelectSingleNode("/configuration/system.webServer/httpErrors");

            if(httpErrors != null)
            {
                if(httpErrors.HasChildNodes)
                    {
                        XmlNode remove404 = doc.SelectSingleNode("/configuration/system.webServer/httpErrors/remove[@statusCode='404']");

                        XmlNode remove500 = doc.SelectSingleNode("/configuration/system.webServer/httpErrors/remove[@statusCode='500']");

                        XmlNode error404 = doc.SelectSingleNode("/configuration/system.webServer/httpErrors/error[@statusCode='404']");

                        XmlNode error500 = doc.SelectSingleNode("/configuration/system.webServer/httpErrors/error[@statusCode='500']");

                        if(remove404 == null)
                        {
                            success = false;

                            message = _textService.Localize("customErrorsHealthCheck/remove404CheckFailed");

                            actions.Add(new HealthCheckAction("fixMissingRemove404", Id)
                            { Name = _textService.Localize("customErrorsHealthCheck/missingRemoveRectifyMessage"), Description = _textService.Localize("customErrorsHealthCheck/missingRemove404RectifyDescription") });
                        }
                        if(remove500 == null)
                        {
                            success = false;

                            message = _textService.Localize("customErrorsHealthCheck/remove500CheckFailed");

                            actions.Add(new HealthCheckAction("fixMissingRemove500", Id)
                            { Name = _textService.Localize("customErrorsHealthCheck/missingRemoveRectifyMessage"), Description = _textService.Localize("customErrorsHealthCheck/missingRemove500RectifyDescription") });
                        }
                        if(error404 == null)
                        {
                            success = false;

                            message = _textService.Localize("customErrorsHealthCheck/httpError404CheckFailed");

                            actions.Add(new HealthCheckAction("fixMissingHttpError404", Id)
                            { Name = _textService.Localize("customErrorsHealthCheck/missingRemoveRectifyMessage"), Description = _textService.Localize("customErrorsHealthCheck/missingHttpError404Description") });
                        }
                        if(error500 == null)
                        {
                            success = false;

                            message = _textService.Localize("customErrorsHealthCheck/httpError500CheckFailed");

                            actions.Add(new HealthCheckAction("fixMissingHttpError500", Id)
                            { Name = _textService.Localize("customErrorsHealthCheck/missingRemoveRectifyMessage"), Description = _textService.Localize("customErrorsHealthCheck/missingHttpError500Description") });
                        }
                        if (!actions.Any())
                        {
                            success = true;

                            message = _textService.Localize("customErrorsHealthCheck/httpErrorsChildNodesCheckSuccess");
                        }
                    }
                    else
                    {
                        success = false;

                        message = _textService.Localize("customErrorsHealthCheck/httpErrorsChildNodesCheckFailed");

                        actions.Add(new HealthCheckAction("fixAllMissingHttpErrorsElements", Id)
                        { Name = _textService.Localize("customErrorsHealthCheck/missingHttpErrorsElementsMessage"), Description = _textService.Localize("customErrorsHealthCheck/missingHttpErrorsElementsDescription") });
                    }
            }
            else
            {
                success = false;

                message = _textService.Localize("customErrorsHealthCheck/noHttpErrorsElement");
            }
            
            return
                new HealthCheckStatus(message)
                {
                    ResultType = success ? StatusResultType.Success : StatusResultType.Error,
                    Actions = actions
                };
        }

        /*********************************************************************************
         *Check whether the <error404> Node ID has been changed in umbracoSettings.config*
         ********************************************************************************/

        private HealthCheckStatus CheckUmbracoSettings404NodeId()
        {
            bool success;

            string message = string.Empty;

            var actions = new List<HealthCheckAction>();

            XmlDocument doc = new XmlDocument();

            doc.Load(HttpContext.Current.Server.MapPath("~/config/umbracoSettings.config"));

            XmlNode error404 = doc.SelectSingleNode("/settings/content/errors/error404");

            if(error404 != null)
            {
                if (error404.InnerText == "1")
                    {
                        success = false;

                        message = _textService.Localize("/customErrorsHealthCheck/umbracoSettings404NodeIdCheckFailed");
                    }
                    else
                    {
                        success = true;

                        message = _textService.Localize("/customErrorsHealthCheck/umbracoSettings404NodeIdCheckSuccess");
                    }
                }
            else
            {
                success = false;

                message = _textService.Localize("/customErrorsHealthCheck/umbracoSettings404NodeNotFound");

                actions.Add(new HealthCheckAction("fixMissingUmbracoSettings404Node", Id)
                { Name = _textService.Localize("customErrorsHealthCheck/missingUmbracoSettings404Node"), Description = _textService.Localize("customErrorsHealthCheck/missingUmbracoSettings404NodeDescription") });
            }

            return
                new HealthCheckStatus(message)
                {
                    ResultType = success ? StatusResultType.Success : StatusResultType.Error,
                    Actions = actions
                };
        }

        /****************************************************************************
         *Add <httpErrors errorMode="Custom"> under <system.webServer> in Web.config*
         ***************************************************************************/

        private HealthCheckStatus FixMissingHttpErrorsElement()
        {
            XmlDocument doc = new XmlDocument();

            doc.Load(HttpContext.Current.Server.MapPath("~/Web.config"));

            XmlNode systemWebServer = doc.SelectSingleNode("/configuration/system.webServer");

            XmlElement httpErrors = doc.CreateElement("httpErrors");

            httpErrors.SetAttribute("errorMode", "Custom");

            systemWebServer.AppendChild(httpErrors);

            doc.Save(HttpContext.Current.Server.MapPath("~/Web.config"));

            string message = _textService.Localize("customErrorsHealthCheck/httpErrorsCheckSuccess");

            var actions = new List<HealthCheckAction>();

            actions.Add(new HealthCheckAction("fixAllMissingHttpErrorsElements", Id)
            { Name = _textService.Localize("customErrorsHealthCheck/missingHttpErrorsElementsMessage"), Description = _textService.Localize("customErrorsHealthCheck/missingHttpErrorsElementsDescription") });

            return
                new HealthCheckStatus(message)
                {
                    ResultType = StatusResultType.Error,
                    Actions = actions
                };
        }

        /****************************************************************
         * Ensure that <httpErrors> has the attribute errorMode="Custom"*
         ***************************************************************/

        private HealthCheckStatus FixMissingHttpErrorsErrorMode()
        {
            XmlDocument doc = new XmlDocument();

            doc.Load(HttpContext.Current.Server.MapPath("~/Web.config"));

            XmlNode httpErrors = doc.SelectSingleNode("/configuration/system.webServer/httpErrors");

            XmlAttribute errorMode = httpErrors.Attributes["errorMode"];

            if (errorMode != null)
            {
                errorMode.Value = "Custom";
            }
            else
            {
                ((XmlElement)httpErrors).SetAttribute("errorMode", "Custom");
            }

            doc.Save(HttpContext.Current.Server.MapPath("~/Web.config"));

            string message = _textService.Localize("customErrorsHealthCheck/httpErrorsErrorModeCheckSuccess");

            return
                new HealthCheckStatus(message)
                {
                    ResultType = StatusResultType.Success
                };
        }

        /***************************************************************************************************
         * Ensure that a <remove statusCode="404" subStatusCode="-1"> element is present under <httpErrors>*
         **************************************************************************************************/

        private HealthCheckStatus FixMissingRemove404()
        {
            XmlDocument doc = new XmlDocument();

            doc.Load(HttpContext.Current.Server.MapPath("~/Web.config"));

            XmlNode httpErrors = doc.SelectSingleNode("/configuration/system.webServer/httpErrors");

            XmlElement remove404 = doc.CreateElement("remove");

            remove404.SetAttribute("statusCode", "404");

            remove404.SetAttribute("subStatusCode", "-1");

            httpErrors.AppendChild(remove404);

            doc.Save(HttpContext.Current.Server.MapPath("~/Web.config"));

            string message = _textService.Localize("customErrorsHealthCheck/remove404ElementSuccess");

            var actions = new List<HealthCheckAction>();

            actions.Add(new HealthCheckAction("checkHttpErrorsChildNodes", Id)
            { Name = "Check other httpErrors child elements", Description = "" });

            return
                new HealthCheckStatus(message)
                {
                    ResultType = StatusResultType.Success,
                    Actions = actions
                };
        }

        /**************************************************************************************************
         *Ensure that a <remove statusCode="500" subStatusCode="-1"> element is present under <httpErrors>*
         *************************************************************************************************/

        private HealthCheckStatus FixMissingRemove500()
        {
            XmlDocument doc = new XmlDocument();

            doc.Load(HttpContext.Current.Server.MapPath("~/Web.config"));

            XmlNode httpErrors = doc.SelectSingleNode("/configuration/system.webServer/httpErrors");

            XmlElement remove500 = doc.CreateElement("remove");

            remove500.SetAttribute("statusCode", "500");

            remove500.SetAttribute("subStatusCode", "-1");

            httpErrors.AppendChild(remove500);

            doc.Save(HttpContext.Current.Server.MapPath("~/Web.config"));

            string message = _textService.Localize("customErrorsHealthCheck/remove500ElementSuccess");

            var actions = new List<HealthCheckAction>();

            actions.Add(new HealthCheckAction("checkHttpErrorsChildNodes", Id)
            { Name = "Check other httpErrors child elements", Description = "" });

            return
                new HealthCheckStatus(message)
                {
                    ResultType = StatusResultType.Success,
                    Actions = actions
                };
        }

        /*****************************************************************************************************************************************************
         *Ensure that a <error statusCode="404" prefixLanguageFilePath="" path="YOUR_ERROR404_VIEW" responseMode="ExecuteURL"/> is present under <httpErrors>*
         ****************************************************************************************************************************************************/

        private HealthCheckStatus FixMissingHttpError404()
        {
            XmlDocument doc = new XmlDocument();

            doc.Load(HttpContext.Current.Server.MapPath("~/Web.config"));

            XmlNode httpErrors = doc.SelectSingleNode("/configuration/system.webServer/httpErrors");

            XmlElement error404 = doc.CreateElement("error");

            error404.SetAttribute("statusCode", "404");

            error404.SetAttribute("prefixLanguageFilePath", "");

            error404.SetAttribute("path", "YOUR_ERROR404_VIEW");

            error404.SetAttribute("responseMode", "ExecuteURL");

            httpErrors.AppendChild(error404);

            doc.Save(HttpContext.Current.Server.MapPath("~/Web.config"));

            string message = _textService.Localize("customErrorsHealthCheck/httpError404ElementSuccess");

            var actions = new List<HealthCheckAction>();

            actions.Add(new HealthCheckAction("checkHttpErrorsChildNodes", Id)
            { Name = "Check other httpErrors child elements", Description = "" });

            return
                new HealthCheckStatus(message)
                {
                    ResultType = StatusResultType.Success,
                    Actions = actions
                };
        }

        /*************************************************************************************************************************************
         *Ensure that a <error statusCode="500" prefixLanguageFilePath="" path="500.html" responseMode="File"/> is present under <httpErrors>*
         ************************************************************************************************************************************/

        private HealthCheckStatus FixMissingHttpError500()
        {
            XmlDocument doc = new XmlDocument();

            doc.Load(HttpContext.Current.Server.MapPath("~/Web.config"));

            XmlNode httpErrors = doc.SelectSingleNode("/configuration/system.webServer/httpErrors");

            XmlElement error500 = doc.CreateElement("error");

            error500.SetAttribute("statusCode", "500");

            error500.SetAttribute("prefixLanguageFilePath", "");

            error500.SetAttribute("path", "500.html");

            error500.SetAttribute("responseMode", "File");

            httpErrors.AppendChild(error500);

            doc.Save(HttpContext.Current.Server.MapPath("~/Web.config"));

            string message = _textService.Localize("customErrorsHealthCheck/httpError500ElementSuccess");

            var actions = new List<HealthCheckAction>();

            actions.Add(new HealthCheckAction("checkHttpErrorsChildNodes", Id)
            { Name = "Check other httpErrors child elements", Description = "" });

            return
                new HealthCheckStatus(message)
                {
                    ResultType = StatusResultType.Success,
                    Actions = actions
                };
        }

        /***************************************************
         *Add all four required child nodes to <httpErrors>*
         **************************************************/

        private HealthCheckStatus FixAllMissingHttpErrorsElements()
        {
            FixMissingRemove404();

            FixMissingRemove500();

            FixMissingHttpError404();

            FixMissingHttpError500();

            string message = _textService.Localize("customErrorsHealthCheck/httpErrorsChildNodesCheckSuccess");

            return
                new HealthCheckStatus(message)
                {
                    ResultType = StatusResultType.Success
                };
        }

        private HealthCheckStatus FixMissingUmbracoSettings404Node()
        {
            XmlDocument doc = new XmlDocument();

            doc.Load(HttpContext.Current.Server.MapPath("~/config/umbracoSettings.config"));

            XmlNode errors = doc.SelectSingleNode("/settings/content/errors");

            XmlElement error404 = doc.CreateElement("error404");

            error404.InnerText = "YOUR_ERROR404_NODE_ID_HERE";

            if(errors != null)
            {
                errors.AppendChild(error404);
            }
            else
            {
                XmlNode newErrorsNode = doc.CreateNode(XmlNodeType.Element, "errors", "");

                newErrorsNode.AppendChild(error404);

                doc.SelectSingleNode("/settings/content").AppendChild(newErrorsNode);
            }

            doc.Save(HttpContext.Current.Server.MapPath("~/config/umbracoSettings.config"));

            string message = _textService.Localize("/customErrorsHealthCheck/umbracoSettings404NodeAdded");

            return
                new HealthCheckStatus(message)
                {
                    ResultType = StatusResultType.Success
                };
        }
    }
}