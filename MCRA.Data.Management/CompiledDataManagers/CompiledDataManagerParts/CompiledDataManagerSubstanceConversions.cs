using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Extensions;
using MCRA.General.TableDefinitions.RawTableFieldEnums;
using MCRA.Utils.DataFileReading;

namespace MCRA.Data.Management.CompiledDataManagers {
    public partial class CompiledDataManager {

        /// <summary>
        /// Gets all residue definitions.
        /// </summary>
        public IList<SubstanceConversion> GetAllSubstanceConversions() {
            if (_data.AllSubstanceConversions == null) {
                LoadScope(SourceTableGroup.ResidueDefinitions);
                var allSubstanceConversions = new List<SubstanceConversion>();
                var rawDataSourceIds = _rawDataProvider.GetRawDatasourceIds(SourceTableGroup.ResidueDefinitions);
                if (rawDataSourceIds?.Count > 0) {
                    GetAllCompounds();
                    using (var rdm = _rawDataProvider.CreateRawDataManager()) {
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawResidueDefinitions>(rawDataSourceId, out int[] fieldMap)) {
                                while (r?.Read() ?? false) {
                                    var idMeasuredSubstance = r.GetString(RawResidueDefinitions.IdMeasuredSubstance, fieldMap);
                                    var idActiveSubstance = r.GetString(RawResidueDefinitions.IdActiveSubstance, fieldMap);
                                    var valid = CheckLinkSelected(ScopingType.Compounds, idMeasuredSubstance)
                                              & CheckLinkSelected(ScopingType.Compounds, idActiveSubstance);
                                    if (valid) {
                                        var record = new SubstanceConversion {
                                            MeasuredSubstance = _data.GetOrAddSubstance(idMeasuredSubstance),
                                            ActiveSubstance = _data.GetOrAddSubstance(idActiveSubstance),
                                            ConversionFactor = r.GetDouble(RawResidueDefinitions.ConversionFactor, fieldMap),
                                            IsExclusive = r.GetBoolean(RawResidueDefinitions.IsExclusive, fieldMap),
                                            Proportion = r.GetDoubleOrNull(RawResidueDefinitions.Proportion, fieldMap)
                                        };
                                        allSubstanceConversions.Add(record);
                                    }
                                }
                            }
                        }
                    }
                }

                _data.AllSubstanceConversions = allSubstanceConversions;
            }
            return _data.AllSubstanceConversions;
        }
    }
}
