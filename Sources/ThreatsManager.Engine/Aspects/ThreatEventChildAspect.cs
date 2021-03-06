﻿using System;
using System.Linq;
using Newtonsoft.Json;
using PostSharp.Aspects;
using PostSharp.Aspects.Advices;
using PostSharp.Aspects.Dependencies;
using PostSharp.Reflection;
using PostSharp.Serialization;
using ThreatsManager.Interfaces.ObjectModel;
using ThreatsManager.Interfaces.ObjectModel.ThreatsMitigations;
using ThreatsManager.Utilities.Aspects.Engine;

namespace ThreatsManager.Engine.Aspects
{
    //#region Additional placeholders required.
    //private Guid _threatEventId { get; set; }
    //private IThreatEvent _threatEvent { get; set; }
    //#endregion    

    [PSerializable]
    [AspectTypeDependency(AspectDependencyAction.Require, AspectDependencyPosition.Any, typeof(ThreatModelChildAspect))]
    public class ThreatEventChildAspect : InstanceLevelAspect
    {
        #region Imports from the extended class.
        [ImportMember("Model", IsRequired=true)]
        public Property<IThreatModel> Model;
        #endregion

        #region Extra elements to be added.
        [IntroduceMember(OverrideAction = MemberOverrideAction.OverrideOrFail, 
            LinesOfCodeAvoided = 1, Visibility = Visibility.Private)]
        [CopyCustomAttributes(typeof(JsonPropertyAttribute), 
            OverrideAction = CustomAttributeOverrideAction.MergeReplaceProperty)]
        [JsonProperty("threatEventId")]
        public Guid _threatEventId { get; set; }

        [IntroduceMember(OverrideAction = MemberOverrideAction.OverrideOrFail, 
            LinesOfCodeAvoided = 0, Visibility = Visibility.Private)]
        public IThreatEvent _threatEvent { get; set; }
        #endregion

        #region Implementation of interface IThreatEventChild.
        [IntroduceMember(OverrideAction = MemberOverrideAction.OverrideOrFail, LinesOfCodeAvoided = 1)]
        public IThreatEvent ThreatEvent
        {
            get
            {
                if (_threatEvent == null)
                {
                    var entities = Model?.Get().Entities?.ToArray();
                    if (entities?.Any() ?? false)
                    {
                        foreach (var entity in entities)
                        {
                            _threatEvent = entity.ThreatEvents?.FirstOrDefault(x => x.Id == _threatEventId);
                            if (_threatEvent != null)
                                break;
                        }
                    }
                }

                if (_threatEvent == null)
                {
                    var dataFlows = Model?.Get().DataFlows?.ToArray();
                    if (dataFlows?.Any() ?? false)
                    {
                        foreach (var dataFlow in dataFlows)
                        {
                            _threatEvent = dataFlow.ThreatEvents?.FirstOrDefault(x => x.Id == _threatEventId);
                            if (_threatEvent != null)
                                break;
                        }
                    }
                }

                if (_threatEvent == null)
                {
                    _threatEvent = Model?.Get().ThreatEvents?.FirstOrDefault(x => x.Id == _threatEventId);
                }

                return _threatEvent;
            }
        }
        #endregion
    }
}
