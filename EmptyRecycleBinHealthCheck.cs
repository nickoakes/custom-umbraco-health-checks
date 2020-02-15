using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.Hosting;
using Umbraco.Core.Logging;
using Umbraco.Core.Services;
using Umbraco.Core;
using Umbraco.Web.Composing;

namespace Umbraco.Web.HealthCheck.Checks.RecycleBin
{
    [HealthCheck("EFDCAA20-EE19-403C-B483-F4800EF9191D", "Recycle Bin", Description = "Checks that the recycle bin has been emptied.", Group = "Recycle Bin")]
    public class EmptyRecycleBinHealthCheck : HealthCheck
    {
        private readonly ILocalizedTextService _textService;

        public EmptyRecycleBinHealthCheck(ILocalizedTextService textService)
        {
            _textService = textService;
        }

        public override IEnumerable<HealthCheckStatus> GetStatus()
        {
            return new[] { CheckRecycleBinIsEmpty() };
        }

        public override HealthCheckStatus ExecuteAction(HealthCheckAction action)
        {
            switch (action.Alias)
            {
                case "emptyRecycleBin":
                    return EmptyRecycleBin();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private HealthCheckStatus CheckRecycleBinIsEmpty()
        {
            IContentService contentService = Current.Services.ContentService;

            IMediaService mediaService = Current.Services.MediaService;

            bool success = contentService.CountChildren(Constants.System.RecycleBinContent) == 0 && mediaService.CountChildren(Constants.System.RecycleBinMedia) == 0;

            var message = success
                ? _textService.Localize("recycleBinHealthCheck/recycleBinEmptyCheckSuccess")
                : _textService.Localize("recycleBinHealthCheck/recycleBinEmptyCheckFailed");

            var actions = new List<HealthCheckAction>();

            if (!success)
            {
                actions.Add(new HealthCheckAction("emptyRecycleBin", Id)
                { Name = _textService.Localize("recycleBinHealthCheck/emptyRecycleBinMessage") });
            }

            return
                new HealthCheckStatus(message)
                {
                    ResultType = success ? StatusResultType.Success : StatusResultType.Error,
                    Actions = actions
                };
        }

        private HealthCheckStatus EmptyRecycleBin()
        {
            IMediaService mediaService = Current.Services.MediaService;

            var userService = Current.Services.UserService;

            var currentUserId = userService.GetByUsername(HttpContext.Current.User.Identity.Name).Id;

            mediaService.EmptyRecycleBin(currentUserId);

            string message = _textService.Localize("recycleBinHealthCheck/recycleBinEmptyCheckSuccess");

            return
                new HealthCheckStatus(message)
                {
                    ResultType = StatusResultType.Success
                };
        }
    }
}