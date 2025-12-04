using System.Globalization;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Extensions;
using MCRA.General.TableDefinitions.RawTableFieldEnums;

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
    }
}
