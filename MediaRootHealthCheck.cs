using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Umbraco.Core;
using Umbraco.Core.Services;
using Umbraco.Web.Composing;

namespace Umbraco.Web.HealthCheck.Checks.Media
{
    [HealthCheck("35C95219-DE87-4AFE-94D0-F8CE63707BB2", "Media Root Check", Description = "Checks that all media items are inside folders, and not in the root.", Group = "Media")]
    public class MediaRootHealthCheck : HealthCheck
    {
        private readonly ILocalizedTextService _textService;

        public MediaRootHealthCheck(ILocalizedTextService textService)
        {
            _textService = textService;
        }

        public override IEnumerable<HealthCheckStatus> GetStatus()
        {
            return new[] { CheckMediaRoot() };
        }

        public override HealthCheckStatus ExecuteAction(HealthCheckAction action)
        {
            switch (action.Alias)
            {
                case "checkMediaRoot":
                    return CheckMediaRoot();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private HealthCheckStatus CheckMediaRoot()
        {
            bool success = true;

            string message = string.Empty;

            IMediaService mediaService = Current.Services.MediaService;

            var rootMedia = mediaService.GetRootMedia();

            foreach(var item in rootMedia)
            {
                if(item.ContentType.Name == "Image")
                {
                    success = false;
                }
            }

            message = success ? _textService.Localize("/mediaRootHealthCheck/mediaRootHealthCheckSuccess") : _textService.Localize("/mediaRootHealthCheck/mediaRootHealthCheckFailed");

            return
                new HealthCheckStatus(message)
                {
                    ResultType = success ? StatusResultType.Success : StatusResultType.Error
                };
        }
    }
}