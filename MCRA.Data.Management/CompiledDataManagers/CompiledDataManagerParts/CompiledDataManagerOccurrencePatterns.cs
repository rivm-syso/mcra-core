using MCRA.Utils.DataFileReading;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Extensions;
using MCRA.General.TableDefinitions;
using MCRA.General.TableDefinitions.RawTableFieldEnums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Data.Management.CompiledDataManagers {
    public partial class CompiledDataManager {
        /// <summary>
        /// Returns all occurrence patterns of the compiled datasource.
        /// </summary>
        public ICollection<OccurrencePattern> GetAllOccurrencePatterns() {
            if (_data.AllOccurrencePatterns == null) {
                LoadScope(SourceTableGroup.AgriculturalUse);
                var allOccurrencePatterns = new HashSet<OccurrencePattern>();
                var rawDataSourceIds = _rawDataProvider.GetRawDatasourceIds(SourceTableGroup.AgriculturalUse);
                //if no data source specified: return immediately.
                if (rawDataSourceIds?.Any() ?? false) {
                    GetAllFoods();
                    GetAllCompounds();

                    // Read agricultural use groups
                    using (var rdm = _rawDataProvider.CreateRawDataManager()) {

                        var occurrencePatternGroups = new Dictionary<string, List<OccurrencePattern>>(StringComparer.OrdinalIgnoreCase);
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawAgriculturalUses>(rawDataSourceId, out int[] fieldMap)) {
                                while (r?.Read() ?? false) {
                                    var idAgriculturalUse = r.GetString(RawAgriculturalUses.IdAgriculturalUse, fieldMap);
                                    var idFood = r.GetString(RawAgriculturalUses.IdFood, fieldMap);

                                    //check all linked entities
                                    var valid = IsCodeSelected(ScopingType.OccurrencePatterns, idAgriculturalUse)
                                              & CheckLinkSelected(ScopingType.Foods, idFood);
                                    //skip filtered or non linked effects here
                                    if (!valid) {
                                        continue;
                                    }

                                    var op = new OccurrencePattern {
                                        Code = idAgriculturalUse,
                                        Food = getOrAddFood(idFood),
                                        OccurrenceFraction = r.GetDouble(RawAgriculturalUses.PercentageCropTreated, fieldMap) / 100D,
                                        Location = r.GetStringOrNull(RawAgriculturalUses.Location, fieldMap),
                                        StartDate = r.GetDateTimeOrNull(RawAgriculturalUses.StartDate, fieldMap),
                                        EndDate = r.GetDateTimeOrNull(RawAgriculturalUses.EndDate, fieldMap),
                                        Name = r.GetStringOrNull(RawAgriculturalUses.Name, fieldMap),
                                        Description = r.GetStringOrNull(RawAgriculturalUses.Description, fieldMap)
                                    };
                                    allOccurrencePatterns.Add(op);

                                    //add to temporary group list per (non-unique) IdAgriculturalUse.
                                    if (!occurrencePatternGroups.TryGetValue(op.Code, out List<OccurrencePattern> groupList)) {
                                        groupList = new List<OccurrencePattern>();
                                        occurrencePatternGroups.Add(op.Code, groupList);
                                    }
                                    groupList.Add(op);
                                }
                            }
                        }

                        // Read/link substances of occurrence paterns groups
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawAgriculturalUses_has_Compounds>(rawDataSourceId, out int[] fieldMap)) {
                                while (r?.Read() ?? false) {
                                    var idAgriculturalUse = r.GetString(RawAgriculturalUses_has_Compounds.IdAgriculturalUse, fieldMap);
                                    var idSubstance = r.GetString(RawAgriculturalUses_has_Compounds.IdCompound, fieldMap);
                                    var valid = CheckLinkSelected(ScopingType.OccurrencePatterns, idAgriculturalUse)
                                              & CheckLinkSelected(ScopingType.Compounds, idSubstance);
                                    if (valid) {
                                        foreach (var op in occurrencePatternGroups[idAgriculturalUse]) {
                                            op.Compounds.Add(_data.GetOrAddSubstance(idSubstance));
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                _data.AllOccurrencePatterns = allOccurrencePatterns;
            }
            return _data.AllOccurrencePatterns;
        }

        private static void writeOccurrencePatternDataToCsv(string tempFolder, IEnumerable<OccurrencePattern> occurrencePatterns) {
            if (!occurrencePatterns?.Any() ?? true) {
                return;
            }

            var tda = McraTableDefinitions.Instance.GetTableDefinition(RawDataSourceTableID.AgriculturalUses);
            var dta = tda.CreateDataTable();
            var tdc = McraTableDefinitions.Instance.GetTableDefinition(RawDataSourceTableID.AgriculturalUsesHasCompounds);
            var dtc = tdc.CreateDataTable();
            var ccra = new int[Enum.GetNames(typeof(RawAgriculturalUses)).Length];

            foreach (var op in occurrencePatterns) {
                var rowa = dta.NewRow();
                rowa.WriteNonEmptyString(RawAgriculturalUses.IdAgriculturalUse, op.Code, ccra);
                rowa.WriteNonEmptyString(RawAgriculturalUses.IdFood, op.Food.Code, ccra);
                rowa.WriteNonEmptyString(RawAgriculturalUses.Location, op.Location, ccra);
                rowa.WriteNonNaNDouble(RawAgriculturalUses.PercentageCropTreated, op.OccurrenceFraction * 100D, ccra);
                rowa.WriteNonNullDateTime(RawAgriculturalUses.StartDate, op.StartDate, ccra);
                rowa.WriteNonNullDateTime(RawAgriculturalUses.EndDate, op.EndDate, ccra);
                dta.Rows.Add(rowa);

                foreach (var agc in op.Compounds) {
                    var rowc = dtc.NewRow();
                    rowc.WriteNonEmptyString(RawAgriculturalUses_has_Compounds.IdAgriculturalUse, op.Code);
                    rowc.WriteNonEmptyString(RawAgriculturalUses_has_Compounds.IdCompound, agc.Code);
                    dtc.Rows.Add(rowc);
                }
            }

            writeToCsv(tempFolder, tda, dta, ccra);
            writeToCsv(tempFolder, tdc, dtc);
        }
    }
}
