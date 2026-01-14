using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using EPiServer.Core;
using EPiServer.DataAbstraction;

namespace Cmsql.Optimizely.Internal
{
    internal class PropertyDataTypeResolver
    {
        private static readonly IDictionary<string, PropertyDataType>
            MetaDataPropertyTypeMappings = new Dictionary<string, PropertyDataType>
            {
                {MetaDataProperties.PageLink, PropertyDataType.PageReference},
                {MetaDataProperties.PageTypeID, PropertyDataType.PageType},
                {MetaDataProperties.PageParentLink, PropertyDataType.PageReference},
                {MetaDataProperties.PagePendingPublish, PropertyDataType.Boolean},
                {MetaDataProperties.PageWorkStatus, PropertyDataType.Number},
                {MetaDataProperties.PageDeleted, PropertyDataType.Boolean},
                {MetaDataProperties.PageSaved, PropertyDataType.Boolean},
                {MetaDataProperties.PageTypeName, PropertyDataType.String},
                {MetaDataProperties.PageChanged, PropertyDataType.Date},
                {MetaDataProperties.PageCreatedBy, PropertyDataType.String},
                {MetaDataProperties.PageChangedBy, PropertyDataType.String},
                {MetaDataProperties.PageDeletedBy, PropertyDataType.String},
                {MetaDataProperties.PageDeletedDate, PropertyDataType.Date},
                {MetaDataProperties.PageCreated, PropertyDataType.Date},
                {MetaDataProperties.PageMasterLanguageBranch, PropertyDataType.String},
                {MetaDataProperties.PageLanguageBranch, PropertyDataType.String},
                {MetaDataProperties.PageGUID, PropertyDataType.String},
                {MetaDataProperties.PageContentAssetsID, PropertyDataType.String},
                {MetaDataProperties.PageContentOwnerID, PropertyDataType.String},
                //{MetaDataProperties.PageFolderID, PropertyDataType.Number},
                {MetaDataProperties.PageVisibleInMenu, PropertyDataType.Boolean},
                {MetaDataProperties.PageURLSegment, PropertyDataType.String},
                {MetaDataProperties.PagePeerOrder, PropertyDataType.Number},
                {MetaDataProperties.PageExternalURL, PropertyDataType.String},
                {MetaDataProperties.PageChangedOnPublish, PropertyDataType.Boolean},
                {MetaDataProperties.PageCategory, PropertyDataType.Category},
                {MetaDataProperties.PageStartPublish, PropertyDataType.Date},
                {MetaDataProperties.PageStopPublish, PropertyDataType.Date},
                {MetaDataProperties.PageArchiveLink, PropertyDataType.PageReference},
                {MetaDataProperties.PageShortcutType, PropertyDataType.Number},
                {MetaDataProperties.PageShortcutLink, PropertyDataType.PageReference},
                {MetaDataProperties.PageTargetFrame, PropertyDataType.Number},
                {MetaDataProperties.PageLinkURL, PropertyDataType.String},
                {MetaDataProperties.PageName, PropertyDataType.String}
            };

        private readonly ContentType _contentType;

        internal PropertyDataTypeResolver(ContentType contentType)
        {
            _contentType = contentType;
        }

        internal bool TryResolve(string propertyIdentifier, out PropertyDataType propertyDataType)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(propertyIdentifier));

            propertyDataType = PropertyDataType.String;
            if (MetaDataPropertyTypeMappings.ContainsKey(propertyIdentifier))
            {
                propertyDataType = MetaDataPropertyTypeMappings[propertyIdentifier];
                return true;
            }

            PropertyDefinition propDef = _contentType
                .PropertyDefinitions
                .SingleOrDefault(prop =>
                    prop.Name.Equals(propertyIdentifier, StringComparison.InvariantCultureIgnoreCase));
            if (propDef != null)
            {
                propertyDataType = propDef.Type.DataType;
                return true;
            }

            return false;
        }
    }
}
