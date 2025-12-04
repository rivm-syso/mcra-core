using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Extensions;
using MCRA.General.TableDefinitions.RawTableFieldEnums;
using MCRA.Utils.DataFileReading;

namespace MCRA.Data.Management.CompiledDataManagers {
    public partial class CompiledDataManager {

        /// <summary>
        /// Read all conversions from the compiled data source.
        /// </summary>
        /// <returns></returns>
        public ICollection<ExposureBiomarkerConversion> GetAllExposureBiomarkerConversions() {
            if (_data.AllExposureBiomarkerConversions == null) {
                LoadScope(SourceTableGroup.ExposureBiomarkerConversions);
                var allExposureBiomarkerConversions = new List<ExposureBiomarkerConversion>();
                var rawDataSourceIds = _rawDataProvider.GetRawDatasourceIds(SourceTableGroup.ExposureBiomarkerConversions);
                if (rawDataSourceIds?.Count > 0) {
                    GetAllCompounds();
                    using (var rdm = _rawDataProvider.CreateRawDataManager()) {
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawExposureBiomarkerConversions>(rawDataSourceId, out int[] fieldMap)) {
                                while (r?.Read() ?? false) {
                                    var idSubstanceFrom = r.GetString(RawExposureBiomarkerConversions.IdSubstanceFrom, fieldMap);
                                    var idSubstanceTo = r.GetString(RawExposureBiomarkerConversions.IdSubstanceTo, fieldMap);
                                    var valid = CheckLinkSelected(ScopingType.Compounds, idSubstanceFrom)
                                              & CheckLinkSelected(ScopingType.Compounds, idSubstanceTo);
                                    if (valid) {
                                        var biologicalMatrixString = r.GetStringOrNull(RawExposureBiomarkerConversions.BiologicalMatrix, fieldMap);
                                        var unitFromString = r.GetStringOrNull(RawExposureBiomarkerConversions.UnitFrom, fieldMap);
                                        var unitFrom = DoseUnitConverter.FromString(unitFromString);
                                        var expressionTypeFromString = r.GetStringOrNull(RawExposureBiomarkerConversions.ExpressionTypeFrom, fieldMap);
                                        var unitToString = r.GetStringOrNull(RawExposureBiomarkerConversions.UnitTo, fieldMap);
                                        var unitTo = DoseUnitConverter.FromString(unitToString);
                                        var expressionTypeToString = r.GetStringOrNull(RawExposureBiomarkerConversions.ExpressionTypeTo, fieldMap);
                                        var distributionTypeString = r.GetStringOrNull(RawExposureBiomarkerConversions.VariabilityDistributionType, fieldMap);
                                        var record = new ExposureBiomarkerConversion() {
                                            IdExposureBiomarkerConversion = r.GetString(RawExposureBiomarkerConversions.IdExposureBiomarkerConversion, fieldMap),
                                            SubstanceFrom = _data.GetOrAddSubstance(idSubstanceFrom),
                                            SubstanceTo = _data.GetOrAddSubstance(idSubstanceTo),
                                            BiologicalMatrix = BiologicalMatrixConverter.FromString(biologicalMatrixString),
                                            UnitFrom = ExposureUnitTriple.FromDoseUnit(unitFrom),
                                            ExpressionTypeFrom = ExpressionTypeConverter.FromString(expressionTypeFromString),
                                            UnitTo = ExposureUnitTriple.FromDoseUnit(unitTo),
                                            ExpressionTypeTo = ExpressionTypeConverter.FromString(expressionTypeToString),
                                            ConversionFactor = r.GetDouble(RawExposureBiomarkerConversions.ConversionFactor, fieldMap),
                                            Distribution = BiomarkerConversionDistributionConverter.FromString(distributionTypeString, BiomarkerConversionDistribution.Unspecified),
                                            VariabilityUpper = r.GetDoubleOrNull(RawExposureBiomarkerConversions.VariabilityUpper, fieldMap)
                                        };
                                        allExposureBiomarkerConversions.Add(record);
                                    }
                                }
                            }
                        }

                        // Create lookup based on combined keys
                        var lookup = allExposureBiomarkerConversions.ToDictionary(r => r.IdExposureBiomarkerConversion.ToLowerInvariant());
                        // Read exposure biomarker conversion  subgroups
                        var ebcSubgroups = new List<ExposureBiomarkerConversionSG>() { };
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawExposureBiomarkerConversionSGs>(rawDataSourceId, out int[] fieldMap)) {
                                if (r != null) {
                                    while (r?.Read() ?? false) {
                                        var idExposureBiomarkerConversion = r.GetString(RawExposureBiomarkerConversionSGs.IdExposureBiomarkerConversion, fieldMap).ToLowerInvariant();
                                        var genderToString = r.GetStringOrNull(RawExposureBiomarkerConversionSGs.Gender, fieldMap);
                                        if (lookup.TryGetValue(idExposureBiomarkerConversion, out var exposureBiomarkerConversion)) {
                                            var record = new ExposureBiomarkerConversionSG {
                                                IdExposureBiomarkerConversion = idExposureBiomarkerConversion,
                                                AgeLower = r.GetDoubleOrNull(RawExposureBiomarkerConversionSGs.AgeLower, fieldMap),
                                                Gender = GenderTypeConverter.FromString(genderToString),
                                                ConversionFactor = r.GetDouble(RawExposureBiomarkerConversionSGs.ConversionFactor, fieldMap),
                                                VariabilityUpper = r.GetDoubleOrNull(RawExposureBiomarkerConversionSGs.VariabilityUpper, fieldMap)
                                            };
                                            exposureBiomarkerConversion.EBCSubgroups.Add(record);
                                        }
                                    }
                                }
                            }

                        }
                    }
                }
                _data.AllExposureBiomarkerConversions = allExposureBiomarkerConversions;
            }
            return _data.AllExposureBiomarkerConversions;
        }
    }
}
