﻿using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using PostSharp.Patterns.Contracts;
using ThreatsManager.Engine.Aspects;
using ThreatsManager.Interfaces.Extensions;
using ThreatsManager.Interfaces.ObjectModel.Properties;
using ThreatsManager.Utilities;
using ThreatsManager.Utilities.Aspects;
using ThreatsManager.Utilities.Aspects.Engine;

namespace ThreatsManager.Engine.ObjectModel.Properties
{
    [JsonObject(MemberSerialization.OptIn)]
    [SimpleNotifyPropertyChanged]
    [AutoDirty]
    [Serializable]
    [IdentityAspect]
    [PropertyTypeAspect]
    public class BaseListPropertyType : IPropertyType
    {
        public BaseListPropertyType()
        {

        }

        public BaseListPropertyType([Required] string name, [NotNull] IPropertySchema schema) : this()
        {
            _id = Guid.NewGuid();
            _schemaId = schema.Id;
            Name = name;
            Visible = true;
        }

        #region Additional placeholders required.
        protected Guid _id { get; set; }
        protected Guid _schemaId { get; set; }
        #endregion

        #region Default implementation.
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Guid SchemaId { get; set; }
        public int Priority { get; set; }
        public bool Visible { get; set; }
        #endregion

        #region Specific implementation.
        /// <summary>
        /// Identifier of the List Provider that provides the list of available items.
        /// </summary>
        [JsonProperty("listProvider")]
        protected string _listProviderId;

        private IListProviderExtension _listProvider;

        private IListProviderExtension ListProvider
        {
            get
            {
                if (_listProvider == null && !string.IsNullOrWhiteSpace(_listProviderId))
                {
                    _listProvider = Manager.Instance.GetExtension<IListProviderExtension>(_listProviderId);
                }

                return _listProvider;
            }
        }

        public void SetListProvider([NotNull] IListProviderExtension listProvider)
        {
            _listProviderId = listProvider.GetExtensionId();
            _listProvider = listProvider;
        }

        [JsonProperty("context")]
        public string Context { get; set; }

        [JsonProperty("cachedList")]
        protected IEnumerable<IListItem> _cachedList;

        public IEnumerable<IListItem> Values
        {
            get
            {
                IEnumerable<IListItem> result = null;

                var listProvider = ListProvider;

                if (listProvider != null)
                {
                    result = listProvider.GetAvailableItems(Context);
                    _cachedList = result?.ToArray();
                }
                else
                {
                    result = _cachedList;
                }

                return result;
            }
        }

        public override string ToString()
        {
            return Name;
        }

        public virtual IPropertyType Clone(IPropertyTypesContainer container)
        {
            return null;
        }
        #endregion
    }
}