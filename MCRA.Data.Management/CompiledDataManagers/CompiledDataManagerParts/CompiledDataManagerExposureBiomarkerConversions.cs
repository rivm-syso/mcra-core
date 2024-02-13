using MCRA.Utils.DataFileReading;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Extensions;
using MCRA.General.TableDefinitions;
using MCRA.General.TableDefinitions.RawTableFieldEnums;
using RDotNet;

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
                if (rawDataSourceIds?.Any() ?? false) {
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

        private static void writeExposureBiomarkerConversionsDataToCsv(string tempFolder, ICollection<ExposureBiomarkerConversion> exposureBiomarkerConversions) {
            if (!exposureBiomarkerConversions?.Any() ?? true) {
                return;
            }

            var td = McraTableDefinitions.Instance.GetTableDefinition(RawDataSourceTableID.ExposureBiomarkerConversions);
            var dt = td.CreateDataTable();
            foreach (var t in exposureBiomarkerConversions) {
                var row = dt.NewRow();
                row.WriteNonEmptyString(RawExposureBiomarkerConversions.IdExposureBiomarkerConversion, t.IdExposureBiomarkerConversion);
                row.WriteNonEmptyString(RawExposureBiomarkerConversions.IdSubstanceFrom, t.SubstanceFrom.Code);
                row.WriteNonEmptyString(RawExposureBiomarkerConversions.IdSubstanceTo, t.SubstanceTo.Code);
                row.WriteNonEmptyString(RawExposureBiomarkerConversions.BiologicalMatrix, t.BiologicalMatrix.ToString());
                row.WriteNonEmptyString(RawExposureBiomarkerConversions.ExpressionTypeFrom, t.ExpressionTypeFrom.ToString());
                row.WriteNonEmptyString(RawExposureBiomarkerConversions.UnitFrom, t.UnitFrom.ToString());
                row.WriteNonEmptyString(RawExposureBiomarkerConversions.UnitTo, t.UnitTo.ToString());
                row.WriteNonEmptyString(RawExposureBiomarkerConversions.ExpressionTypeTo, t.ExpressionTypeTo.ToString());
                row.WriteNonNullDouble(RawExposureBiomarkerConversions.ConversionFactor, t.ConversionFactor);
                row.WriteNonEmptyString(RawExposureBiomarkerConversions.VariabilityDistributionType, t.Distribution.ToString());
                row.WriteNonNullDouble(RawExposureBiomarkerConversions.VariabilityUpper, t.VariabilityUpper);
                dt.Rows.Add(row);
            }
            writeToCsv(tempFolder, td, dt);
        }
    }
}
