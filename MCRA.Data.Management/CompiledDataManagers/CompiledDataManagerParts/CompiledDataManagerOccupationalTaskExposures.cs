using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Extensions;
using MCRA.General.TableDefinitions;
using MCRA.General.TableDefinitions.RawTableFieldEnums;
using MCRA.Utils.DataFileReading;

namespace MCRA.Data.Management.CompiledDataManagers {
    public partial class CompiledDataManager {

        public ICollection<OccupationalTaskExposure> GetAllOccupationalTaskExposures() {
            if (_data.AllOccupationalTaskExposures == null) {
                LoadScope(SourceTableGroup.OccupationalTaskExposures);
                GetAllCompounds();
                GetAllOccupationalTasks();
                var allOccupationalTaskExposures = new List<OccupationalTaskExposure>();
                var rawDataSourceIds = _rawDataProvider.GetRawDatasourceIds(SourceTableGroup.OccupationalTaskExposures);
                if (rawDataSourceIds?.Count > 0) {
                    using (var rdm = _rawDataProvider.CreateRawDataManager()) {
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawOccupationalTaskExposures>(rawDataSourceId, out int[] fieldMap)) {
                                while (r?.Read() ?? false) {
                                    var substance = r.GetString(RawOccupationalTaskExposures.IdSubstance, fieldMap);
                                    var task = r.GetString(RawOccupationalTaskExposures.IdOccupationalTask, fieldMap);
                                    var validSubstance = CheckLinkSelected(ScopingType.Compounds, substance);
                                    var validTask = CheckLinkSelected(ScopingType.OccupationalTasks, task);
                                    if (validSubstance && validTask) {
                                        var OccupationalTaskExposure = new OccupationalTaskExposure {
                                            OccupationalTask = _data.AllOccupationalTasks[task],
                                            RpeType = r.GetEnum(RawOccupationalTaskExposures.RPEType, fieldMap, RPEType.Undefined),
                                            HandProtectionType = r.GetEnum(RawOccupationalTaskExposures.HandProtectionType, fieldMap, HandProtectionType.Undefined),
                                            ProtectiveClothingType = r.GetEnum(RawOccupationalTaskExposures.ProtectiveClothingType, fieldMap, ProtectiveClothingType.Undefined),
                                            ExposureRoute = r.GetEnum(RawOccupationalTaskExposures.ExposureRoute, fieldMap, ExposureRoute.Undefined),
                                            Substance = _data.GetOrAddSubstance(substance),
                                            Unit = r.GetEnum<JobTaskExposureUnit>(RawOccupationalTaskExposures.Unit, fieldMap),
                                            EstimateType = r.GetEnum(RawOccupationalTaskExposures.ExposureType, fieldMap, JobTaskExposureEstimateType.Undefined),
                                            Percentage = r.GetDouble(RawOccupationalTaskExposures.Percentage, fieldMap),
                                            Value = r.GetDouble(RawOccupationalTaskExposures.Value, fieldMap),
                                            Reference = r.GetStringOrNull(RawOccupationalTaskExposures.Reference, fieldMap)
                                        };
                                        allOccupationalTaskExposures.Add(OccupationalTaskExposure);
                                    }
                                }
                            }
                        }
                    }
                }
                _data.AllOccupationalTaskExposures = allOccupationalTaskExposures;
            }
            return _data.AllOccupationalTaskExposures;
        }

        private static void writeOccupationalTaskExposuresToCsv(string tempFolder, IEnumerable<OccupationalTaskExposure> OccupationalTaskExposure) {
            if (!OccupationalTaskExposure?.Any() ?? true) {
                return;
            }
            var td = McraTableDefinitions.Instance.GetTableDefinition(RawDataSourceTableID.OccupationalTaskExposures);
            var dt = td.CreateDataTable();
            var ccr = new int[Enum.GetNames<RawOccupationalTaskExposures>().Length];
            foreach (var occupationalTaskExposure in OccupationalTaskExposure) {
                var r = dt.NewRow();
                r.WriteNonEmptyString(RawOccupationalTaskExposures.IdOccupationalTask, occupationalTaskExposure.OccupationalTask.Code, ccr);
                r.WriteNonEmptyString(RawOccupationalTaskExposures.RPEType, occupationalTaskExposure.RpeType.ToString(), ccr);
                r.WriteNonEmptyString(RawOccupationalTaskExposures.ExposureRoute, occupationalTaskExposure.ExposureRoute.ToString(), ccr);
                r.WriteNonEmptyString(RawOccupationalTaskExposures.Unit, occupationalTaskExposure.Unit.ToString(), ccr);
                r.WriteNonEmptyString(RawOccupationalTaskExposures.IdSubstance, occupationalTaskExposure.Substance?.Code, ccr);
                r.WriteNonNullDouble(RawOccupationalTaskExposures.Percentage, occupationalTaskExposure.Percentage);
                r.WriteNonNullDouble(RawOccupationalTaskExposures.Value, occupationalTaskExposure.Value);
                r.WriteNonEmptyString(RawOccupationalTaskExposures.Reference, occupationalTaskExposure.Reference, ccr);
                dt.Rows.Add(r);
            }
            writeToCsv(tempFolder, td, dt, ccr);
        }
    }
}
