using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Extensions;
using MCRA.General.TableDefinitions;
using MCRA.General.TableDefinitions.RawTableFieldEnums;
using MCRA.Utils.DataFileReading;

namespace MCRA.Data.Management.CompiledDataManagers {
    public partial class CompiledDataManager {

        /// <summary>
        /// Gets all kinetic model conversion factors.
        /// </summary>
        /// <returns></returns>
        public IList<KineticConversionFactor> GetAllKineticConversionFactors() {
            if (_data.AllKineticConversionFactors == null) {
                var kineticConversionFactors = new List<KineticConversionFactor>();
                var rawDataSourceIds = _rawDataProvider.GetRawDatasourceIds(SourceTableGroup.KineticConversionFactors);
                if (rawDataSourceIds?.Any() ?? false) {
                    GetAllCompounds();
                    using (var rdm = _rawDataProvider.CreateRawDataManager()) {
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawKineticConversionFactors>(rawDataSourceId, out int[] fieldMap)) {
                                while (r?.Read() ?? false) {
                                    var idSubstanceFrom = r.GetString(RawKineticConversionFactors.IdSubstanceFrom, fieldMap);
                                    var idSubstanceTo = r.GetString(RawKineticConversionFactors.IdSubstanceTo, fieldMap);
                                    var valid = CheckLinkSelected(ScopingType.Compounds, idSubstanceFrom);
                                    if (valid) {
                                        var exposureRouteFromString = r.GetStringOrNull(RawKineticConversionFactors.ExposureRouteFrom, fieldMap);
                                        var biologicalMatrixFromString = r.GetStringOrNull(RawKineticConversionFactors.BiologicalMatrixFrom, fieldMap);
                                        var doseUnitFromString = r.GetStringOrNull(RawKineticConversionFactors.DoseUnitFrom, fieldMap);
                                        var doseUnitFrom = DoseUnitConverter.FromString(doseUnitFromString);
                                        var expressionTypeFromString = r.GetStringOrNull(RawKineticConversionFactors.ExpressionTypeFrom, fieldMap);
                                        var exposureRouteToString = r.GetStringOrNull(RawKineticConversionFactors.ExposureRouteTo, fieldMap);
                                        var biologicalMatrixToString = r.GetStringOrNull(RawKineticConversionFactors.BiologicalMatrixTo, fieldMap);
                                        var doseUnitToString = r.GetStringOrNull(RawKineticConversionFactors.DoseUnitTo, fieldMap);
                                        var doseUnitTo = DoseUnitConverter.FromString(doseUnitToString);
                                        var expressionTypeToString = r.GetStringOrNull(RawKineticConversionFactors.ExpressionTypeTo, fieldMap);
                                        var distributionTypeString = r.GetStringOrNull(RawKineticConversionFactors.UncertaintyDistributionType, fieldMap);
                                        var kaf = new KineticConversionFactor {
                                            IdKineticConversionFactor = r.GetString(RawKineticConversionFactors.IdKineticConversionFactor, fieldMap),
                                            SubstanceFrom = _data.GetOrAddSubstance(idSubstanceFrom),
                                            ExposureRouteFrom = ExposureRouteConverter.FromString(exposureRouteFromString, ExposureRoute.Undefined),
                                            BiologicalMatrixFrom = BiologicalMatrixConverter.FromString(biologicalMatrixFromString, BiologicalMatrix.Undefined),
                                            DoseUnitFrom = ExposureUnitTriple.FromDoseUnit(doseUnitFrom),
                                            ExpressionTypeFrom = ExpressionTypeConverter.FromString(expressionTypeFromString),
                                            SubstanceTo = _data.GetOrAddSubstance(idSubstanceTo),
                                            ExposureRouteTo = ExposureRouteConverter.FromString(exposureRouteToString, ExposureRoute.Undefined),
                                            BiologicalMatrixTo = BiologicalMatrixConverter.FromString(biologicalMatrixToString, BiologicalMatrix.Undefined),
                                            DoseUnitTo = ExposureUnitTriple.FromDoseUnit(doseUnitTo),
                                            ExpressionTypeTo = ExpressionTypeConverter.FromString(expressionTypeToString),
                                            ConversionFactor = r.GetDoubleOrNull(RawKineticConversionFactors.ConversionFactor, fieldMap) ?? 1d,
                                            Distribution = KineticConversionFactorDistributionTypeConverter.FromString(distributionTypeString, KineticConversionFactorDistributionType.Unspecified),
                                            UncertaintyUpper = r.GetDoubleOrNull(RawKineticConversionFactors.UncertaintyUpper, fieldMap)
                                        };
                                        kineticConversionFactors.Add(kaf);
                                    }
                                }
                            }
                        }

                        // Create lookup based on combined keys
                        var lookup = kineticConversionFactors.ToDictionary(r => r.IdKineticConversionFactor.ToLowerInvariant());
                        // Read kinetic conversion factor subgroups
                        var kcfSubgroups = new List<KineticConversionFactorSG>();
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawKineticConversionFactorSGs>(rawDataSourceId, out int[] fieldMap)) {
                                if (r != null) {
                                    while (r?.Read() ?? false) {
                                        var idKineticConversionFactor = r.GetString(RawKineticConversionFactorSGs.IdKineticConversionFactor, fieldMap).ToLowerInvariant();
                                        var genderToString = r.GetStringOrNull(RawKineticConversionFactorSGs.Gender, fieldMap);
                                        if (lookup.TryGetValue(idKineticConversionFactor, out var kineticConversionFactor)) {
                                            var record = new KineticConversionFactorSG {
                                                IdKineticConversionFactor = idKineticConversionFactor,
                                                AgeLower = r.GetDoubleOrNull(RawKineticConversionFactorSGs.AgeLower, fieldMap),
                                                Gender = GenderTypeConverter.FromString(genderToString),
                                                ConversionFactor = r.GetDouble(RawKineticConversionFactorSGs.ConversionFactor, fieldMap),
                                                UncertaintyUpper = r.GetDoubleOrNull(RawKineticConversionFactorSGs.UncertaintyUpper, fieldMap)
                                            };
                                            kineticConversionFactor.KCFSubgroups.Add(record);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                _data.AllKineticConversionFactors = kineticConversionFactors;
            }
            return _data.AllKineticConversionFactors;
        }

        private static void writeKineticConversionFactorDataToCsv(string tempFolder, IEnumerable<KineticConversionFactor> factors) {
            if (!factors?.Any() ?? true) {
                return;
            }

            var td = McraTableDefinitions.Instance.GetTableDefinition(RawDataSourceTableID.KineticConversionFactors);
            var dt = td.CreateDataTable();

            foreach (var factor in factors) {
                var row = dt.NewRow();
                row.WriteNonEmptyString(RawKineticConversionFactors.IdKineticConversionFactor, factor.IdKineticConversionFactor);
                row.WriteNonEmptyString(RawKineticConversionFactors.IdSubstanceFrom, factor.SubstanceFrom.Code);
                row.WriteNonEmptyString(RawKineticConversionFactors.ExposureRouteFrom, factor.ExposureRouteFrom.ToString());
                row.WriteNonEmptyString(RawKineticConversionFactors.BiologicalMatrixFrom, factor.BiologicalMatrixFrom.ToString());
                row.WriteNonEmptyString(RawKineticConversionFactors.ExpressionTypeFrom, factor.ExpressionTypeFrom.ToString());
                row.WriteNonEmptyString(RawKineticConversionFactors.DoseUnitFrom, factor.DoseUnitFrom.ToString());
                row.WriteNonEmptyString(RawKineticConversionFactors.IdSubstanceTo, factor.SubstanceTo.Code);
                row.WriteNonEmptyString(RawKineticConversionFactors.ExposureRouteTo, factor.ExposureRouteTo.ToString());
                row.WriteNonEmptyString(RawKineticConversionFactors.BiologicalMatrixTo, factor.BiologicalMatrixTo.ToString());
                row.WriteNonEmptyString(RawKineticConversionFactors.DoseUnitTo, factor.DoseUnitTo.ToString());
                row.WriteNonEmptyString(RawKineticConversionFactors.ExpressionTypeTo, factor.ExpressionTypeTo.ToString());
                row.WriteNonNullDouble(RawKineticConversionFactors.ConversionFactor, factor.ConversionFactor);
                row.WriteNonEmptyString(RawKineticConversionFactors.UncertaintyDistributionType, factor.Distribution.ToString());
                row.WriteNonNullDouble(RawKineticConversionFactors.UncertaintyUpper, factor.UncertaintyUpper);
                dt.Rows.Add(row);
            }
            writeToCsv(tempFolder, td, dt);
        }
    }
}
