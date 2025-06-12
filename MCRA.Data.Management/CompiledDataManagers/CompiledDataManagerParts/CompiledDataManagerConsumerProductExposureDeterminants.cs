using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Extensions;
using MCRA.General.TableDefinitions;
using MCRA.General.TableDefinitions.RawTableFieldEnums;
using MCRA.Utils.DataFileReading;

namespace MCRA.Data.Management.CompiledDataManagers {
    public partial class CompiledDataManager {

        public IList<ConsumerProductExposureFraction> GetAllConsumerProductExposureFractions() {
            if (_data.AllConsumerProductExposureFractions == null) {
                LoadScope(SourceTableGroup.ConsumerProductExposureDeterminants);
                var allExposureFractions = new List<ConsumerProductExposureFraction>();
                var rawDataSourceIds = _rawDataProvider.GetRawDatasourceIds(SourceTableGroup.ConsumerProductExposureDeterminants);
                if (rawDataSourceIds?.Count > 0) {
                    GetAllCompounds();
                    GetAllConsumerProducts();
                    using (var rdm = _rawDataProvider.CreateRawDataManager()) {
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawConsumerProductExposureFractions>(rawDataSourceId, out int[] fieldMap)) {
                                while (r?.Read() ?? false) {
                                    var idSubstance = r.GetString(RawConsumerProductExposureFractions.IdSubstance, fieldMap);
                                    var idProduct = r.GetString(RawConsumerProductExposureFractions.IdProduct, fieldMap);
                                    var valid = CheckLinkSelected(ScopingType.Compounds, idSubstance) && CheckLinkSelected(ScopingType.ConsumerProducts, idProduct);
                                    if (valid) {
                                        var exposureFraction = new ConsumerProductExposureFraction {
                                            Product = _data.GetOrAddConsumerProduct(idProduct),
                                            Substance = _data.GetOrAddSubstance(idSubstance),
                                            Route = r.GetEnum(RawConsumerProductExposureFractions.ExposureRoute, fieldMap, ExposureRoute.Undefined),
                                            ExposureFraction = r.GetDouble(RawConsumerProductExposureFractions.ExposureFraction, fieldMap),
                                        };
                                        allExposureFractions.Add(exposureFraction);
                                    }
                                }
                            }
                        }
                    }
                }
                _data.AllConsumerProductExposureFractions = allExposureFractions;
            }
            return _data.AllConsumerProductExposureFractions;
        }

        public IList<ConsumerProductApplicationAmount> GetAllConsumerProductApplicationAmounts() {
            if (_data.AllConsumerProductApplicationAmounts == null) {
                LoadScope(SourceTableGroup.ConsumerProductExposureDeterminants);
                var allConsumerProductApplicationAmounts = new List<ConsumerProductApplicationAmount>();
                var rawDataSourceIds = _rawDataProvider.GetRawDatasourceIds(SourceTableGroup.ConsumerProductExposureDeterminants);

                if (rawDataSourceIds?.Count > 0) {
                    GetAllConsumerProducts();
                    using (var rdm = _rawDataProvider.CreateRawDataManager()) {
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawConsumerProductApplicationAmounts>(rawDataSourceId, out int[] fieldMap)) {
                                while (r?.Read() ?? false) {
                                    var idProduct = r.GetString(RawConsumerProductExposureFractions.IdProduct, fieldMap);
                                    var valid = CheckLinkSelected(ScopingType.ConsumerProducts, idProduct);
                                    if (valid) {
                                        var applicationAmount = new ConsumerProductApplicationAmount {
                                            Product = _data.GetOrAddConsumerProduct(idProduct),
                                            Amount = r.GetDouble(RawConsumerProductApplicationAmounts.Amount, fieldMap),
                                            DistributionType = r.GetEnum(RawConsumerProductApplicationAmounts.DistributionType, fieldMap, ApplicationAmountDistributionType.Constant),
                                            CvVariability = r.GetDoubleOrNull(RawConsumerProductApplicationAmounts.CvVariability, fieldMap),
                                        };
                                        allConsumerProductApplicationAmounts.Add(applicationAmount);
                                    }
                                }
                            }
                        }
                    }
                }
                _data.AllConsumerProductApplicationAmounts = allConsumerProductApplicationAmounts;
            }
            return _data.AllConsumerProductApplicationAmounts;
        }

        private static void writeConsumerProductExposureFractions(
            string tempFolder,
            IEnumerable<ConsumerProductExposureFraction> consumerProductExposureFraction
        ) {
            if (!consumerProductExposureFraction?.Any() ?? true) {
                return;
            }

            var td = McraTableDefinitions.Instance.GetTableDefinition(RawDataSourceTableID.ConsumerProductExposureFractions);
            var dt = td.CreateDataTable();
            var ccr = new int[Enum.GetNames(typeof(RawConsumerProductExposureFractions)).Length];

            foreach (var exposureFraction in consumerProductExposureFraction) {
                var row = dt.NewRow();
                row.WriteNonEmptyString(RawConsumerProductExposureFractions.IdProduct, exposureFraction.Product.Code, ccr);
                row.WriteNonEmptyString(RawConsumerProductExposureFractions.IdSubstance, exposureFraction.Substance.Code, ccr);
                row.WriteNonEmptyString(RawConsumerProductExposureFractions.ExposureRoute, exposureFraction.Route.ToString(), ccr);
                row.WriteNonNullDouble(RawConsumerProductExposureFractions.ExposureFraction, exposureFraction.ExposureFraction, ccr);
                dt.Rows.Add(row);
            }

            writeToCsv(tempFolder, td, dt, ccr);
        }

        private static void writeConsumerProductApplicationAmounts(string tempFolder, IEnumerable<ConsumerProductApplicationAmount> consumerProductApplicationAmount) {
            if (!consumerProductApplicationAmount?.Any() ?? true) {
                return;
            }

            var td = McraTableDefinitions.Instance.GetTableDefinition(RawDataSourceTableID.ConsumerProductApplicationAmounts);
            var dt = td.CreateDataTable();
            var ccr = new int[Enum.GetNames(typeof(RawConsumerProductApplicationAmounts)).Length];

            foreach (var amount in consumerProductApplicationAmount) {
                var row = dt.NewRow();
                row.WriteNonEmptyString(RawConsumerProductApplicationAmounts.IdProduct, amount.Product.Code, ccr);
                row.WriteNonNullDouble(RawConsumerProductApplicationAmounts.Amount, amount.Amount, ccr);
                row.WriteNonEmptyString(RawConsumerProductApplicationAmounts.DistributionType, amount.DistributionType.ToString(), ccr);
                row.WriteNonNullDouble(RawConsumerProductApplicationAmounts.CvVariability, amount.CvVariability, ccr);
                dt.Rows.Add(row);
            }
            writeToCsv(tempFolder, td, dt, ccr);
        }
    }
}
