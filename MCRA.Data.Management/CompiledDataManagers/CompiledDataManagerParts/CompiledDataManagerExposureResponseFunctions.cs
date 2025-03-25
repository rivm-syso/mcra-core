using System.Globalization;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Extensions;
using MCRA.General.TableDefinitions;
using MCRA.General.TableDefinitions.RawTableFieldEnums;
using MCRA.Utils.DataFileReading;
using NCalc;

namespace MCRA.Data.Management.CompiledDataManagers {
    public partial class CompiledDataManager {

        /// <summary>
        /// Returns all exposure response functions of the compiled datasource.
        /// </summary>
        public IList<ExposureResponseFunction> GetAllExposureResponseFunctions() {
            if (_data.AllExposureResponseFunctions == null) {
                LoadScope(SourceTableGroup.ExposureResponseFunctions);
                var allExposureResponseFunctions = new List<ExposureResponseFunction>();
                var rawDataSourceIds = _rawDataProvider.GetRawDatasourceIds(SourceTableGroup.ExposureResponseFunctions);
                if (rawDataSourceIds?.Count > 0) {
                    using (var rdm = _rawDataProvider.CreateRawDataManager()) {
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawExposureResponseFunctions>(rawDataSourceId, out int[] fieldMap)) {
                                while (r?.Read() ?? false) {
                                    var idSubstance = r.GetString(RawExposureResponseFunctions.IdSubstance, fieldMap);
                                    var idEffect = r.GetString(RawExposureResponseFunctions.IdEffect, fieldMap);
                                    var idModel = r.GetStringOrNull(RawExposureResponseFunctions.IdModel, fieldMap);
                                    if (IsCodeSelected(ScopingType.ExposureResponseFunctions, idModel)) {
                                        var biologicalMatrix = r.GetEnum(
                                            RawExposureResponseFunctions.BiologicalMatrix,
                                            fieldMap,
                                            BiologicalMatrix.Undefined
                                        );
                                        var exposureRoute = r.GetEnum(
                                            RawExposureResponseFunctions.ExposureRoute,
                                            fieldMap,
                                            ExposureRoute.Undefined
                                        );
                                        var targetLevel = r.GetEnum(
                                            RawExposureResponseFunctions.TargetLevel,
                                            fieldMap,
                                            biologicalMatrix != BiologicalMatrix.Undefined
                                                ? TargetLevelType.Internal
                                                : exposureRoute != ExposureRoute.Undefined
                                                    ? TargetLevelType.External
                                                    : TargetLevelType.Systemic
                                        );
                                        if (exposureRoute == ExposureRoute.Undefined && targetLevel == TargetLevelType.External) {
                                            exposureRoute = ExposureRoute.Oral;
                                        }
                                        var doseUnitString = r.GetStringOrNull(RawExposureResponseFunctions.DoseUnit, fieldMap);
                                        var expressionType = r.GetEnum(
                                            RawExposureResponseFunctions.ExpressionType,
                                            fieldMap,
                                            ExpressionType.None
                                        );
                                        var effectMetric = r.GetEnum(
                                            RawExposureResponseFunctions.EffectMetric,
                                            fieldMap,
                                            EffectMetric.Undefined
                                        );
                                        var exposureResponseType = r.GetEnum(
                                            RawExposureResponseFunctions.ExposureResponseType,
                                            fieldMap,
                                            ExposureResponseType.Function
                                        );
                                        Expression exposureResponseSpecification;
                                        var exposureResponseSpecificationString = r.GetStringOrNull(RawExposureResponseFunctions.ExposureResponseSpecification, fieldMap);
                                        if (exposureResponseType == ExposureResponseType.Function) {
                                            exposureResponseSpecification = new Expression(
                                                exposureResponseSpecificationString,
                                                ExpressionOptions.IgnoreCase,
                                                CultureInfo.InvariantCulture
                                            );
                                        } else {
                                            // Not so nice hack for excel: here we expect a double value
                                            // Depending on location settings, excel files may use commas as decimal separators.
                                            // In order to get valid decimal values, we replace all commas by decimal points.
                                            // Note that a similar construct is also used for Excel files when reading double values,
                                            // but this construct does not work here, because the column does not have a numeric fieldtype.
                                            var erfValue = exposureResponseSpecificationString.Replace(",", ".");
                                            exposureResponseSpecification = new Expression(
                                                erfValue.ToString(CultureInfo.InvariantCulture),
                                                ExpressionOptions.IgnoreCase,
                                                CultureInfo.InvariantCulture
                                            );
                                        }

                                        var record = new ExposureResponseFunction() {
                                            Code = idModel,
                                            Name = r.GetStringOrNull(RawExposureResponseFunctions.Name, fieldMap),
                                            Description = r.GetStringOrNull(RawExposureResponseFunctions.Description, fieldMap),
                                            Substance = _data.GetOrAddSubstance(idSubstance),
                                            Effect = _data.GetOrAddEffect(idEffect),
                                            TargetLevel = targetLevel,
                                            ExposureRoute = exposureRoute,
                                            BiologicalMatrix = biologicalMatrix,
                                            DoseUnit = DoseUnitConverter.FromString(doseUnitString),
                                            ExpressionType = expressionType,
                                            EffectMetric = effectMetric,
                                            ExposureResponseType = exposureResponseType,
                                            ExposureResponseSpecification = exposureResponseSpecification,
                                            Baseline = r.GetDouble(RawExposureResponseFunctions.Baseline, fieldMap)
                                        };
                                        allExposureResponseFunctions.Add(record);
                                    }
                                }
                            }
                        }
                    }
                }
                _data.AllExposureResponseFunctions = allExposureResponseFunctions;
            }
            return _data.AllExposureResponseFunctions;
        }

        private static void writeExposureResponseFunctionsDataToCsv(string tempFolder, IEnumerable<ExposureResponseFunction> exposureResponseFunctions) {
            if (!exposureResponseFunctions?.Any() ?? true) {
                return;
            }

            var tdExposureResponseFunctions = McraTableDefinitions.Instance.GetTableDefinition(RawDataSourceTableID.ExposureResponseFunctions);
            var dtAExposureResponseFunctions = tdExposureResponseFunctions.CreateDataTable();
            var ccr = new int[Enum.GetNames(typeof(RawExposureResponseFunctions)).Length];
            foreach (var erf in exposureResponseFunctions) {
                var r = dtAExposureResponseFunctions.NewRow();
                r.WriteNonEmptyString(RawExposureResponseFunctions.IdModel, erf.Code, ccr);
                r.WriteNonEmptyString(RawExposureResponseFunctions.Name, erf.Name, ccr);
                r.WriteNonEmptyString(RawExposureResponseFunctions.Description, erf.Description, ccr);
                r.WriteNonEmptyString(RawExposureResponseFunctions.IdSubstance, erf.Substance?.Code, ccr);
                r.WriteNonEmptyString(RawExposureResponseFunctions.IdEffect, erf.Effect?.Code, ccr);
                r.WriteNonEmptyString(RawExposureResponseFunctions.TargetLevel, erf.TargetLevel.ToString(), ccr);
                r.WriteNonEmptyString(RawExposureResponseFunctions.ExposureRoute, erf.ExposureRoute.ToString(), ccr);
                r.WriteNonEmptyString(RawExposureResponseFunctions.BiologicalMatrix, erf.BiologicalMatrix.ToString(), ccr);
                r.WriteNonEmptyString(RawExposureResponseFunctions.DoseUnit, erf.DoseUnit.ToString(), ccr);
                r.WriteNonEmptyString(RawExposureResponseFunctions.ExpressionType, erf.ExpressionType.ToString(), ccr);
                r.WriteNonEmptyString(RawExposureResponseFunctions.EffectMetric, erf.EffectMetric.ToString(), ccr);
                r.WriteNonEmptyString(RawExposureResponseFunctions.ExposureResponseType, erf.ExposureResponseType.ToString(), ccr);
                r.WriteNonEmptyString(RawExposureResponseFunctions.ExposureResponseSpecification, erf.ExposureResponseSpecification.ToString(), ccr);
                r.WriteNonEmptyString(RawExposureResponseFunctions.Baseline, erf.Baseline.ToString(), ccr);
                dtAExposureResponseFunctions.Rows.Add(r);
            }
            writeToCsv(tempFolder, tdExposureResponseFunctions, dtAExposureResponseFunctions);
        }
    }
}
