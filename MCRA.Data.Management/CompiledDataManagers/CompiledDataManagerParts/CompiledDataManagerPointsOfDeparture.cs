using System.Globalization;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Extensions;
using MCRA.General.TableDefinitions;
using MCRA.General.TableDefinitions.RawTableFieldEnums;
using MCRA.Utils.DataFileReading;

namespace MCRA.Data.Management.CompiledDataManagers {
    public partial class CompiledDataManager {
        public ICollection<Compiled.Objects.PointOfDeparture> GetAllPointsOfDeparture() {
            if (_data.AllPointsOfDeparture == null) {
                LoadScope(SourceTableGroup.HazardDoses);
                var pointsOfDeparture = new List<Compiled.Objects.PointOfDeparture>();
                var rawDataSourceIds = _rawDataProvider.GetRawDatasourceIds(SourceTableGroup.HazardDoses);

                //if no data source specified: return immediately.
                if (rawDataSourceIds?.Count > 0) {
                    GetAllCompounds();
                    GetAllEffects();

                    using (var rdm = _rawDataProvider.CreateRawDataManager()) {

                        // Read points of departure
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawHazardDoses>(rawDataSourceId, out int[] fieldMap)) {
                                if (r != null) {
                                    while (r?.Read() ?? false) {
                                        var idModel = r.GetStringOrNull(RawHazardDoses.IdDoseResponseModel, fieldMap);
                                        var idEffect = r.GetString(RawHazardDoses.IdEffect, fieldMap);
                                        var idSubstance = r.GetString(RawHazardDoses.IdCompound, fieldMap);
                                        var noModel = string.IsNullOrEmpty(idModel);
                                        var valid = CheckLinkSelected(ScopingType.Effects, idEffect)
                                                  & CheckLinkSelected(ScopingType.Compounds, idSubstance);
                                        if (valid) {
                                            var cesString = r.GetStringOrNull(RawHazardDoses.CriticalEffectSize, fieldMap);
                                            var ces = !string.IsNullOrEmpty(cesString) ? Convert.ToDouble(cesString, CultureInfo.InvariantCulture) : double.NaN;
                                            var podTypeString = r.GetStringOrNull(RawHazardDoses.HazardDoseType, fieldMap);
                                            var podType = PointOfDepartureTypeConverter.FromString(podTypeString, PointOfDepartureType.Unspecified);

                                            var targetLevel = r.GetEnum(RawHazardDoses.TargetLevel, fieldMap, TargetLevelType.External);
                                            var exposureRoute = r.GetEnum(
                                                RawHazardDoses.ExposureRoute,
                                                fieldMap,
                                                targetLevel == TargetLevelType.External ? ExposureRoute.Oral : ExposureRoute.Undefined
                                            );

                                            var pointOfDeparture = new Compiled.Objects.PointOfDeparture {
                                                Code = idModel,
                                                Effect = _data.GetOrAddEffect(idEffect),
                                                Compound = _data.GetOrAddSubstance(idSubstance),
                                                LimitDose = r.GetDouble(RawHazardDoses.LimitDose, fieldMap),
                                                PointOfDepartureType = podType,
                                                Species = r.GetStringOrNull(RawHazardDoses.Species, fieldMap),
                                                DoseResponseModelEquation = r.GetStringOrNull(RawHazardDoses.DoseResponseModelEquation, fieldMap),
                                                DoseResponseModelParameterValues = r.GetStringOrNull(RawHazardDoses.DoseResponseModelParameterValues, fieldMap),
                                                CriticalEffectSize = ces,
                                                ExposureRoute = exposureRoute,
                                                DoseUnit = r.GetEnum(RawHazardDoses.DoseUnit, fieldMap, DoseUnit.mgPerKgBWPerDay),
                                                IsCriticalEffect = r.GetBooleanOrNull(RawHazardDoses.IsCriticalEffect, fieldMap) ?? false,
                                                BiologicalMatrix = r.GetEnum(RawHazardDoses.BiologicalMatrix, fieldMap, BiologicalMatrix.Undefined),
                                                ExpressionType = r.GetEnum(RawHazardDoses.ExpressionType, fieldMap, ExpressionType.None),
                                                TargetLevel = targetLevel,
                                                PublicationAuthors = r.GetStringOrNull(RawHazardDoses.PublicationAuthors, fieldMap),
                                                PublicationTitle = r.GetStringOrNull(RawHazardDoses.PublicationTitle, fieldMap),
                                                PublicationUri = r.GetStringOrNull(RawHazardDoses.PublicationUri, fieldMap),
                                                PublicationYear = r.GetIntOrNull(RawHazardDoses.PublicationYear, fieldMap)
                                            };
                                            pointsOfDeparture.Add(pointOfDeparture);
                                        }
                                    }
                                }
                            }
                        }

                        // Create lookup based on combined keys
                        var lookup = pointsOfDeparture.ToDictionary(r => string.Join("\a", r.Effect.Code, r.Compound.Code, r.Code), StringComparer.OrdinalIgnoreCase);

                        // Read hazard dose uncertainties
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawHazardDosesUncertain>(rawDataSourceId, out int[] fieldMap)) {
                                if (r != null) {
                                    while (r?.Read() ?? false) {
                                        var idModel = r.GetString(RawHazardDosesUncertain.IdDoseResponseModel, fieldMap);
                                        var idEffect = r.GetString(RawHazardDosesUncertain.IdEffect, fieldMap);
                                        var idCompound = r.GetString(RawHazardDosesUncertain.IdCompound, fieldMap);
                                        var idHazardDose = string.Join("\a", idEffect, idCompound, idModel);
                                        var valid = CheckLinkSelected(ScopingType.Effects, idEffect)
                                            & CheckLinkSelected(ScopingType.Compounds, idCompound);
                                        if (valid) {
                                            var model = lookup[idHazardDose];
                                            var record = new PointOfDepartureUncertain {
                                                Compound = _data.GetOrAddSubstance(idCompound),
                                                Effect = _data.GetOrAddEffect(idEffect),
                                                LimitDose = r.GetDouble(RawHazardDosesUncertain.LimitDose, fieldMap),
                                                DoseResponseModelParameterValues = r.GetStringOrNull(RawHazardDosesUncertain.DoseResponseModelParameterValues, fieldMap)
                                                // NOTE: IdUncertaintySet is not read, it is also not used in the code. Should be not required in fact.
                                            };
                                            model.PointOfDepartureUncertains.Add(record);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                _data.AllPointsOfDeparture = pointsOfDeparture;
            }

            return _data.AllPointsOfDeparture;
        }

        private static void writePointsOfDepartureDataToCsv(string tempFolder, IEnumerable<Compiled.Objects.PointOfDeparture> hazardDoses) {
            if (!hazardDoses?.Any() ?? true) {
                return;
            }

            var tdh = McraTableDefinitions.Instance.GetTableDefinition(RawDataSourceTableID.HazardDoses);
            var dth = tdh.CreateDataTable();
            var tdu = McraTableDefinitions.Instance.GetTableDefinition(RawDataSourceTableID.HazardDosesUncertain);
            var dtu = tdu.CreateDataTable();

            foreach (var hd in hazardDoses) {
                var rowHd = dth.NewRow();
                rowHd.WriteNonEmptyString(RawHazardDoses.IdDoseResponseModel, hd.Code);
                rowHd.WriteNonEmptyString(RawHazardDoses.IdEffect, hd.Effect.Code);
                rowHd.WriteNonEmptyString(RawHazardDoses.IdCompound, hd.Compound.Code);
                rowHd.WriteNonNaNDouble(RawHazardDoses.LimitDose, hd.LimitDose);
                rowHd.WriteNonEmptyString(RawHazardDoses.HazardDoseType, hd.PointOfDepartureType.ToString());
                rowHd.WriteNonEmptyString(RawHazardDoses.Species, hd.Species);
                rowHd.WriteNonEmptyString(RawHazardDoses.DoseResponseModelEquation, hd.DoseResponseModelEquation);
                rowHd.WriteNonEmptyString(RawHazardDoses.DoseResponseModelParameterValues, hd.DoseResponseModelParameterValues);
                rowHd.WriteNonNaNDouble(RawHazardDoses.CriticalEffectSize, hd.CriticalEffectSize);
                rowHd.WriteNonEmptyString(RawHazardDoses.ExposureRoute, hd.ExposureRoute.ToString());
                rowHd.WriteNonEmptyString(RawHazardDoses.DoseUnit, hd.DoseUnit.ToString());
                rowHd.WriteNonEmptyString(RawHazardDoses.BiologicalMatrix, hd.BiologicalMatrix.ToString());
                rowHd.WriteNonEmptyString(RawHazardDoses.ExpressionType, hd.ExpressionType.ToString());
                rowHd.WriteNonEmptyString(RawHazardDoses.TargetLevel, hd.TargetLevel.ToString());
                rowHd.WriteNonEmptyString(RawHazardDoses.PublicationAuthors, hd.PublicationAuthors);
                rowHd.WriteNonEmptyString(RawHazardDoses.PublicationTitle, hd.PublicationTitle);
                rowHd.WriteNonEmptyString(RawHazardDoses.PublicationUri, hd.PublicationUri);
                rowHd.WriteNonEmptyString(RawHazardDoses.PublicationYear, hd.PublicationYear.HasValue ? hd.PublicationYear.Value.ToString() : "");

                dth.Rows.Add(rowHd);

                foreach (var hdu in hd.PointOfDepartureUncertains) {
                    var rowHdu = dtu.NewRow();
                    rowHdu.WriteNonEmptyString(RawHazardDosesUncertain.IdDoseResponseModel, hd.Code);
                    rowHdu.WriteNonEmptyString(RawHazardDosesUncertain.IdEffect, hd.Effect.Code);
                    rowHdu.WriteNonEmptyString(RawHazardDosesUncertain.IdCompound, hd.Compound.Code);
                    rowHdu.WriteNonNaNDouble(RawHazardDosesUncertain.LimitDose, hdu.LimitDose);
                    rowHdu.WriteNonEmptyString(RawHazardDosesUncertain.DoseResponseModelParameterValues, hdu.DoseResponseModelParameterValues);
                    dtu.Rows.Add(rowHdu);
                }
            }

            writeToCsv(tempFolder, tdh, dth);
            writeToCsv(tempFolder, tdu, dtu);
        }
    }
}
