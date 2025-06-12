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
                                        var populationCharacteristicType = r.GetEnum(
                                            RawExposureResponseFunctions.PopulationCharacteristic,
                                            fieldMap,
                                            PopulationCharacteristicType.Undefined);
                                        var exposureResponseSpecificationString = r.GetStringOrNull(RawExposureResponseFunctions.ExposureResponseSpecification, fieldMap);
                                        var exposureResponseSpecification = parseErfString(
                                            exposureResponseSpecificationString,
                                            exposureResponseType
                                        );
                                        var exposureResponseSpecificationLowerString = r.GetStringOrNull(RawExposureResponseFunctions.ExposureResponseSpecificationLower, fieldMap);
                                        var exposureResponseSpecificationLower = parseErfString(
                                            exposureResponseSpecificationLowerString,
                                            exposureResponseType
                                        );
                                        var exposureResponseSpecificationUpperString = r.GetStringOrNull(RawExposureResponseFunctions.ExposureResponseSpecificationUpper, fieldMap);
                                        var exposureResponseSpecificationUpper = parseErfString(
                                            exposureResponseSpecificationUpperString,
                                            exposureResponseType
                                        );

                                        var exposureTarget = targetLevel == TargetLevelType.External
                                            ? new ExposureTarget(exposureRoute)
                                            : new ExposureTarget(biologicalMatrix, expressionType);
                                        var doseUnit = DoseUnitConverter.FromString(doseUnitString);

                                        var record = new ExposureResponseFunction() {
                                            Code = idModel,
                                            Name = r.GetStringOrNull(RawExposureResponseFunctions.Name, fieldMap),
                                            Description = r.GetStringOrNull(RawExposureResponseFunctions.Description, fieldMap),
                                            Substance = _data.GetOrAddSubstance(idSubstance),
                                            Effect = _data.GetOrAddEffect(idEffect),
                                            ExposureTarget = exposureTarget,
                                            ExposureUnit = ExposureUnitTriple.FromDoseUnit(doseUnit),
                                            EffectMetric = effectMetric,
                                            ExposureResponseType = exposureResponseType,
                                            ExposureResponseSpecification = exposureResponseSpecification,
                                            ExposureResponseSpecificationLower = exposureResponseSpecificationLower,
                                            ExposureResponseSpecificationUpper = exposureResponseSpecificationUpper,
                                            CounterfactualValue = r.GetDouble(RawExposureResponseFunctions.CounterfactualValue, fieldMap),
                                            PopulationCharacteristic = populationCharacteristicType,
                                            EffectThresholdLower = r.GetDoubleOrNull(RawExposureResponseFunctions.EffectThresholdLower, fieldMap),
                                            EffectThresholdUpper = r.GetDoubleOrNull(RawExposureResponseFunctions.EffectThresholdUpper, fieldMap)
                                        };
                                        allExposureResponseFunctions.Add(record);
                                    }
                                }
                            }
                        }

                        // Create lookup based on combined keys
                        var lookup = allExposureResponseFunctions
                            .ToDictionary(r => r.Code.ToLowerInvariant());

                        // Read exposure response functions subgroups
                        var erfSubgroups = new List<ErfSubgroup>();
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawErfSubgroups>(rawDataSourceId, out int[] fieldMap)) {
                                if (r != null) {
                                    while (r?.Read() ?? false) {
                                        var idModel = r.GetString(RawErfSubgroups.IdModel, fieldMap);
                                        var valid = CheckLinkSelected(ScopingType.ExposureResponseFunctions, idModel);
                                        var idLookup = idModel.ToLowerInvariant();
                                        if (valid && lookup.TryGetValue(idLookup, out var exposureResponseFunction)) {
                                            var exposureResponseSpecificationString = r.GetStringOrNull(RawErfSubgroups.ExposureResponseSpecification, fieldMap);
                                            var exposureResponseSpecification = parseErfString(
                                                exposureResponseSpecificationString,
                                                exposureResponseFunction.ExposureResponseType
                                            );
                                            var exposureResponseSpecificationLowerString = r.GetStringOrNull(RawErfSubgroups.ExposureResponseSpecificationLower, fieldMap);
                                            var exposureResponseSpecificationLower = parseErfString(
                                                exposureResponseSpecificationLowerString,
                                                exposureResponseFunction.ExposureResponseType
                                            );
                                            var exposureResponseSpecificationUpperString = r.GetStringOrNull(RawErfSubgroups.ExposureResponseSpecificationUpper, fieldMap);
                                            var exposureResponseSpecificationUpper = parseErfString(
                                                exposureResponseSpecificationUpperString,
                                                exposureResponseFunction.ExposureResponseType
                                            );
                                            var record = new ErfSubgroup {
                                                idModel = idLookup,
                                                idSubgroup = r.GetString(RawErfSubgroups.IdSubgroup, fieldMap),
                                                ExposureUpper = r.GetDoubleOrNull(RawErfSubgroups.ExposureUpper, fieldMap),
                                                ExposureResponseSpecification = exposureResponseSpecification,
                                                ExposureResponseSpecificationLower = exposureResponseSpecificationLower,
                                                ExposureResponseSpecificationUpper = exposureResponseSpecificationUpper
                                            };

                                            exposureResponseFunction.ErfSubgroups.Add(record);
                                        }
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
                r.WriteNonEmptyString(RawExposureResponseFunctions.DoseUnit, erf.ExposureUnit.GetShortDisplayName(), ccr);
                r.WriteNonEmptyString(RawExposureResponseFunctions.ExpressionType, erf.ExpressionType.ToString(), ccr);
                r.WriteNonEmptyString(RawExposureResponseFunctions.EffectMetric, erf.EffectMetric.ToString(), ccr);
                r.WriteNonEmptyString(RawExposureResponseFunctions.ExposureResponseType, erf.ExposureResponseType.ToString(), ccr);
                r.WriteNonEmptyString(RawExposureResponseFunctions.ExposureResponseSpecification, erf.ExposureResponseSpecification.ToString(), ccr);
                r.WriteNonEmptyString(RawExposureResponseFunctions.ExposureResponseSpecificationLower, erf.ExposureResponseSpecificationLower.ToString(), ccr);
                r.WriteNonEmptyString(RawExposureResponseFunctions.ExposureResponseSpecificationUpper, erf.ExposureResponseSpecificationUpper.ToString(), ccr);
                r.WriteNonEmptyString(RawExposureResponseFunctions.CounterfactualValue, erf.CounterfactualValue.ToString(), ccr);
                r.WriteNonEmptyString(RawExposureResponseFunctions.PopulationCharacteristic, erf.PopulationCharacteristic.ToString(), ccr);
                r.WriteNonEmptyString(RawExposureResponseFunctions.EffectThresholdLower, erf.EffectThresholdLower.ToString(), ccr);
                r.WriteNonEmptyString(RawExposureResponseFunctions.EffectThresholdUpper, erf.EffectThresholdUpper.ToString(), ccr);
                dtAExposureResponseFunctions.Rows.Add(r);
            }
            writeToCsv(tempFolder, tdExposureResponseFunctions, dtAExposureResponseFunctions);
        }

        private static Expression parseErfString(
            string exposureResponseSpecificationString,
            ExposureResponseType exposureResponseType
        ) {
            Expression exposureResponseSpecification;
            if (exposureResponseSpecificationString == null) {
                exposureResponseSpecification = new Expression("");
            } else if (exposureResponseType == ExposureResponseType.Function) {
                exposureResponseSpecification = new Expression(
                    exposureResponseSpecificationString,
                    ExpressionOptions.IgnoreCaseAtBuiltInFunctions,
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
                    ExpressionOptions.IgnoreCaseAtBuiltInFunctions,
                    CultureInfo.InvariantCulture
                );
            }

            return exposureResponseSpecification;
        }
    }
}
