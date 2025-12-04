using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Extensions;
using MCRA.General.TableDefinitions.RawTableFieldEnums;

namespace MCRA.Data.Management.CompiledDataManagers {
    public partial class CompiledDataManager {

        /// <summary>
        /// GetAdverseOutcomePathwayNetworks
        /// </summary>
        /// <returns></returns>
        public IDictionary<string, AdverseOutcomePathwayNetwork> GetAdverseOutcomePathwayNetworks() {
            if (_data.AllAdverseOutcomePathwayNetworks == null) {
                LoadScope(SourceTableGroup.AdverseOutcomePathwayNetworks);
                var allAdverseOutcomePathwayNetworks = new Dictionary<string, AdverseOutcomePathwayNetwork>(StringComparer.OrdinalIgnoreCase);
                var rawDataSourceIds = _rawDataProvider.GetRawDatasourceIds(SourceTableGroup.AdverseOutcomePathwayNetworks);
                if (rawDataSourceIds?.Count > 0) {
                    GetAllEffects();

                    using (var rdm = _rawDataProvider.CreateRawDataManager()) {

                        // Read adverse outcome pathway networks
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawAdverseOutcomePathwayNetworks>(rawDataSourceId, out int[] fieldMap)) {
                                while (r?.Read() ?? false) {
                                    var idAopn = r.GetString(RawAdverseOutcomePathwayNetworks.IdAdverseOutcomePathwayNetwork, fieldMap);
                                    var codeAdverseOutcome = r.GetString(RawAdverseOutcomePathwayNetworks.IdAdverseOutcome, fieldMap);

                                    var valid = IsCodeSelected(ScopingType.AdverseOutcomePathwayNetworks, idAopn)
                                              & CheckLinkSelected(ScopingType.Effects, codeAdverseOutcome);
                                    if (valid) {
                                        var aopn = new AdverseOutcomePathwayNetwork() {
                                            Code = idAopn,
                                            Name = r.GetStringOrNull(RawAdverseOutcomePathwayNetworks.Name, fieldMap),
                                            Description = r.GetStringOrNull(RawAdverseOutcomePathwayNetworks.Description, fieldMap),
                                            Reference = r.GetStringOrNull(RawAdverseOutcomePathwayNetworks.Reference, fieldMap),
                                            AdverseOutcome = _data.GetOrAddEffect(codeAdverseOutcome),
                                            EffectRelations = [],
                                        };
                                        allAdverseOutcomePathwayNetworks.Add(idAopn, aopn);
                                    }
                                }
                            }
                        }

                        // Read effect relations
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawEffectRelations>(rawDataSourceId, out int[] fieldMap)) {
                                while (r?.Read() ?? false) {
                                    // Get linked AOP network
                                    var codeAopNetwork = r.GetString(RawEffectRelations.IdAdverseOutcomePathwayNetwork, fieldMap);
                                    var codeUpstreamKeyEvent = r.GetString(RawEffectRelations.IdUpstreamKeyEvent, fieldMap);
                                    var codeDownstreamKeyEvent = r.GetString(RawEffectRelations.IdDownstreamKeyEvent, fieldMap);

                                    //check all linked entities
                                    var valid = CheckLinkSelected(ScopingType.AdverseOutcomePathwayNetworks, codeAopNetwork)
                                              & CheckLinkSelected(ScopingType.Effects, codeUpstreamKeyEvent)
                                              & CheckLinkSelected(ScopingType.Effects, codeDownstreamKeyEvent);
                                    //skip filtered or non linked effects here
                                    if (valid) {
                                        var adverseOutcomePathwayNetwork = allAdverseOutcomePathwayNetworks[codeAopNetwork];
                                        var record = new EffectRelationship() {
                                            AdverseOutcomePathwayNetwork = adverseOutcomePathwayNetwork,
                                            UpstreamKeyEvent = _data.GetOrAddEffect(codeUpstreamKeyEvent),
                                            DownstreamKeyEvent = _data.GetOrAddEffect(codeDownstreamKeyEvent)
                                        };
                                        adverseOutcomePathwayNetwork.EffectRelations.Add(record);
                                    }
                                }
                            }
                        }
                    }
                }

                _data.AllAdverseOutcomePathwayNetworks = allAdverseOutcomePathwayNetworks;
            }
            return _data.AllAdverseOutcomePathwayNetworks;
        }
    }
}
