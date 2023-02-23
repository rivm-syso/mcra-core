using MCRA.Data.Compiled.Objects;
using MCRA.General.TableDefinitions.RawTableFieldEnums;
using MCRA.General;
using MCRA.General.Extensions;
using MCRA.General.TableDefinitions;
using MCRA.Utils.DataFileReading;

namespace MCRA.Data.Management.CompiledDataManagers {
    public partial class CompiledDataManager {

        /// <summary>
        /// Returns the list of all unit variability factors.
        /// </summary>
        public IList<UnitVariabilityFactor> GetAllUnitVariabilityFactors() {
            if (_data.AllUnitVariabilityFactors == null) {
                LoadScope(SourceTableGroup.UnitVariabilityFactors);
                var allUnitVariabilityFactors = new List<UnitVariabilityFactor>();
                var rawDataSourceIds = _rawDataProvider.GetRawDatasourceIds(SourceTableGroup.UnitVariabilityFactors);
                if (rawDataSourceIds?.Any() ?? false) {
                    GetAllFoods();
                    GetAllCompounds();
                    GetAllProcessingTypes();
                    using (var rdm = _rawDataProvider.CreateRawDataManager()) {
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawUnitVariabilityFactors>(rawDataSourceId, out int[] fieldMap)) {
                                while (r?.Read() ?? false) {
                                    var idFood = r.GetString(RawUnitVariabilityFactors.IdFood, fieldMap);
                                    var idSubstance = r.GetStringOrNull(RawUnitVariabilityFactors.IdCompound, fieldMap);
                                    var noSubstance = string.IsNullOrEmpty(idSubstance);
                                    var idProcType = r.GetStringOrNull(RawUnitVariabilityFactors.IdProcessingType, fieldMap);
                                    var noProcType = string.IsNullOrEmpty(idProcType);
                                    var valid = CheckLinkSelected(ScopingType.Foods, idFood)
                                              & (noSubstance || CheckLinkSelected(ScopingType.Compounds, idSubstance))
                                              & (noProcType || CheckLinkSelected(ScopingType.ProcessingTypes, idProcType));
                                    if (valid) {
                                        var food = getOrAddFood(idFood);
                                        var compound = noSubstance ? null : _data.GetOrAddSubstance(idSubstance);
                                        var procType = noProcType ? null : _data.GetOrAddProcessingType(idProcType);
                                        var uv = new UnitVariabilityFactor {
                                            Food = food,
                                            Compound = compound,
                                            ProcessingType = procType,
                                            Factor = r.GetDoubleOrNull(RawUnitVariabilityFactors.Factor, fieldMap),
                                            UnitsInCompositeSample = r.GetDouble(RawUnitVariabilityFactors.UnitsInCompositeSample, fieldMap),
                                            Coefficient = r.GetDoubleOrNull(RawUnitVariabilityFactors.Coefficient, fieldMap)
                                        };
                                        allUnitVariabilityFactors.Add(uv);
                                    }
                                }
                            }
                        }
                    }
                }
                _data.AllUnitVariabilityFactors = allUnitVariabilityFactors;
            }
            return _data.AllUnitVariabilityFactors;
        }

        /// <summary>
        /// Returns the list of all IESTI special cases.
        /// </summary>
        public IList<IestiSpecialCase> GetAllIestiSpecialCases() {
            if (_data.AllIestiSpecialCases == null) {
                //LoadScope(SourceTableGroup.UnitVariabilityFactors);
                var allIestiSpecialCases = new List<IestiSpecialCase>();
                var rawDataSourceIds = _rawDataProvider.GetRawDatasourceIds(SourceTableGroup.UnitVariabilityFactors);
                if (rawDataSourceIds?.Any() ?? false) {
                    GetAllFoods();
                    GetAllCompounds();
                    using (var rdm = _rawDataProvider.CreateRawDataManager()) {
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawIestiSpecialCases>(rawDataSourceId, out int[] fieldMap)) {
                                while (r?.Read() ?? false) {
                                    var idFood = r.GetString(RawIestiSpecialCases.IdFood, fieldMap);
                                    var idSubstance = r.GetString(RawIestiSpecialCases.IdSubstance, fieldMap);
                                    var valid = CheckLinkSelected(ScopingType.Foods, idFood)
                                              & CheckLinkSelected(ScopingType.Compounds, idSubstance);
                                    if (valid) {
                                        var food = getOrAddFood(idFood);
                                        var substance = _data.GetOrAddSubstance(idSubstance);
                                        var record = new IestiSpecialCase {
                                            Food = food,
                                            Substance = substance,
                                            ApplicationTypeString = r.GetString(RawIestiSpecialCases.ApplicationType, fieldMap),
                                            Reference = r.GetStringOrNull(RawIestiSpecialCases.Reference, fieldMap),
                                        };
                                        allIestiSpecialCases.Add(record);
                                    }
                                }
                            }
                        }
                    }
                }
                _data.AllIestiSpecialCases = allIestiSpecialCases;
            }
            return _data.AllIestiSpecialCases;
        }

        private static void writeUnitVariabilityFactorsDataToCsv(string tempFolder, IEnumerable<UnitVariabilityFactor> factors) {
            if (!factors?.Any() ?? true) {
                return;
            }

            var tdu = McraTableDefinitions.Instance.GetTableDefinition(RawDataSourceTableID.UnitVariabilityFactors);
            var dtu = tdu.CreateDataTable();

            var ccr = new int[Enum.GetNames(typeof(RawUnitVariabilityFactors)).Length];

            foreach (var uvf in factors) {
                var rowuvf = dtu.NewRow();
                rowuvf.WriteNonEmptyString(RawUnitVariabilityFactors.IdFood, uvf.Food.Code, ccr);
                rowuvf.WriteNonEmptyString(RawUnitVariabilityFactors.IdCompound, uvf.Compound.Code, ccr);
                rowuvf.WriteNonEmptyString(RawUnitVariabilityFactors.IdProcessingType, uvf.ProcessingType.Code, ccr);
                rowuvf.WriteNonNullDouble(RawUnitVariabilityFactors.Factor, uvf.Factor, ccr);
                rowuvf.WriteNonNaNDouble(RawUnitVariabilityFactors.UnitsInCompositeSample, uvf.UnitsInCompositeSample, ccr);
                rowuvf.WriteNonNullDouble(RawUnitVariabilityFactors.Coefficient, uvf.Coefficient, ccr);

                dtu.Rows.Add(rowuvf);
            }

            writeToCsv(tempFolder, tdu, dtu, ccr);
        }

        private static void writeIestiSpecialCasesDataToCsv(string tempFolder, IEnumerable<IestiSpecialCase> items) {
            if (!items?.Any() ?? true) {
                return;
            }

            var td = McraTableDefinitions.Instance.GetTableDefinition(RawDataSourceTableID.IestiSpecialCases);
            var dt = td.CreateDataTable();

            var ccr = new int[Enum.GetNames(typeof(RawIestiSpecialCases)).Length];

            foreach (var item in items) {
                var newRow = dt.NewRow();
                newRow.WriteNonEmptyString(RawIestiSpecialCases.IdFood, item.Food.Code, ccr);
                newRow.WriteNonEmptyString(RawIestiSpecialCases.IdSubstance, item.Substance.Code, ccr);
                newRow.WriteNonEmptyString(RawIestiSpecialCases.Reference, item.Reference, ccr);
                newRow.WriteNonEmptyString(RawIestiSpecialCases.ApplicationType, item.ApplicationTypeString, ccr);

                dt.Rows.Add(newRow);
            }

            writeToCsv(tempFolder, td, dt, ccr);
        }
    }
}
