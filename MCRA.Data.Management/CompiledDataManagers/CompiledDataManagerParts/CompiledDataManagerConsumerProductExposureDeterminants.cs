using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Extensions;
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
                                    var idProduct = r.GetString(RawConsumerProductApplicationAmounts.IdProduct, fieldMap);
                                    var valid = CheckLinkSelected(ScopingType.ConsumerProducts, idProduct);
                                    if (valid) {
                                        var unitString = r.GetStringOrNull(RawConsumerProductApplicationAmounts.Unit, fieldMap);
                                        var unit = ApplicationAmountUnitConverter.FromString(unitString, ApplicationAmountUnit.g);
                                        var applicationAmount = new ConsumerProductApplicationAmount {
                                            Product = _data.GetOrAddConsumerProduct(idProduct),
                                            Amount = r.GetDouble(RawConsumerProductApplicationAmounts.Amount, fieldMap),
                                            DistributionType = r.GetEnum(RawConsumerProductApplicationAmounts.DistributionType, fieldMap, ApplicationAmountDistributionType.Constant),
                                            CvVariability = r.GetDoubleOrNull(RawConsumerProductApplicationAmounts.CvVariability, fieldMap),
                                            AgeLower = r.GetDoubleOrNull(RawConsumerProductApplicationAmounts.AgeLower, fieldMap),
                                            Sex = r.GetEnum(RawConsumerProductApplicationAmounts.Sex, fieldMap, GenderType.Undefined),
                                            Unit = unit
                                        };
                                        allConsumerProductApplicationAmounts.Add(applicationAmount);
                                    }
                                }
                            }
                        }
                    }
                }
                _data.AllConsumerProductApplicationAmounts = [.. allConsumerProductApplicationAmounts
                    .OrderBy(c => c.Product.Code)
                    .ThenBy(c => c.AgeLower)];
            }
            return _data.AllConsumerProductApplicationAmounts;
        }
    }
}
