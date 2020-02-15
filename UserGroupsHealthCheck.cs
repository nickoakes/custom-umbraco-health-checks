using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Services;
using Umbraco.Web.Composing;

namespace Umbraco.Web.HealthCheck.Checks.UserGroups
{
    [HealthCheck("86367CEB-575F-43B4-8E54-58969570560A", "User Groups Check", Description = "Checks that all required user groups have been created.", Group = "User Groups")]
    public class UserGroupsHealthCheck : HealthCheck
    {
        private readonly ILocalizedTextService _textService;

        public UserGroupsHealthCheck(ILocalizedTextService textService)
        {
            _textService = textService;
        }

        public override IEnumerable<HealthCheckStatus> GetStatus()
        {
            return new[] { CheckUserGroups() };
        }

        public override HealthCheckStatus ExecuteAction(HealthCheckAction action)
        {
            switch (action.Alias)
            {
                case "checkAdministratorAllowedSections":
                    return CheckAdministratorAllowedSections();
                case "fixMissingUserGroups":
                    return FixMissingUserGroups();
                case "fixAdministratorAllowedSections":
                    return FixAdministratorAllowedSections();
                case "checkMarketingAllowedSections":
                    return CheckMarketingAllowedSections();
                case "fixMarketingAllowedSections":
                    return FixMarketingAllowedSections();
                case "checkEndUsersAllowedSections":
                    return CheckEndUsersAllowedSections();
                case "fixEndUsersAllowedSections":
                    return FixEndUsersAllowedSections();
                case "checkThirdPartyAllowedSections":
                    return CheckThirdPartyAllowedSections();
                case "fixThirdPartyAllowedSections":
                    return FixThirdPartyAllowedSections();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /***************************************************************************************
         *Check that user groups exist for Administrators, Marketing, End Users and Third Party*
         **************************************************************************************/

        private HealthCheckStatus CheckUserGroups()
        {
            bool success = false;

            string message = string.Empty;

            var actions = new List<HealthCheckAction>();

            IUserService userService = Current.Services.UserService;

            var userGroups = userService.GetAllUserGroups();

            bool adminGroup = false;

            bool marketingGroup = false;

            bool endUsersGroup = false;

            bool thirdPartyGroup = false;

            foreach (var group in userGroups)
            {
                if(group.Name.ToLower() == "administrators")
                {
                    adminGroup = true;
                }
                if(group.Name.ToLower() == "marketing")
                {
                    marketingGroup = true;
                }
                if(group.Name.ToLower() == "end users")
                {
                    endUsersGroup = true;
                }
                if(group.Name.ToLower() == "third party")
                {
                    thirdPartyGroup = true;
                }
            }

            if (adminGroup && marketingGroup && endUsersGroup && thirdPartyGroup)
            {
                success = true;

                message = _textService.Localize("userGroupsHealthCheck/userGroupsCheckSuccess");

                actions.Add(new HealthCheckAction("checkAdministratorAllowedSections", Id)
                { Name = _textService.Localize("userGroupsHealthCheck/administratorAllowedSectionsMessage"), Description = _textService.Localize("userGroupsHealthCheck/administratorAllowedSectionsDescription") });
            }

            if (success == false)
            {
                message = _textService.Localize("userGroupsHealthCheck/userGroupsCheckFailed");

                actions.Add(new HealthCheckAction("fixMissingUserGroups", Id)
                { Name = _textService.Localize("userGroupsHealthCheck/fixMissingUserGroupsMessage"), Description = _textService.Localize("userGroupsHealthCheck/fixMissingUserGroupsDescription") });
            }

            return
                new HealthCheckStatus(message)
                {
                    ResultType = success ? StatusResultType.Success : StatusResultType.Error,
                    Actions = actions
                };
        }

        /*********************************************************************
         *Check that the Administrator user groups has access to all sections*
         ********************************************************************/

        private HealthCheckStatus CheckAdministratorAllowedSections()
        {
            bool success;

            string message = string.Empty;

            var actions = new List<HealthCheckAction>();

            IUserService userService = Current.Services.UserService;

            IUserGroup adminGroup = userService.GetAllUserGroups().Where(x => x.Name == "Administrators").First();

            IEnumerable<string> allowedSections = adminGroup.AllowedSections;

            if(allowedSections.Count() == Current.SectionService.GetSections().Count())
            {
                success = true;

                message = _textService.Localize("userGroupsHealthCheck/administratorAllowedSectionsSuccess");

                actions.Add(new HealthCheckAction("checkMarketingAllowedSections", Id)
                { Name = _textService.Localize("userGroupsHealthCheck/marketingAllowedSectionsMessage"), Description = _textService.Localize("userGroupsHealthCheck/marketingAllowedSectionsDescription") });
            }
            else
            {
                success = false;

                message = _textService.Localize("userGroupsHealthCheck/administratorAllowedSectionsFailed");

                actions.Add(new HealthCheckAction("fixAdministratorAllowedSections", Id)
                { Name = _textService.Localize("userGroupsHealthCheck/fixAllowedSectionsMessage"), Description = _textService.Localize("userGroupsHealthCheck/fixAdministratorAllowedSectionsDescription") });
            }

            return
                new HealthCheckStatus(message)
                {
                    ResultType = success ? StatusResultType.Success : StatusResultType.Error,
                    Actions = actions
                };
        }

        /*********************************************************************************
         *Check that the Marketing user group has access to Content, Media and Forms only*
         ********************************************************************************/

        private HealthCheckStatus CheckMarketingAllowedSections()
        {
            bool success = true;

            string message = string.Empty;

            var actions = new List<HealthCheckAction>();

            IUserService userService = Current.Services.UserService;

            IUserGroup marketingGroup = userService.GetAllUserGroups().Where(x => x.Name == "Marketing").First();

            IEnumerable<string> allowedSections = marketingGroup.AllowedSections;

            if(allowedSections.Count() != 3)
            {
                success = false;
            }
            else
            {
                foreach(var section in allowedSections)
                {
                    if(!(section == "content" || section == "media" || section == "forms"))
                    {
                        success = false;
                    }
                }
            }

            if (success)
            {
                message = _textService.Localize("userGroupsHealthCheck/marketingAllowedSectionsSuccess");

                actions.Add(new HealthCheckAction("checkEndUsersAllowedSections", Id)
                { Name = _textService.Localize("userGroupsHealthCheck/endUsersAllowedSectionsMessage"), Description = _textService.Localize("userGroupsHealthCheck/endUsersAllowedSectionsDescription") });
            }
            else
            {
                message = _textService.Localize("userGroupsHealthCheck/marketingAllowedSectionsFailed");

                actions.Add(new HealthCheckAction("fixMarketingAllowedSections", Id)
                { Name = _textService.Localize("userGroupsHealthCheck/fixAllowedSectionsMessage"), Description = _textService.Localize("userGroupsHealthCheck/fixMarketingAllowedSectionsDescription") });
            }

            return
                new HealthCheckStatus(message)
                {
                    ResultType = success ? StatusResultType.Success : StatusResultType.Error,
                    Actions = actions
                };
        }

        /*********************************************************************************
         *Check that the End Users user group has access to Content, Media and Forms only*
         ********************************************************************************/

        private HealthCheckStatus CheckEndUsersAllowedSections()
        {
            bool success = true;

            string message = string.Empty;

            var actions = new List<HealthCheckAction>();

            IUserService userService = Current.Services.UserService;

            IUserGroup endUsersGroup = userService.GetAllUserGroups().Where(x => x.Name == "End Users").First();

            IEnumerable<string> allowedSections = endUsersGroup.AllowedSections;

            if (allowedSections.Count() != 3)
            {
                success = false;
            }
            else
            {
                foreach (var section in allowedSections)
                {
                    if (!(section == "content" || section == "media" || section == "forms"))
                    {
                        success = false;
                    }
                }
            }

            if (success)
            {
                message = _textService.Localize("userGroupsHealthCheck/endUsersAllowedSectionsSuccess");

                actions.Add(new HealthCheckAction("checkThirdPartyAllowedSections", Id)
                { Name = _textService.Localize("userGroupsHealthCheck/thirdPartyAllowedSectionsMessage"), Description = _textService.Localize("userGroupsHealthCheck/thirdPartyAllowedSectionsDescription") });
            }
            else
            {
                message = _textService.Localize("userGroupsHealthCheck/endUsersAllowedSectionsFailed");

                actions.Add(new HealthCheckAction("fixEndUsersAllowedSections", Id)
                { Name = _textService.Localize("userGroupsHealthCheck/fixAllowedSectionsMessage"), Description = _textService.Localize("userGroupsHealthCheck/fixEndUsersAllowedSectionsDescription") });
            }

            return
                new HealthCheckStatus(message)
                {
                    ResultType = success ? StatusResultType.Success : StatusResultType.Error,
                    Actions = actions
                };
        }

        /***************************************************************************************************
         *Check that the Third Party user group has access to everything but the Users and Members sections*
         **************************************************************************************************/

        private HealthCheckStatus CheckThirdPartyAllowedSections()
        {
            bool success = true;

            string message = string.Empty;

            var actions = new List<HealthCheckAction>();

            IUserService userService = Current.Services.UserService;

            IUserGroup thirdPartyGroup = userService.GetAllUserGroups().Where(x => x.Name == "Third Party").First();

            IEnumerable<string> allowedSections = thirdPartyGroup.AllowedSections;

            if (allowedSections.Count() != Current.SectionService.GetSections().Count() - 2)
            {
                success = false;
            }
            else
            {
                foreach (var section in allowedSections)
                {
                    if (section == "Users" || section == "Members")
                    {
                        success = false;
                    }
                }
            }

            if (success)
            {
                message = _textService.Localize("userGroupsHealthCheck/userGroupsAllowedSectionsSuccess");
            }
            else
            {
                message = _textService.Localize("userGroupsHealthCheck/thirdPartyAllowedSectionsFailed");

                actions.Add(new HealthCheckAction("fixThirdPartyAllowedSections", Id)
                { Name = _textService.Localize("userGroupsHealthCheck/fixAllowedSectionsMessage"), Description = _textService.Localize("userGroupsHealthCheck/fixThirdPartyAllowedSectionsDescription") });
            }

            return
                new HealthCheckStatus(message)
                {
                    ResultType = success ? StatusResultType.Success : StatusResultType.Error,
                    Actions = actions
                };
        }

        /****************************************
         *Create and add any missing user groups*
         ***************************************/

        private HealthCheckStatus FixMissingUserGroups()
        {
            IUserService userService = Current.Services.UserService;

            var userGroups = userService.GetAllUserGroups();

            bool adminGroup = false;

            bool marketingGroup = false;

            bool endUsersGroup = false;

            bool thirdPartyGroup = false;

            foreach(var group in userGroups)
            {
                if (group.Name.ToLower() == "administrators")
                {
                    adminGroup = true;
                }
                if(group.Name.ToLower() == "marketing")
                {
                    marketingGroup = true;
                }
                if(group.Name.ToLower() == "end users")
                {
                    endUsersGroup = true;
                }
                if(group.Name.ToLower() == "third party")
                {
                    thirdPartyGroup = true;
                }
            }

            if (!adminGroup)
            {
                IUserGroup Administrators = new UserGroup();

                Administrators.Name = "Administrators";

                Administrators.Alias = "admin";

                userService.Save(Administrators);
            }

            if (!marketingGroup)
            {
                IUserGroup Marketing = new UserGroup();

                Marketing.Name = "Marketing";

                Marketing.Alias = "marketing";

                userService.Save(Marketing);
            }

            if (!endUsersGroup)
            {
                IUserGroup endUsers = new UserGroup();

                endUsers.Name = "End Users";

                endUsers.Alias = "endUsers";

                userService.Save(endUsers);
            }

            if (!thirdPartyGroup)
            {
                IUserGroup thirdParty = new UserGroup();

                thirdParty.Name = "Third Party";

                thirdParty.Alias = "thirdParty";

                userService.Save(thirdParty);
            }

            string message = _textService.Localize("userGroupsHealthCheck/userGroupsCheckSuccess");

            var actions = new List<HealthCheckAction>();

            actions.Add(new HealthCheckAction("checkAdministratorAllowedSections", Id)
            { Name = _textService.Localize("userGroupsHealthCheck/administratorAllowedSectionsMessage"), Description = _textService.Localize("userGroupsHealthCheck/administratorAllowedSectionsDescription") });

            return
                new HealthCheckStatus(message)
                {
                    ResultType = StatusResultType.Success,
                    Actions = actions
                };
        }

        /***************************************************************
         *Fix missing allowed sections for the Administrator user group*
         **************************************************************/

        private HealthCheckStatus FixAdministratorAllowedSections()
        {
            IUserService userService = Current.Services.UserService;

            IUserGroup adminGroup = userService.GetAllUserGroups().Where(x => x.Name == "Administrators").First();

            foreach(var section in Current.SectionService.GetSections())
            {
                if (!adminGroup.AllowedSections.Contains(section.Name))
                {
                    adminGroup.AddAllowedSection(section.Alias);

                    userService.Save(adminGroup);
                }
            }

            string message = _textService.Localize("userGroupsHealthCheck/administratorAllowedSectionsSuccess");

            var actions = new List<HealthCheckAction>();

            actions.Add(new HealthCheckAction("checkMarketingAllowedSections", Id)
            { Name = _textService.Localize("userGroupsHealthCheck/marketingAllowedSectionsMessage"), Description = _textService.Localize("userGroupsHealthCheck/marketingAllowedSectionsDescription") });

            return
                new HealthCheckStatus(message)
                {
                    ResultType = StatusResultType.Success,
                    Actions = actions
                };
        }

        /***************************************************
         *Fix allowed sections for the Marketing user group*
         **************************************************/

        private HealthCheckStatus FixMarketingAllowedSections()
        {
            IUserService userService = Current.Services.UserService;

            IUserGroup marketingGroup = userService.GetAllUserGroups().Where(x => x.Name == "Marketing").First();

            marketingGroup.ClearAllowedSections();

            foreach(var section in Current.SectionService.GetSections().Where(x => x.Name == "Content" || x.Name == "Media" || x.Name == "Forms"))
            {
                marketingGroup.AddAllowedSection(section.Alias);

                userService.Save(marketingGroup);
            }

            var actions = new List<HealthCheckAction>();

            string message = _textService.Localize("userGroupsHealthCheck/marketingAllowedSectionsSuccess");

            actions.Add(new HealthCheckAction("checkEndUsersAllowedSections", Id)
            { Name = _textService.Localize("userGroupsHealthCheck/endUsersAllowedSectionsMessage"), Description = _textService.Localize("userGroupsHealthCheck/endUsersAllowedSectionsDescription") });

            return
                new HealthCheckStatus(message)
                {
                    ResultType = StatusResultType.Success,
                    Actions = actions
                };
        }

        /***************************************************
         *Fix allowed sections for the End Users user group*
         **************************************************/

        private HealthCheckStatus FixEndUsersAllowedSections()
        {
            IUserService userService = Current.Services.UserService;

            IUserGroup endUsersGroup = userService.GetAllUserGroups().Where(x => x.Name == "End Users").First();

            endUsersGroup.ClearAllowedSections();

            foreach (var section in Current.SectionService.GetSections().Where(x => x.Name == "Content" || x.Name == "Media" || x.Name == "Forms"))
            {
                endUsersGroup.AddAllowedSection(section.Alias);

                userService.Save(endUsersGroup);
            }

            var actions = new List<HealthCheckAction>();

            string message = _textService.Localize("userGroupsHealthCheck/endUsersAllowedSectionsSuccess");

            actions.Add(new HealthCheckAction("checkThirdPartyAllowedSections", Id)
            { Name = _textService.Localize("userGroupsHealthCheck/thirdPartyAllowedSectionsMessage"), Description = _textService.Localize("userGroupsHealthCheck/thirdPartyAllowedSectionsDescription") });

            return
                new HealthCheckStatus(message)
                {
                    ResultType = StatusResultType.Success,
                    Actions = actions
                };
        }

        /*****************************************************
         *Fix allowed sections for the Third Party user group*
         ****************************************************/

        private HealthCheckStatus FixThirdPartyAllowedSections()
        {
            IUserService userService = Current.Services.UserService;

            IUserGroup thirdPartyGroup = userService.GetAllUserGroups().Where(x => x.Name == "Third Party").First();

            thirdPartyGroup.ClearAllowedSections();

            foreach (var section in Current.SectionService.GetSections().Where(x => x.Name != "Users" && x.Name != "Members"))
            {
                thirdPartyGroup.AddAllowedSection(section.Alias);

                userService.Save(thirdPartyGroup);
            }

            string message = _textService.Localize("userGroupsHealthCheck/userGroupsAllowedSectionsSuccess");

            return
                new HealthCheckStatus(message)
                {
                    ResultType = StatusResultType.Success
                };
        }
    }
}