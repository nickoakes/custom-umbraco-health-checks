using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.Hosting;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web.Composing;

namespace Umbraco.Web.HealthCheck.Checks.DocumentTypes
{
    [HealthCheck("345B8686-A220-41EC-8F3D-7E6E9924910F", "Document Type Icons", Description = "Checks that all document types have been given an icon.", Group = "Document Types")]
    public class DocumentTypeIconHealthCheck : HealthCheck
    {
        private readonly ILocalizedTextService _textService;

        public DocumentTypeIconHealthCheck(ILocalizedTextService textService)
        {
            _textService = textService;
        }

        public override IEnumerable<HealthCheckStatus> GetStatus()
        {
            return new[] { CheckDocumentTypeIcons() };
        }

        public override HealthCheckStatus ExecuteAction(HealthCheckAction action)
        {
            switch (action.Alias)
            {
                case "checkDocumentTypeIcons":
                    return CheckDocumentTypeIcons();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private HealthCheckStatus CheckDocumentTypeIcons()
        {
            var success = true;

            IContentTypeService contentTypeService = Current.Services.ContentTypeService;

            var docTypesWithDefaultIcon = new List<string>();

            foreach(IContentType contentType in contentTypeService.GetAll())
            {
                if(contentType.Icon == "icon-document")
                {
                    success = false;

                    docTypesWithDefaultIcon.Add(contentType.Name);
                }
            }

            string docTypesWithDefaultIconNames = string.Empty;

            foreach(var docTypeName in docTypesWithDefaultIcon)
            {
                docTypesWithDefaultIconNames += docTypeName + ", ";
            }

            var message = success
                ? _textService.Localize("documentTypeIconHealthCheck/documentTypeIconCheckSuccess")
                : _textService.Localize("documentTypeIconHealthCheck/documentTypeIconCheckFailed") + " " + docTypesWithDefaultIconNames;

            var actions = new List<HealthCheckAction>();

            return
                new HealthCheckStatus(message)
                {
                    ResultType = success ? StatusResultType.Success : StatusResultType.Error,
                    Actions = actions
                };
        }
    }
}