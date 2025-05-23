using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Extensions;
using MCRA.General.TableDefinitions;
using MCRA.General.TableDefinitions.RawTableFieldEnums;
using MCRA.Utils.DataFileReading;

namespace MCRA.Data.Management.CompiledDataManagers {
    public partial class CompiledDataManager {

        /// <summary>
        /// Returns all burdens of disease of the compiled datasource.
        /// </summary>
        public IList<BurdenOfDisease> GetAllBurdensOfDisease() {
            if (_data.AllBurdensOfDisease == null) {
                LoadScope(SourceTableGroup.BurdensOfDisease);
                GetAllEffects();
                GetAllPopulations();
                var allBurdensOfDisease = new List<BurdenOfDisease>();
                var rawDataSourceIds = _rawDataProvider.GetRawDatasourceIds(SourceTableGroup.BurdensOfDisease);
                if (rawDataSourceIds?.Count > 0) {
                    using (var rdm = _rawDataProvider.CreateRawDataManager()) {
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawBurdensOfDisease>(rawDataSourceId, out int[] fieldMap)) {
                                while (r?.Read() ?? false) {
                                    var idEffect = r.GetString(RawBurdensOfDisease.IdEffect, fieldMap);
                                    var idPopulation = r.GetString(RawBurdensOfDisease.Population, fieldMap);
                                    var record = new BurdenOfDisease() {
                                        Population = _data.GetOrAddPopulation(idPopulation),
                                        Effect = _data.GetOrAddEffect(idEffect),
                                        BodIndicator = r.GetEnum<BodIndicator>(RawBurdensOfDisease.BodIndicator, fieldMap),
                                        Value = r.GetDouble(RawBurdensOfDisease.Value, fieldMap)
                                    };
                                    allBurdensOfDisease.Add(record);

                                }
                            }
                        }
                    }
                }
                _data.AllBurdensOfDisease = allBurdensOfDisease;
            }
            return _data.AllBurdensOfDisease;
        }

        private static void writeBurdensOfDiseaseDataToCsv(string tempFolder, IEnumerable<BurdenOfDisease> burdensOfDisease) {
            if (!burdensOfDisease?.Any() ?? true) {
                return;
            }

            var tdBurdensOfDisease = McraTableDefinitions.Instance.GetTableDefinition(RawDataSourceTableID.BurdensOfDisease);
            var dtABurdensOfDisease = tdBurdensOfDisease.CreateDataTable();
            var ccr = new int[Enum.GetNames(typeof(RawBurdensOfDisease)).Length];
            foreach (var bod in burdensOfDisease) {
                var r = dtABurdensOfDisease.NewRow();
                r.WriteNonEmptyString(RawBurdensOfDisease.Population, bod.Population.ToString(), ccr);
                r.WriteNonEmptyString(RawBurdensOfDisease.IdEffect, bod.Effect?.Code, ccr);
                r.WriteNonEmptyString(RawBurdensOfDisease.BodIndicator, bod.BodIndicator.ToString(), ccr);
                r.WriteNonEmptyString(RawBurdensOfDisease.Value, bod.Value.ToString(), ccr);
                dtABurdensOfDisease.Rows.Add(r);
            }
            writeToCsv(tempFolder, tdBurdensOfDisease, dtABurdensOfDisease);
        }
    }
}
