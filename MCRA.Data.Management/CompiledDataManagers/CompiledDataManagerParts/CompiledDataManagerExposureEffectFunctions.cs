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
        /// Returns all exposure effect functions of the compiled datasource.
        /// </summary>
        public IList<ExposureEffectFunction> GetAllExposureEffectFunctions() {
            if (_data.AllExposureEffectFunctions == null) {
                LoadScope(SourceTableGroup.ExposureEffectFunctions);
                var allExposureEffectFunctions = new List<ExposureEffectFunction>();
                var rawDataSourceIds = _rawDataProvider.GetRawDatasourceIds(SourceTableGroup.ExposureEffectFunctions);
                if (rawDataSourceIds?.Count > 0) {
                    using (var rdm = _rawDataProvider.CreateRawDataManager()) {
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawExposureEffectFunctions>(rawDataSourceId, out int[] fieldMap)) {
                                while (r?.Read() ?? false) {
                                    var idSubstance = r.GetString(RawExposureEffectFunctions.IdSubstance, fieldMap);
                                    var idEffect = r.GetString(RawExposureEffectFunctions.IdEffect, fieldMap);
                                    var idModel = r.GetStringOrNull(RawExposureEffectFunctions.IdModel, fieldMap);
                                    if (IsCodeSelected(ScopingType.ExposureEffectFunctions, idModel)) {
                                        var biologicalMatrix = r.GetEnum(
                                            RawExposureEffectFunctions.BiologicalMatrix,
                                            fieldMap,
                                            BiologicalMatrix.Undefined
                                        );
                                        var doseUnitString = r.GetStringOrNull(RawExposureEffectFunctions.DoseUnit, fieldMap);
                                        var expressionType = r.GetEnum(
                                            RawExposureEffectFunctions.ExpressionType,
                                            fieldMap,
                                            ExpressionType.None
                                        );
                                        var effectMetric = r.GetEnum(
                                            RawExposureEffectFunctions.EffectMetric,
                                            fieldMap,
                                            EffectMetric.Undefined
                                        );
                                        var exposureResponseType = r.GetEnum(
                                            RawExposureEffectFunctions.ExposureResponseType,
                                            fieldMap,
                                            ExposureResponseType.Function
                                        );
                                        Expression exposureResponseSpecification;
                                        var exposureResponseSpecificationString = r.GetStringOrNull(RawExposureEffectFunctions.ExposureResponseSpecification, fieldMap);
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
                                            var eefValue = exposureResponseSpecificationString.Replace(",", ".");
                                            exposureResponseSpecification = new Expression(
                                                eefValue.ToString(CultureInfo.InvariantCulture),
                                                ExpressionOptions.IgnoreCase,
                                                CultureInfo.InvariantCulture
                                            );
                                        }
                                        var exposureRoute = r.GetEnum(
                                            RawExposureEffectFunctions.ExposureRoute,
                                            fieldMap,
                                            ExposureRoute.Undefined
                                        );
                                        var targetLevel = r.GetEnum(
                                            RawExposureEffectFunctions.TargetLevel,
                                            fieldMap,
                                            biologicalMatrix != BiologicalMatrix.Undefined
                                                ? TargetLevelType.Internal
                                                : TargetLevelType.External
                                        );

                                        var record = new ExposureEffectFunction() {
                                            Code = idModel,
                                            Name = r.GetStringOrNull(RawExposureEffectFunctions.Name, fieldMap),
                                            Description = r.GetStringOrNull(RawExposureEffectFunctions.Description, fieldMap),
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
                                            Baseline = r.GetDouble(RawExposureEffectFunctions.Baseline, fieldMap)
                                        };
                                        allExposureEffectFunctions.Add(record);
                                    }
                                }
                            }
                        }
                    }
                }
                _data.AllExposureEffectFunctions = allExposureEffectFunctions;
            }
            return _data.AllExposureEffectFunctions;
        }

        private static void writeExposureEffectFunctionsDataToCsv(string tempFolder, IEnumerable<ExposureEffectFunction> exposureEffectFunctions) {
            if (!exposureEffectFunctions?.Any() ?? true) {
                return;
            }

            var tdExposureEffectFunctions = McraTableDefinitions.Instance.GetTableDefinition(RawDataSourceTableID.ExposureEffectFunctions);
            var dtAExposureEffectFunctions = tdExposureEffectFunctions.CreateDataTable();
            var ccr = new int[Enum.GetNames(typeof(RawExposureEffectFunctions)).Length];
            foreach (var eef in exposureEffectFunctions) {
                var r = dtAExposureEffectFunctions.NewRow();
                r.WriteNonEmptyString(RawExposureEffectFunctions.IdModel, eef.Code, ccr);
                r.WriteNonEmptyString(RawExposureEffectFunctions.Name, eef.Name, ccr);
                r.WriteNonEmptyString(RawExposureEffectFunctions.Description, eef.Description, ccr);
                r.WriteNonEmptyString(RawExposureEffectFunctions.IdSubstance, eef.Substance?.Code, ccr);
                r.WriteNonEmptyString(RawExposureEffectFunctions.IdEffect, eef.Effect?.Code, ccr);
                r.WriteNonEmptyString(RawExposureEffectFunctions.TargetLevel, eef.TargetLevel.ToString(), ccr);
                r.WriteNonEmptyString(RawExposureEffectFunctions.ExposureRoute, eef.ExposureRoute.ToString(), ccr);
                r.WriteNonEmptyString(RawExposureEffectFunctions.BiologicalMatrix, eef.BiologicalMatrix.ToString(), ccr);
                r.WriteNonEmptyString(RawExposureEffectFunctions.DoseUnit, eef.DoseUnit.ToString(), ccr);
                r.WriteNonEmptyString(RawExposureEffectFunctions.ExpressionType, eef.ExpressionType.ToString(), ccr);
                r.WriteNonEmptyString(RawExposureEffectFunctions.EffectMetric, eef.EffectMetric.ToString(), ccr);
                r.WriteNonEmptyString(RawExposureEffectFunctions.ExposureResponseType, eef.ExposureResponseType.ToString(), ccr);
                r.WriteNonEmptyString(RawExposureEffectFunctions.ExposureResponseSpecification, eef.ExposureResponseSpecification.ToString(), ccr);
                r.WriteNonEmptyString(RawExposureEffectFunctions.Baseline, eef.Baseline.ToString(), ccr);
                dtAExposureEffectFunctions.Rows.Add(r);
            }
            writeToCsv(tempFolder, tdExposureEffectFunctions, dtAExposureEffectFunctions);
        }
    }
}
