using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Extensions;
using MCRA.General.TableDefinitions.RawTableFieldEnums;

namespace MCRA.Data.Management.CompiledDataManagers {
    public partial class CompiledDataManager {

        /// <summary>
        /// Gets all individual sets.
        /// </summary>
        /// <returns></returns>
        public IDictionary<string, IndividualSet> GetAllIndividualSets() {
            if (_data.AllIndividualSets == null) {
                LoadScope(SourceTableGroup.Individuals);
                var allIndividualSets = new Dictionary<string, IndividualSet>(StringComparer.OrdinalIgnoreCase);
                var rawDataSourceIds = _rawDataProvider.GetRawDatasourceIds(SourceTableGroup.Individuals);
                if (rawDataSourceIds?.Count > 0) {
                    using (var rdm = _rawDataProvider.CreateRawDataManager()) {
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawIndividualSets>(rawDataSourceId, out int[] fieldMap)) {
                                while (r?.Read() ?? false) {
                                    var setId = r.GetString(RawIndividualSets.IdSet, fieldMap);
                                    var valid = IsCodeSelected(ScopingType.IndividualSets, setId);
                                    if (valid) {
                                        var set = new IndividualSet {
                                            Code = setId,
                                            Name = r.GetStringOrNull(RawIndividualSets.Name, fieldMap),
                                            Description = r.GetStringOrNull(RawIndividualSets.Description, fieldMap),
                                            BodyWeightUnit = r.GetEnum(RawIndividualSets.BodyWeightUnit, fieldMap, BodyWeightUnit.kg),
                                            AgeUnitString = r.GetStringOrNull(RawIndividualSets.AgeUnit, fieldMap),
                                            StartDate = r.GetDateTimeOrNull(RawIndividualSets.StartDate, fieldMap),
                                            EndDate = r.GetDateTimeOrNull(RawIndividualSets.EndDate, fieldMap),
                                            IdPopulation = r.GetStringOrNull(RawIndividualSets.IdPopulation, fieldMap),
                                        };
                                        allIndividualSets[set.Code] = set;
                                    }
                                }
                            }
                        }
                    }
                }
                _data.AllIndividualSets = allIndividualSets;
            }
            return _data.AllIndividualSets;
        }

        /// <summary>
        /// Gets all individual set individuals.
        /// </summary>
        /// <returns></returns>
        public IDictionary<string, Individual> GetAllIndividualSetIndividuals() {
            if (_data.AllIndividualSetIndividuals == null) {
                GetAllIndividualSets();
                _data.AllIndividualSetIndividuals = GetIndividuals(
                    SourceTableGroup.Individuals,
                    ScopingType.IndividualSets,
                    _data.GetOrAddIndividualSet
                    );
                _data.AllIndividualSetIndividualProperties = GetIndividualProperties(
                    SourceTableGroup.Individuals,
                    ScopingType.IndividualSetIndividuals,
                    _data.AllIndividualSetIndividuals
                    );
            }
            return _data.AllIndividualSetIndividuals;
        }

        /// <summary>
        /// Gets all individual properties.
        /// </summary>
        /// <returns></returns>
        public IDictionary<string, IndividualProperty> GetAllIndividualSetIndividualProperties() {
            if (_data.AllIndividualSetIndividualProperties == null) {
                GetAllIndividualSetIndividuals();
            }
            return _data.AllIndividualSetIndividualProperties;
        }

    }
}
