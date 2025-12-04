using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Extensions;
using MCRA.General.TableDefinitions.RawTableFieldEnums;
using MCRA.Utils.DataFileReading;

namespace MCRA.Data.Management.CompiledDataManagers {
    public partial class CompiledDataManager {

        public IList<ConsumerProductConcentration> GetAllConsumerProductConcentrations() {
            if (_data.AllConsumerProductConcentrations == null) {
                LoadScope(SourceTableGroup.ConsumerProductConcentrations);
                var allConsumerProductConcentrations = new List<ConsumerProductConcentration>();
                var rawDataSourceIds = _rawDataProvider.GetRawDatasourceIds(SourceTableGroup.ConsumerProductConcentrations);
                if (rawDataSourceIds?.Count > 0) {
                    GetAllCompounds();
                    GetAllConsumerProducts();
                    using (var rdm = _rawDataProvider.CreateRawDataManager()) {
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawConsumerProductConcentrations>(rawDataSourceId, out int[] fieldMap)) {
                                while (r?.Read() ?? false) {
                                    var idSubstance = r.GetString(RawConsumerProductConcentrations.IdSubstance, fieldMap);
                                    var idProduct = r.GetString(RawConsumerProductConcentrations.IdProduct, fieldMap);
                                    var valid = CheckLinkSelected(ScopingType.Compounds, idSubstance) && CheckLinkSelected(ScopingType.ConsumerProducts, idProduct);
                                    if (valid) {
                                        var unitString = r.GetStringOrNull(RawConsumerProductConcentrations.Unit, fieldMap);
                                        var unit = ConcentrationUnitConverter.FromString(unitString, ConcentrationUnit.ugPerKg);
                                        var consumerProductConcentration = new ConsumerProductConcentration {
                                            Product = _data.GetOrAddConsumerProduct(idProduct),
                                            Substance = _data.GetOrAddSubstance(idSubstance),
                                            Concentration = r.GetDouble(RawConsumerProductConcentrations.Concentration, fieldMap),
                                            Unit = unit,
                                            SamplingWeight = r.GetDoubleOrNull(RawConsumerProductConcentrations.SamplingWeight, fieldMap),
                                        };
                                        allConsumerProductConcentrations.Add(consumerProductConcentration);
                                    }
                                }
                            }
                        }
                    }
                }
                _data.AllConsumerProductConcentrations = allConsumerProductConcentrations;
            }
            return _data.AllConsumerProductConcentrations;
        }
    }
}
