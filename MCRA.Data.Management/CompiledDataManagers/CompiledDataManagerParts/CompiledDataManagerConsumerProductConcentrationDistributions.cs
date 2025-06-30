using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Extensions;
using MCRA.General.TableDefinitions;
using MCRA.General.TableDefinitions.RawTableFieldEnums;
using MCRA.Utils.DataFileReading;

namespace MCRA.Data.Management.CompiledDataManagers {
    public partial class CompiledDataManager {

        public IList<ConsumerProductConcentrationDistribution> GetAllConsumerProductConcentrationDistributions() {
            if (_data.AllConsumerProductConcentrations == null) {
                LoadScope(SourceTableGroup.ConsumerProductConcentrationDistributions);
                var allConsumerProductConcentrationDistributions = new List<ConsumerProductConcentrationDistribution>();
                var rawDataSourceIds = _rawDataProvider.GetRawDatasourceIds(SourceTableGroup.ConsumerProductConcentrationDistributions);
                if (rawDataSourceIds?.Count > 0) {
                    GetAllCompounds();
                    GetAllConsumerProducts();
                    using (var rdm = _rawDataProvider.CreateRawDataManager()) {
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawConsumerProductConcentrationDistributions>(rawDataSourceId, out int[] fieldMap)) {
                                while (r?.Read() ?? false) {
                                    var idSubstance = r.GetString(RawConsumerProductConcentrationDistributions.IdSubstance, fieldMap);
                                    var idProduct = r.GetString(RawConsumerProductConcentrationDistributions.IdProduct, fieldMap);
                                    var valid = CheckLinkSelected(ScopingType.Compounds, idSubstance) && CheckLinkSelected(ScopingType.ConsumerProducts, idProduct);
                                    if (valid) {
                                        var unitString = r.GetStringOrNull(RawConsumerProductConcentrationDistributions.Unit, fieldMap);
                                        var unit = ConcentrationUnitConverter.FromString(unitString, ConcentrationUnit.ugPerKg);
                                        var consumerProductConcentration = new ConsumerProductConcentrationDistribution {
                                            Product = _data.GetOrAddConsumerProduct(idProduct),
                                            Substance = _data.GetOrAddSubstance(idSubstance),
                                            Mean = r.GetDouble(RawConsumerProductConcentrationDistributions.Mean, fieldMap),
                                            Unit = unit,
                                            DistributionType = r.GetEnum(RawConsumerProductConcentrationDistributions.DistributionType, fieldMap, ConsumerProductConcentrationDistributionType.Constant),
                                            CvVariability = r.GetDoubleOrNull(RawConsumerProductConcentrationDistributions.CvVariability, fieldMap),
                                            OccurrencePercentage = r.GetDoubleOrNull(RawConsumerProductConcentrationDistributions.OccurrencePercentage, fieldMap),
                                        };
                                        allConsumerProductConcentrationDistributions.Add(consumerProductConcentration);
                                    }
                                }
                            }
                        }
                    }
                }
                _data.AllConsumerProductConcentrationDistributions = allConsumerProductConcentrationDistributions;
            }
            return _data.AllConsumerProductConcentrationDistributions;
        }

        private static void writeConsumerProductConcentrationDistributions(string tempFolder, IEnumerable<ConsumerProductConcentrationDistribution> consumerProductConcentrationDistributions) {
            if (!consumerProductConcentrationDistributions?.Any() ?? true) {
                return;
            }

            var td = McraTableDefinitions.Instance.GetTableDefinition(RawDataSourceTableID.ConsumerProductConcentrationDistributions);
            var dt = td.CreateDataTable();
            var ccr = new int[Enum.GetNames(typeof(RawConsumerProductConcentrationDistributions)).Length];

            foreach (var concentration in consumerProductConcentrationDistributions) {
                var row = dt.NewRow();
                row.WriteNonEmptyString(RawConsumerProductConcentrationDistributions.IdProduct, concentration.Product.Code, ccr);
                row.WriteNonEmptyString(RawConsumerProductConcentrationDistributions.IdSubstance, concentration.Substance.Code, ccr);
                row.WriteNonNaNDouble(RawConsumerProductConcentrationDistributions.Mean, concentration.Mean, ccr);
                row.WriteNonEmptyString(RawConsumerProductConcentrationDistributions.Unit, concentration.Unit.ToString(), ccr);
                row.WriteNonNullDouble(RawConsumerProductConcentrationDistributions.CvVariability, concentration.CvVariability, ccr);
                row.WriteNonEmptyString(RawConsumerProductConcentrationDistributions.DistributionType, concentration.DistributionType.ToString(), ccr);
                row.WriteNonNullDouble(RawConsumerProductConcentrationDistributions.OccurrencePercentage, concentration.OccurrencePercentage, ccr);
                dt.Rows.Add(row);
            }
            writeToCsv(tempFolder, td, dt, ccr);
        }
    }
}
