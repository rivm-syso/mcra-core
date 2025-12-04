using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Extensions;
using MCRA.General.TableDefinitions.RawTableFieldEnums;

namespace MCRA.Data.Management.CompiledDataManagers {
    public partial class CompiledDataManager {

        /// <summary>
        /// GetAllInterSpeciesFactors
        /// </summary>
        /// <returns></returns>
        public ICollection<InterSpeciesFactor> GetAllInterSpeciesFactors() {
            if (_data.AllInterSpeciesFactors == null) {
                LoadScope(SourceTableGroup.InterSpeciesFactors);
                var allInterSpeciesFactors = new List<InterSpeciesFactor>();
                var rawDataSourceIds = _rawDataProvider.GetRawDatasourceIds(SourceTableGroup.InterSpeciesFactors);
                if (rawDataSourceIds?.Count > 0) {
                    GetAllEffects();
                    GetAllCompounds();

                    using (var rdm = _rawDataProvider.CreateRawDataManager()) {

                        // Read inter species models
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawInterSpeciesModelParameters>(rawDataSourceId, out int[] fieldMap)) {
                                if (r != null) {

                                    while (r?.Read() ?? false) {
                                        var idEffect = r.GetString(RawInterSpeciesModelParameters.IdEffect, fieldMap);
                                        var idSubstance = r.GetStringOrNull(RawInterSpeciesModelParameters.IdCompound, fieldMap);
                                        var noSubstance = string.IsNullOrEmpty(idSubstance);

                                        //check both links: don't use &&
                                        var valid = CheckLinkSelected(ScopingType.Effects, idEffect)
                                                  & (noSubstance || CheckLinkSelected(ScopingType.Compounds, idSubstance));
                                        if (valid) {
                                            var compound = noSubstance ? null : _data.GetOrAddSubstance(idSubstance);
                                            var species = r.GetString(RawInterSpeciesModelParameters.Species, fieldMap);
                                            var ismp = new InterSpeciesFactor {
                                                Compound = compound,
                                                Effect = _data.GetOrAddEffect(idEffect),
                                                Species = r.GetString(RawInterSpeciesModelParameters.Species, fieldMap),
                                                StandardAnimalBodyWeight = r.GetDouble(RawInterSpeciesModelParameters.StandardAnimalBodyWeight, fieldMap),
                                                StandardHumanBodyWeight = r.GetDouble(RawInterSpeciesModelParameters.StandardHumanBodyWeight, fieldMap),
                                                InterSpeciesFactorGeometricMean = r.GetDouble(RawInterSpeciesModelParameters.InterSpeciesGeometricMean, fieldMap),
                                                InterSpeciesFactorGeometricStandardDeviation = r.GetDouble(RawInterSpeciesModelParameters.InterSpeciesGeometricStandardDeviation, fieldMap),
                                                AnimalBodyWeightUnit = r.GetEnum(RawInterSpeciesModelParameters.AnimalBodyWeightUnit, fieldMap, BodyWeightUnit.kg),
                                                HumanBodyWeightUnit = r.GetEnum(RawInterSpeciesModelParameters.HumanBodyWeightUnit, fieldMap, BodyWeightUnit.kg)
                                            };
                                            allInterSpeciesFactors.Add(ismp);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                _data.AllInterSpeciesFactors = allInterSpeciesFactors;
            }
            return _data.AllInterSpeciesFactors;
        }
    }
}