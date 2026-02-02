using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Extensions;
using MCRA.General.TableDefinitions.RawTableFieldEnums;

namespace MCRA.Data.Management.CompiledDataManagers {
    public partial class CompiledDataManager {
        /// <summary>
        /// Read all dust concentrations within scope.
        /// </summary>
        public IList<SubstanceConcentration> GetAllDustConcentrations() {
            if (_data.AllDustConcentrations == null) {
                LoadScope(SourceTableGroup.DustConcentrations);
                var allDustConcentrations = new List<SubstanceConcentration>();
                var rawDataSourceIds = _rawDataProvider.GetRawDatasourceIds(SourceTableGroup.DustConcentrations);
                if (rawDataSourceIds?.Count > 0) {
                    GetAllCompounds();
                    using (var rdm = _rawDataProvider.CreateRawDataManager()) {
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawDustConcentrations>(rawDataSourceId, out int[] fieldMap)) {
                                while (r?.Read() ?? false) {
                                    var idSubstance = r.GetString(RawDustConcentrations.IdSubstance, fieldMap);
                                    var valid = CheckLinkSelected(ScopingType.Compounds, idSubstance);
                                    if (valid) {
                                        var unitString = r.GetStringOrNull(RawDustConcentrations.ConcentrationUnit, fieldMap);
                                        var unit = ConcentrationUnitConverter.FromString(unitString, ConcentrationUnit.ugPerg);
                                        var dustConcentrations = new SubstanceConcentration {
                                            idSample = r.GetStringOrNull(RawDustConcentrations.IdSample, fieldMap),
                                            Substance = _data.GetOrAddSubstance(idSubstance),
                                            Concentration = r.GetDouble(RawDustConcentrations.Concentration, fieldMap),
                                            Unit = unit
                                        };
                                        allDustConcentrations.Add(dustConcentrations);
                                    }
                                }
                            }
                        }
                    }
                }
                _data.AllDustConcentrations = allDustConcentrations;
            }
            return _data.AllDustConcentrations;
        }
    }
}
