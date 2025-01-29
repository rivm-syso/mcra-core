using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Extensions;
using MCRA.General.TableDefinitions;
using MCRA.General.TableDefinitions.RawTableFieldEnums;
using MCRA.Utils.DataFileReading;

namespace MCRA.Data.Management.CompiledDataManagers {
    public partial class CompiledDataManager {

        /// <summary>
        /// All maximum residue limits.
        /// </summary>
        public IList<SoilConcentrationDistribution> GetAllSoilConcentrationDistributions() {
            if (_data.AllSoilConcentrationDistributions == null) {
                LoadScope(SourceTableGroup.SoilConcentrationDistributions);
                var allSoilConcentrationDistributions = new List<SoilConcentrationDistribution>();
                var rawDataSourceIds = _rawDataProvider.GetRawDatasourceIds(SourceTableGroup.SoilConcentrationDistributions);
                if (rawDataSourceIds?.Count > 0) {
                    GetAllCompounds();
                    using (var rdm = _rawDataProvider.CreateRawDataManager()) {
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawSoilConcentrationDistributions>(rawDataSourceId, out int[] fieldMap)) {
                                while (r?.Read() ?? false) {
                                    var idSubstance = r.GetString(RawSoilConcentrationDistributions.IdSubstance, fieldMap);
                                    var valid = CheckLinkSelected(ScopingType.Compounds, idSubstance);
                                    if (valid) {
                                        var unitString = r.GetStringOrNull(RawSoilConcentrationDistributions.ConcentrationUnit, fieldMap);
                                        var unit = ConcentrationUnitConverter.FromString(unitString, ConcentrationUnit.ugPerg);
                                        var soilConcentrationDistribution = new SoilConcentrationDistribution {
                                            idSample = r.GetStringOrNull(RawSoilConcentrationDistributions.IdSample, fieldMap),
                                            Substance = _data.GetOrAddSubstance(idSubstance),
                                            Concentration = r.GetDouble(RawSoilConcentrationDistributions.Concentration, fieldMap),
                                            ConcentrationUnit = unit
                                        };
                                        allSoilConcentrationDistributions.Add(soilConcentrationDistribution);
                                    }
                                }
                            }
                        }
                    }
                }
                _data.AllSoilConcentrationDistributions = allSoilConcentrationDistributions;
            }
            return _data.AllSoilConcentrationDistributions;
        }

        private static void writeSoilConcentrationDistributionsToCsv(string tempFolder, IEnumerable<SoilConcentrationDistribution> soilConcentrationDistributions) {
            if (!soilConcentrationDistributions?.Any() ?? true) {
                return;
            }

            var td = McraTableDefinitions.Instance.GetTableDefinition(RawDataSourceTableID.SoilConcentrationDistributions);
            var dt = td.CreateDataTable();
            var ccr = new int[Enum.GetNames(typeof(RawSoilConcentrationDistributions)).Length];

            foreach (var soilConcentrationDistribution in soilConcentrationDistributions) {
                var row = dt.NewRow();
                row.WriteNonEmptyString(RawSoilConcentrationDistributions.IdSample, soilConcentrationDistribution.idSample, ccr);
                row.WriteNonEmptyString(RawSoilConcentrationDistributions.IdSubstance, soilConcentrationDistribution.Substance?.Code, ccr);
                row.WriteNonNaNDouble(RawSoilConcentrationDistributions.Concentration, soilConcentrationDistribution.Concentration, ccr);
                row.WriteNonEmptyString(RawSoilConcentrationDistributions.ConcentrationUnit, soilConcentrationDistribution.ConcentrationUnit.ToString(), ccr);
                dt.Rows.Add(row);
            }

            writeToCsv(tempFolder, td, dt, ccr);
        }
    }
}
