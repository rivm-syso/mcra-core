using MCRA.Data.Compiled.Objects;
using MCRA.General.TableDefinitions.RawTableFieldEnums;
using MCRA.General;
using MCRA.General.Extensions;
using MCRA.General.TableDefinitions;
using MCRA.Utils.DataFileReading;

namespace MCRA.Data.Management.CompiledDataManagers {
    public partial class CompiledDataManager {

        /// <summary>
        /// GetAllIntraSpeciesFactors
        /// </summary>
        /// <returns></returns>
        public ICollection<IntraSpeciesFactor> GetAllIntraSpeciesFactors() {
            if (_data.AllIntraSpeciesFactors == null) {
                LoadScope(SourceTableGroup.IntraSpeciesFactors);
                var allIntraSpeciesFactors = new List<IntraSpeciesFactor>();
                var rawDataSourceIds = _rawDataProvider.GetRawDatasourceIds(SourceTableGroup.IntraSpeciesFactors);
                if (rawDataSourceIds?.Count > 0) {
                    GetAllEffects();
                    GetAllCompounds();

                    using (var rdm = _rawDataProvider.CreateRawDataManager()) {

                        // Read intra-species factor models
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawIntraSpeciesModelParameters>(rawDataSourceId, out int[] fieldMap)) {
                                if (r != null) {
                                    while (r?.Read() ?? false) {
                                        var idEffect = r.GetString(RawIntraSpeciesModelParameters.IdEffect, fieldMap);
                                        var idSubstance = r.GetStringOrNull(RawIntraSpeciesModelParameters.IdCompound, fieldMap);
                                        var noSubstance = string.IsNullOrEmpty(idSubstance);
                                        var valid = CheckLinkSelected(ScopingType.Effects, idEffect)
                                                  & (noSubstance || CheckLinkSelected(ScopingType.Compounds, idSubstance));
                                        if (valid) {
                                            var effect = _data.GetOrAddEffect(idEffect);
                                            var substance = noSubstance ? null : _data.GetOrAddSubstance(idSubstance);
                                            var ismp = new IntraSpeciesFactor {
                                                Compound = substance,
                                                Effect = effect,
                                                LowerVariationFactor = r.GetDoubleOrNull(RawIntraSpeciesModelParameters.IntraSpeciesLowerVariationFactor, fieldMap),
                                                UpperVariationFactor = r.GetDouble(RawIntraSpeciesModelParameters.IntraSpeciesUpperVariationFactor, fieldMap),
                                                IdPopulation = r.GetStringOrNull(RawIntraSpeciesModelParameters.IdPopulation, fieldMap),
                                            };
                                            allIntraSpeciesFactors.Add(ismp);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                _data.AllIntraSpeciesFactors = allIntraSpeciesFactors;
            }
            return _data.AllIntraSpeciesFactors;
        }

        private static void writeIntraSpeciesFactorDataToCsv(string tempFolder, IEnumerable<IntraSpeciesFactor> factors) {
            if (!factors?.Any() ?? true) {
                return;
            }

            var tdi = McraTableDefinitions.Instance.GetTableDefinition(RawDataSourceTableID.IntraSpeciesModelParameters);
            var dti = tdi.CreateDataTable();

            foreach (var factor in factors) {
                var row = dti.NewRow();
                row.WriteNonEmptyString(RawIntraSpeciesModelParameters.IdEffect, factor.Effect?.Code);
                row.WriteNonEmptyString(RawIntraSpeciesModelParameters.IdCompound, factor.Compound?.Code);
                row.WriteNonEmptyString(RawIntraSpeciesModelParameters.IdPopulation, factor.IdPopulation);
                row.WriteNonNullDouble(RawIntraSpeciesModelParameters.IntraSpeciesLowerVariationFactor, factor.LowerVariationFactor);
                row.WriteNonNaNDouble(RawIntraSpeciesModelParameters.IntraSpeciesUpperVariationFactor, factor.UpperVariationFactor);

                dti.Rows.Add(row);
            }
            writeToCsv(tempFolder, tdi, dti);
        }
    }
}
