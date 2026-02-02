using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Extensions;
using MCRA.General.TableDefinitions.RawTableFieldEnums;

namespace MCRA.Data.Management.CompiledDataManagers {
    public partial class CompiledDataManager {

        /// <summary>
        /// All maximum residue limits.
        /// </summary>
        public IList<SubstanceConcentration> GetAllSoilConcentrations() {
            if (_data.AllSoilConcentrations == null) {
                LoadScope(SourceTableGroup.SoilConcentrations);
                var allSoilConcentrations = new List<SubstanceConcentration>();
                var rawDataSourceIds = _rawDataProvider.GetRawDatasourceIds(SourceTableGroup.SoilConcentrations);
                if (rawDataSourceIds?.Count > 0) {
                    GetAllCompounds();
                    using (var rdm = _rawDataProvider.CreateRawDataManager()) {
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawSoilConcentrations>(rawDataSourceId, out int[] fieldMap)) {
                                while (r?.Read() ?? false) {
                                    var idSubstance = r.GetString(RawSoilConcentrations.IdSubstance, fieldMap);
                                    var valid = CheckLinkSelected(ScopingType.Compounds, idSubstance);
                                    if (valid) {
                                        var unitString = r.GetStringOrNull(RawSoilConcentrations.ConcentrationUnit, fieldMap);
                                        var unit = ConcentrationUnitConverter.FromString(unitString, ConcentrationUnit.ugPerg);
                                        var soilConcentration = new SubstanceConcentration {
                                            idSample = r.GetStringOrNull(RawSoilConcentrations.IdSample, fieldMap),
                                            Substance = _data.GetOrAddSubstance(idSubstance),
                                            Concentration = r.GetDouble(RawSoilConcentrations.Concentration, fieldMap),
                                            Unit = unit
                                        };
                                        allSoilConcentrations.Add(soilConcentration);
                                    }
                                }
                            }
                        }
                    }
                }
                _data.AllSoilConcentrations = allSoilConcentrations;
            }
            return _data.AllSoilConcentrations;
        }
    }
}
