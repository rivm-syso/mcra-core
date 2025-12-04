using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Extensions;
using MCRA.General.TableDefinitions.RawTableFieldEnums;

namespace MCRA.Data.Management.CompiledDataManagers {
    public partial class CompiledDataManager {

        /// <summary>
        /// All consumer products of the project's compiled data source.
        /// </summary>
        public IDictionary<string, ConsumerProduct> GetAllConsumerProducts() {
            if (_data.AllConsumerProducts == null) {
                LoadScope(SourceTableGroup.ConsumerProducts);
                var allConsumerProducts = new Dictionary<string, ConsumerProduct>(StringComparer.OrdinalIgnoreCase);
                var rawDataSourceIds = _rawDataProvider.GetRawDatasourceIds(SourceTableGroup.ConsumerProducts);
                if (rawDataSourceIds?.Count > 0) {
                    using (var rdm = _rawDataProvider.CreateRawDataManager()) {
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawConsumerProducts>(rawDataSourceId, out int[] fieldMap)) {
                                while (r?.Read() ?? false) {
                                    var consumerProductId = r.GetString(RawConsumerProducts.Id, fieldMap);
                                    if (IsCodeSelected(ScopingType.ConsumerProducts, consumerProductId)) {
                                        var idParent = r.GetStringOrNull(RawConsumerProducts.IdGroup, fieldMap);
                                        var consumerProduct = new ConsumerProduct {
                                            Code = consumerProductId,
                                            Name = r.GetStringOrNull(RawConsumerProducts.Name, fieldMap),
                                            Description = r.GetStringOrNull(RawConsumerProducts.Description, fieldMap),
                                        };
                                        consumerProduct = getOrAddConsumerProduct(allConsumerProducts, consumerProductId);
                                        var parent = getOrAddConsumerProduct(allConsumerProducts, idParent);
                                        consumerProduct.Parent = parent;
                                        parent?.Children.Add(consumerProduct);

                                    }
                                }
                            }
                        }

                        // Add items by code from the scope where no matched items were found in the source
                        foreach (var code in GetCodesInScope(ScopingType.ConsumerProducts).Except(allConsumerProducts.Keys, StringComparer.OrdinalIgnoreCase)) {
                            allConsumerProducts[code] = new ConsumerProduct { Code = code };
                        }
                    }
                }
                _data.AllConsumerProducts = allConsumerProducts;
            }
            return _data.AllConsumerProducts;
        }

        private ConsumerProduct getOrAddConsumerProduct(
            IDictionary<string,
            ConsumerProduct> consumerProducts,
            string id,
            string name = null
        ) {
            if (string.IsNullOrWhiteSpace(id)) {
                return null;
            }
            if (!consumerProducts.TryGetValue(id, out ConsumerProduct item)) {
                item = new ConsumerProduct { Code = id, Name = name ?? id, Children = [] };
                consumerProducts.Add(id, item);
            }
            return item;
        }
    }
}
