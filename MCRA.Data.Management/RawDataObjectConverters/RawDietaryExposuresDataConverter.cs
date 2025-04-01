using MCRA.Data.Compiled;
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Raw.Objects.RawTableGroups;
using MCRA.General.TableDefinitions.RawTableObjects;
using MCRA.General;
using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Wrappers;

namespace MCRA.Data.Management.RawDataObjectConverters {
    public sealed class RawDietaryExposuresDataConverter : RawTableGroupDataConverterBase<RawDietaryExposuresData> {

        public override RawDietaryExposuresData FromCompiledData(CompiledData data) {
            throw new NotImplementedException();
        }

        public RawDietaryExposuresData ToRaw(IEnumerable<DietaryExposureModel> dietaryExposureModels) {
            var result = new RawDietaryExposuresData();
            foreach (var model in dietaryExposureModels) {
                var modelRecord = new RawDietaryExposureModel {
                    idDietaryExposureModel = model.Code,
                    Name = model.Name,
                    Description = model.Description,
                    idSubstance = model.Compound.Code,
                    ExposureUnit = model.ExposureUnit.ToString()
                };

                foreach(var perc in model.DietaryExposurePercentiles.Values) {
                    result.DietaryExposurePercentileRecords.Add(
                        new RawDietaryExposurePercentile {
                            idDietaryExposureModel = model.Code,
                            Percentage = perc.Percentage,
                            Exposure = perc.Exposure
                        });

                    foreach(var unc in perc.ExposureUncertainties) {
                        result.DietaryExposurePercentileUncertainRecords.Add(
                            new RawDietaryExposurePercentileUncertain {
                                idDietaryExposureModel = model.Code,
                                Percentage = unc,
                            });
                    }
                }

                result.DietaryExposureModelRecords.Add(modelRecord);
            }
            return result;
        }

        public RawDietaryExposuresData ToRaw(
            ICollection<SimpleExposureStatistics> exposureStatisticss,
            double[] percentages
        ) {
            var result = new RawDietaryExposuresData();
            foreach (var exposureStatistics in exposureStatisticss) {
                var modelRecord = new RawDietaryExposureModel() {
                    idDietaryExposureModel = exposureStatistics.Code,
                    Name = exposureStatistics.Name,
                    Description = exposureStatistics.Description,
                    idSubstance = exposureStatistics.Substance.Code,
                    ExposureUnit = exposureStatistics.TargetUnit.GetShortDisplayName(TargetUnit.DisplayOption.AppendBiologicalMatrix)
                };
                var percentiles = exposureStatistics.Intakes.PercentilesWithSamplingWeights(exposureStatistics.SamplingWeights, percentages);
                var percentileRecords = percentages
                    .Select((p, ix) => new RawDietaryExposurePercentile() {
                        idDietaryExposureModel = exposureStatistics.Code,
                        Percentage = p,
                        Exposure = percentiles[ix]
                    })
                    .ToList();
                result.DietaryExposureModelRecords.Add(modelRecord);
                result.DietaryExposurePercentileRecords.AddRange(percentileRecords);
            }
            return result;
        }

        public void AppendUncertaintyRunValues(
            RawDietaryExposuresData data,
            int bootstrap,
            ICollection<SimpleExposureStatistics> exposureStatisticss,
            double[] percentages
        ) {
            foreach (var exposureStatistics in exposureStatisticss) {
                var percentiles = exposureStatistics.Intakes.PercentilesWithSamplingWeights(exposureStatistics.SamplingWeights, percentages);
                var percentileRecords = percentages
                    .Select((p, ix) => new RawDietaryExposurePercentileUncertain() {
                        idDietaryExposureModel = exposureStatistics.Code,
                        idUncertaintySet = $"{bootstrap}",
                        Percentage = p,
                        Exposure = percentiles[ix]
                    })
                    .ToList();
                data.DietaryExposurePercentileUncertainRecords.AddRange(percentileRecords);
            }
        }
    }
}
