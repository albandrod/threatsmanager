﻿using System;
using System.Collections.Generic;
using System.Linq;
using PostSharp.Patterns.Contracts;
using ThreatsManager.Interfaces.ObjectModel;
using ThreatsManager.Interfaces.ObjectModel.Entities;
using ThreatsManager.Interfaces.ObjectModel.Properties;
using ThreatsManager.Interfaces.ObjectModel.ThreatsMitigations;

namespace ThreatsManager.Utilities
{
    /// <summary>
    /// Extensions to simplify development by adding behaviors to the Threat Model and its components.
    /// </summary>
    public static class ThreatModelExtensions
    {
        /// <summary>
        /// Get the maximum severity applied to the Threat Events derived from the specific Threat Type.
        /// </summary>
        /// <param name="threatType">Threat Type to be analyzed.</param>
        /// <returns>Maximum severity applied to Threat Events derived from the Threat Type.</returns>
        public static ISeverity GetTopSeverity(this IThreatType threatType)
        {
            ISeverity result = null;

            var model = threatType.Model;

            if (model != null)
            {
                var modelTe = model.ThreatEvents?.Where(x => x.ThreatTypeId == threatType.Id)
                    .OrderByDescending(x => x.SeverityId).FirstOrDefault();
                if (modelTe != null)
                {
                    result = modelTe.Severity;
                }

                var entitiesTe = model.Entities?
                    .Select(e => e.ThreatEvents?.Where(x => x.ThreatTypeId == threatType.Id)
                        .OrderByDescending(x => x.SeverityId).FirstOrDefault())
                    .Where(x => x != null).ToArray();
                if (entitiesTe?.Any() ?? false)
                {
                    foreach (var entityTe in entitiesTe)
                    {
                        if (result == null || entityTe.SeverityId > result.Id)
                        {
                            result = entityTe.Severity;
                        }
                    }
                }

                var flowsTe = model.DataFlows?
                    .Select(e => e.ThreatEvents?.Where(x => x.ThreatTypeId == threatType.Id)
                        .OrderByDescending(x => x.SeverityId).FirstOrDefault())
                    .Where(x => x != null).ToArray();
                if (flowsTe?.Any() ?? false)
                {
                    foreach (var flowTe in flowsTe)
                    {
                        if (result == null || flowTe.SeverityId > result.Id)
                        {
                            result = flowTe.Severity;
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Verifies if the Identity should be picked, based on the filter passed as argument.
        /// </summary>
        /// <param name="identity">Identity to be analyzed.</param>
        /// <param name="filter">Filter to be applied.</param>
        /// <returns>True if any string in the filter is present in any text field of teh Identity.</returns>
        /// <remarks>It analyzes the Name, the Description and eventual Text properties.
        /// <para>The search is case-insensitive.</para></remarks>
        public static bool Filter(this IIdentity identity, [Required] string filter)
        {
            var result = (!string.IsNullOrWhiteSpace(identity.Name) &&
                          identity.Name.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0) ||
                         (!string.IsNullOrWhiteSpace(identity.Description) &&
                          identity.Description.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0);

            if (!result && identity is IPropertiesContainer container)
            {
                var properties = container.Properties?.ToArray();
                if (properties?.Any() ?? false)
                {
                    foreach (var property in properties)
                    {
                        var stringValue = property.StringValue;
                        if ((!string.IsNullOrWhiteSpace(stringValue) &&
                             stringValue.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0))
                        {
                            result = true;
                            break;
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Get the type of the Entity passed as argument.
        /// </summary>
        /// <param name="entity">Entity for which the Type should be retrieved.</param>
        /// <returns>Entity Type.</returns>
        public static EntityType GetEntityType([NotNull] this IEntity entity)
        {
            EntityType result;

            if (entity is IExternalInteractor)
                result = EntityType.ExternalInteractor;
            else if (entity is IProcess)
                result = EntityType.Process;
            else if (entity is IDataStore)
                result = EntityType.DataStore;
            else
                throw new ArgumentException("Not a known Entity.");

            return result;
        }

        /// <summary>
        /// Get the associations between Threat Events and the Mitigation passed as argument. 
        /// </summary>
        /// <param name="mitigation">Mitigation to be analyzed.</param>
        /// <returns>Enumeration of all the relationships.</returns>
        public static IEnumerable<IThreatEventMitigation> GetThreatEventMitigations(this IMitigation mitigation)
        {
            IEnumerable<IThreatEventMitigation> result = null;

            var model = mitigation?.Model;
            if (model != null)
            {
                List<IThreatEventMitigation> mitigations = new List<IThreatEventMitigation>();

                GetThreatEventMitigations(mitigation, model, mitigations);
                GetThreatEventMitigations(mitigation, model.Entities, mitigations);
                GetThreatEventMitigations(mitigation, model.DataFlows, mitigations);

                result = mitigations;
            }

            return result;
        }

        #region Auxiliary functions.
        private static void GetThreatEventMitigations([NotNull] IMitigation mitigation, IEnumerable<IThreatEventsContainer> containers,
            [NotNull] List<IThreatEventMitigation> list)
        {
            var tecs = containers?.ToArray();
            if (tecs?.Any() ?? false)
            {
                foreach (var tec in tecs)
                    GetThreatEventMitigations(mitigation, tec, list);
            }
        }

        private static void GetThreatEventMitigations([NotNull] IMitigation mitigation, [NotNull] IThreatEventsContainer container,
            [NotNull] List<IThreatEventMitigation> list)
        {
            var tes = container.ThreatEvents?.ToArray();
            if (tes?.Any() ?? false)
            {
                foreach (var te in tes)
                {
                    var m = te.Mitigations?.FirstOrDefault(x => x.MitigationId == mitigation.Id);
                    if (m != null)
                        list.Add(m);
                }
            }
        }
        #endregion
    }
}
