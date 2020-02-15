using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web.Composing;

namespace Umbraco.Web.HealthCheck.Checks.ImageAltTagProperty
{
    [HealthCheck("99228088-6A96-4FE5-90DD-40F88DD2D5A8", "Image Alt Tag Property Check", Description = "Checks that the image media type has a property through which alt tag text can be added to images.", Group = "Media")]
    public class ImageAltTagPropertyHealthCheck : HealthCheck
    {
        private readonly ILocalizedTextService _textService;

        public ImageAltTagPropertyHealthCheck(ILocalizedTextService textService)
        {
            _textService = textService;
        }

        public override IEnumerable<HealthCheckStatus> GetStatus()
        {
            return new[] { CheckImageAltTagProperty() };
        }

        public override HealthCheckStatus ExecuteAction(HealthCheckAction action)
        {
            switch (action.Alias)
            {
                case "fixMissingImageAltTagProperty":
                    return FixMissingImageAltTagProperty();
                case "checkImagesForAltValues":
                    return CheckImagesForAltValues();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /***********************************************************************
         *Check that some kind of 'alt' property exists on the Image media type*
         **********************************************************************/

        private HealthCheckStatus CheckImageAltTagProperty()
        {
            bool success = false;

            string message = string.Empty;

            var actions = new List<HealthCheckAction>();

            IMediaTypeService mediaTypeService = Current.Services.MediaTypeService;

            IMediaType imageMediaType = mediaTypeService.GetAll().Where(x => x.Name == "Image").First();

            if(imageMediaType != null)
            {
                IEnumerable<PropertyType> propertyTypes = imageMediaType.PropertyTypes;
                
                foreach(var type in propertyTypes)
                {
                    if (type.Name.ToLower().Contains("alt"))
                    {
                        success = true;
                    }
                }
            }

            if (success)
            {
                message = _textService.Localize("imageAltTagPropertyHealthCheck/imageAltTagPropertyCheckSuccess");

                actions.Add(new HealthCheckAction("checkImagesForAltValues", Id)
                { Name = _textService.Localize("imageAltTagPropertyHealthCheck/CheckImagesForAltValuesMessage"), Description = _textService.Localize("imageAltTagPropertyHealthCheck/CheckImagesForAltValuesDescription") });
            }
            else
            {
                message = _textService.Localize("imageAltTagPropertyHealthCheck/imageAltTagPropertyCheckFailed");


                actions.Add(new HealthCheckAction("fixMissingImageAltTagProperty", Id)
                { Name = _textService.Localize("imageAltTagPropertyHealthCheck/fixMissingImageAltTagPropertyMessage"), Description = _textService.Localize("imageAltTagPropertyHealthCheck/fixMissingImageAltTagPropertyDescription") });
            }

            return
                new HealthCheckStatus(message)
                {
                    ResultType = success ? StatusResultType.Success : StatusResultType.Error,
                    Actions = actions
                };
        }

        /*******************************************************************************************
         *Check that all of the images in the Media section have a value for the 'alt tag' property*
         ******************************************************************************************/

        private HealthCheckStatus CheckImagesForAltValues()
        {
            bool success = true;

            IMediaService mediaService = Current.Services.MediaService;

            IMediaTypeService mediaTypeService = Current.Services.MediaTypeService;

            IMediaType imageMediaType = mediaTypeService.GetAll().Where(x => x.Name == "Image").First();

            PropertyType altTag = null;

            if (imageMediaType != null)
            {
                IEnumerable<PropertyType> propertyTypes = imageMediaType.PropertyTypes;

                foreach (var type in propertyTypes)
                {
                    if (type.Name.ToLower().Contains("alt"))
                    {
                        altTag = type;
                    }
                }
            }

            int nestingLevel = 0;

            for(int i = 1; i < 100; i++)
            {
                bool levelExists = (mediaService.GetByLevel(i).Any());

                if (!levelExists)
                {
                    nestingLevel = i;
                    break;
                }
            }

            var images = new List<IMedia>();

            for(int j = 1; j <= nestingLevel; j++)
            {
                IEnumerable<IMedia> itemsInLevel = mediaService.GetByLevel(j);

                foreach(var item in itemsInLevel)
                {
                    if(item.ContentType.Name == "Image")
                    {
                        images.Add(item);
                    }
                }
            }

            var imagesWithoutAltValue = new List<string>();

            foreach(IMedia image in images)
            {
                foreach(var prop in image.Properties)
                {
                    if(prop.Alias == altTag.Alias)
                    {
                        if (prop.Values.Count() == 0)
                        {
                            success = false;

                            imagesWithoutAltValue.Add(image.Name);
                        }
                    }
                }
            }

            string imagesWithoutAltValueNames = string.Empty;

            foreach(var image in imagesWithoutAltValue)
            {
                imagesWithoutAltValueNames += image + ", ";
            }

            string message = success ? _textService.Localize("imageAltTagPropertyHealthCheck/checkImagesForAltValuesSuccess") : _textService.Localize("imageAltTagPropertyHealthCheck/checkImagesForAltValuesFailed") + " " + imagesWithoutAltValueNames;

            return
                new HealthCheckStatus(message)
                {
                    ResultType = success ? StatusResultType.Success : StatusResultType.Error
                };
        }

        /******************************************************
         *Create an 'alt tag' property on the Image media type*
         *****************************************************/

        private HealthCheckStatus FixMissingImageAltTagProperty()
        {
            IMediaTypeService mediaTypeService = Current.Services.MediaTypeService;

            IMediaType imageMediaType = mediaTypeService.GetAll().Where(x => x.Alias.ToLower() == "image").First();

            IDataTypeService dataTypeService = Current.Services.DataTypeService;

            IDataType textBox = dataTypeService.GetByEditorAlias("Umbraco.TextBox").First();

            PropertyType altTag = new PropertyType(textBox);

            altTag.Name = "Alt Tag";

            altTag.Alias = "altTag";

            altTag.Description = "Enter text for the image alt tag.";

            imageMediaType.AddPropertyType(altTag, "Image");

            mediaTypeService.Save(imageMediaType);

            string message = _textService.Localize("imageAltTagPropertyHealthCheck/imageAltTagPropertyAdded");

            return
                new HealthCheckStatus(message)
                {
                    ResultType = StatusResultType.Info
                };
        }
    }
}