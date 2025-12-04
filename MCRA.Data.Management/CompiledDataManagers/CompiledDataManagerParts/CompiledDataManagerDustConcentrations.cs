using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Extensions;
using MCRA.General.TableDefinitions.RawTableFieldEnums;

namespace MCRA.Data.Management.CompiledDataManagers {
    public partial class CompiledDataManager {

        /// <summary>
        /// All maximum residue limits.
        /// </summary>
        public IList<DustConcentrationDistribution> GetAllDustConcentrationDistributions() {
            if (_data.AllDustConcentrationDistributions == null) {
                LoadScope(SourceTableGroup.DustConcentrationDistributions);
                var allDustConcentrationDistributions = new List<DustConcentrationDistribution>();
                var rawDataSourceIds = _rawDataProvider.GetRawDatasourceIds(SourceTableGroup.DustConcentrationDistributions);
                if (rawDataSourceIds?.Count > 0) {
                    GetAllCompounds();
                    using (var rdm = _rawDataProvider.CreateRawDataManager()) {
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawDustConcentrationDistributions>(rawDataSourceId, out int[] fieldMap)) {
                                while (r?.Read() ?? false) {
                                    var idSubstance = r.GetString(RawDustConcentrationDistributions.IdSubstance, fieldMap);
                                    var valid = CheckLinkSelected(ScopingType.Compounds, idSubstance);
                                    if (valid) {
                                        var unitString = r.GetStringOrNull(RawDustConcentrationDistributions.ConcentrationUnit, fieldMap);
                                        var unit = ConcentrationUnitConverter.FromString(unitString, ConcentrationUnit.ugPerg);
                                        var dustConcentrationDistribution = new DustConcentrationDistribution {
                                            idSample = r.GetStringOrNull(RawDustConcentrationDistributions.IdSample, fieldMap),
                                            Substance = _data.GetOrAddSubstance(idSubstance),
                                            Concentration = r.GetDouble(RawDustConcentrationDistributions.Concentration, fieldMap),
                                            Unit = unit
                                        };
                                        allDustConcentrationDistributions.Add(dustConcentrationDistribution);
                                    }
                                }
                            }
                        }
                    }
                }
                _data.AllDustConcentrationDistributions = allDustConcentrationDistributions;
            }
            return _data.AllDustConcentrationDistributions;
        }
    }
}
