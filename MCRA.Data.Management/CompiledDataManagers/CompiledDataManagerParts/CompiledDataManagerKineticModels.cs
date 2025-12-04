using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Extensions;
using MCRA.General.TableDefinitions.RawTableFieldEnums;

namespace MCRA.Data.Management.CompiledDataManagers {
    public partial class CompiledDataManager {

        /// <summary>
        /// Gets all simple absorption factors.
        /// </summary>
        /// <returns></returns>
        public IList<SimpleAbsorptionFactor> GetAllAbsorptionFactors() {
            if (_data.AllAbsorptionFactors == null) {
                var allAbsorptionFactors = new List<SimpleAbsorptionFactor>();
                var rawDataSourceIds = _rawDataProvider.GetRawDatasourceIds(SourceTableGroup.KineticModels);
                if (rawDataSourceIds?.Count > 0) {
                    GetAllCompounds();
                    using (var rdm = _rawDataProvider.CreateRawDataManager()) {
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawKineticAbsorptionFactors>(rawDataSourceId, out int[] fieldMap)) {
                                while (r?.Read() ?? false) {
                                    var idSubstance = r.GetString(RawKineticAbsorptionFactors.IdCompound, fieldMap);
                                    var valid = CheckLinkSelected(ScopingType.Compounds, idSubstance);
                                    if (valid) {
                                        var kaf = new SimpleAbsorptionFactor {
                                            Substance = _data.GetOrAddSubstance(idSubstance),
                                            ExposurePathType = r.GetEnum(RawKineticAbsorptionFactors.Route, fieldMap, ExposurePathType.Undefined),
                                            AbsorptionFactor = r.GetDouble(RawKineticAbsorptionFactors.AbsorptionFactor, fieldMap),
                                        };
                                        allAbsorptionFactors.Add(kaf);
                                    }
                                }
                            }
                        }
                    }
                }
                _data.AllAbsorptionFactors = allAbsorptionFactors;
            }
            return _data.AllAbsorptionFactors;
        }
    }
}
