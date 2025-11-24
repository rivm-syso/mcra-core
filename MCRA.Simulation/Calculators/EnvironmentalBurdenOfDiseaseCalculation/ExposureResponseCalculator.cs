using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.CounterFactualValueModels;
using MCRA.Simulation.Calculators.ExposureResponseFunctions;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualConcentrationCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.EnvironmentalBurdenOfDiseaseCalculation {

    public class ExposureResponseCalculator {

        private readonly List<double> _defaultBinBoundaries;
        private readonly ExposureGroupingMethod _exposureGroupingMethod;
        private readonly WithinBinExposureRepresentationMethod _withinBinExposureRepresentationMethod;

        public ExposureResponseCalculator(
            ExposureGroupingMethod exposureGroupingMethod,
            List<double> defaultBinBoundaries,
            WithinBinExposureRepresentationMethod withinBinExposureRepresentationMethod
        ) {
            _exposureGroupingMethod = exposureGroupingMethod;
            _defaultBinBoundaries = defaultBinBoundaries;
            _withinBinExposureRepresentationMethod = withinBinExposureRepresentationMethod;
        }

        public List<ExposureResponseResult> ComputeFromHbmSingleValueExposures(
            ICollection<HbmSingleValueExposureSet> hbmSingleValueExposureSets,
            ICollection<IExposureResponseFunctionModel> exposureResponseFunctionModels,
            ICollection<ICounterFactualValueModel> counterFactualModels
        ) {
            var exposureResponseResults = new List<ExposureResponseResult>();
            foreach (var exposureResponseFunctionModel in exposureResponseFunctionModels) {
                var erf = exposureResponseFunctionModel.ExposureResponseFunction;
                var counterFactualModel = counterFactualModels.FirstOrDefault(c => c.ExposureResponseFunction == erf);
                var hbmSingleValueExposureSet = hbmSingleValueExposureSets
                    .FirstOrDefault(c => c.Substance == erf.Substance);
                if (hbmSingleValueExposureSet == null) {
                    var msg = $"Failed to compute effects for exposure response function {erf.Code}: missing estimates for target {erf.ExposureTarget.GetDisplayName()}.";
                    throw new Exception(msg);
                }

                var defaultUnit = hbmSingleValueExposureSet.BiologicalMatrix.GetTargetConcentrationUnit();
                var exposureUnitTriple = new ExposureUnitTriple(
                    defaultUnit.GetSubstanceAmountUnit(),
                    defaultUnit.GetConcentrationMassUnit()
                );
                var exposureUnit = new TargetUnit(new ExposureTarget(hbmSingleValueExposureSet.BiologicalMatrix), exposureUnitTriple);

                // Compute exposure response results
                var exposureResponseResult = ComputeFromHbmSingleValueExposureSet(
                    exposureResponseFunctionModel,
                    hbmSingleValueExposureSet,
                    exposureUnit,
                    _withinBinExposureRepresentationMethod
                );
                exposureResponseResults.Add(exposureResponseResult);
            }

            return exposureResponseResults;
        }

        public List<ExposureResponseResult> ComputeFromTargetIndividualExposures(
            Dictionary<ExposureTarget, (List<ITargetIndividualExposure> Exposures, TargetUnit Unit)> exposuresCollections,
            ICollection<IExposureResponseFunctionModel> exposureResponseFunctionModels,
            ICollection<ICounterFactualValueModel> counterFactualModels
        ) {
            var exposureResponseResults = new List<ExposureResponseResult>();
            var percentileIntervals = getPercentileIntervalsFromBinBoundaries(_defaultBinBoundaries);
            foreach (var exposureResponseFunctionModel in exposureResponseFunctionModels) {
                var erf = exposureResponseFunctionModel.ExposureResponseFunction;
                var counterFactualModel = counterFactualModels.FirstOrDefault(c => c.ExposureResponseFunction == erf);
                // Get exposures for target
                if (!exposuresCollections.TryGetValue(erf.ExposureTarget, out var targetExposures)) {
                    var msg = $"Failed to compute effects for exposure response function {erf.Code}: missing estimates for target {erf.ExposureTarget.GetDisplayName()}.";
                    throw new Exception(msg);
                }
                (var exposures, var exposureUnit) = (targetExposures.Exposures, targetExposures.Unit);

                // Compute exposure response results
                var exposureResponseResult = ComputeFromTargetIndividualExposures(
                    exposureResponseFunctionModel,
                    counterFactualModel,
                    exposures,
                    exposureUnit,
                    percentileIntervals,
                    _exposureGroupingMethod,
                    _withinBinExposureRepresentationMethod
                );
                exposureResponseResults.Add(exposureResponseResult);
            }
            return exposureResponseResults;
        }

        public ExposureResponseResult ComputeFromTargetIndividualExposures(
            IExposureResponseFunctionModel exposureResponseFunctionModel,
            ICounterFactualValueModel counterFactualValueModel,
            List<ITargetIndividualExposure> exposures,
            TargetUnit targetUnit,
            List<PercentileInterval> percentileIntervals,
            ExposureGroupingMethod exposureGroupingMethod,
            WithinBinExposureRepresentationMethod withinBinExposureRepresentationMethod
        ) {
            var erf = exposureResponseFunctionModel.ExposureResponseFunction;

            var substance = erf.Substance;

            var unitAlignmentFactor = targetUnit.GetAlignmentFactor(
                erf.TargetUnit, substance.MolecularMass, double.NaN
            );

            var useErfBins = erf.HasErfSubGroups
                && exposureGroupingMethod == ExposureGroupingMethod.ErfDefinedBins;

            var bins = getBins(
                exposureResponseFunctionModel,
                counterFactualValueModel,
                exposures,
                percentileIntervals,
                unitAlignmentFactor,
                useErfBins
            );

            var resultRecords = bins
                .Select((r, ix) => compute(
                    exposureResponseFunctionModel,
                    r.ExposureInterval,
                    r.PercentileInterval,
                    withinBinExposureRepresentationMethod,
                    targetUnit,
                    unitAlignmentFactor,
                    useErfBins,
                    ix
                ))
                .ToList();

            var result = new ExposureResponseResult {
                ExposureResponseFunction = erf,
                ErfDoseUnitAlignmentFactor = unitAlignmentFactor,
                TargetUnit = targetUnit,
                ExposureResponseFunctionModel = exposureResponseFunctionModel,
                ExposureResponseResultRecords = resultRecords
            };

            return result;
        }

        public ExposureResponseResult ComputeFromHbmSingleValueExposureSet(
            IExposureResponseFunctionModel exposureResponseFunctionModel,
            HbmSingleValueExposureSet hbmSingleValueExposureSet,
            TargetUnit targetUnit,
            WithinBinExposureRepresentationMethod withinBinExposureRepresentationMethod
        ) {
            var erf = exposureResponseFunctionModel.ExposureResponseFunction;
            var substance = erf.Substance;

            var unitAlignmentFactor = targetUnit.GetAlignmentFactor(
                erf.TargetUnit, substance.MolecularMass, double.NaN
            );

            var percentiles = hbmSingleValueExposureSet
                .HbmSingleValueExposures
                .Select(c => (c.Percentage, c.Value))
                .OrderBy(c => c.Percentage)
                .ToList();

            var percentages = percentiles
                .Select(c => c.Percentage)
                .ToList();
            var exposureLevels = percentiles
                .Select(c => c.Value)
                .ToList();

            var bins = getIntervals(exposureLevels, percentages);
            var topBin = bins.Last();
            if (topBin.PercentileInterval.Upper != 100) {
                var exposureInterval = new ExposureInterval(topBin.ExposureInterval.Upper, double.NaN);
                var percentileInterval = new PercentileInterval(topBin.PercentileInterval.Upper, 100.0);
                bins.Add((exposureInterval, percentileInterval));
            }

            var resultRecords = bins
                .Select((r, ix) => compute(
                    exposureResponseFunctionModel,
                    r.ExposureInterval,
                    r.PercentileInterval,
                    withinBinExposureRepresentationMethod,
                    targetUnit,
                    unitAlignmentFactor,
                    false,
                    ix
                ))
                .ToList();

            var result = new ExposureResponseResult {
                ExposureResponseFunction = erf,
                TargetUnit = targetUnit,
                ErfDoseUnitAlignmentFactor = unitAlignmentFactor,
                ExposureResponseFunctionModel = exposureResponseFunctionModel,
                ExposureResponseResultRecords = resultRecords
            };

            return result;
        }

        private List<(ExposureInterval ExposureInterval, PercentileInterval PercentileInterval)> getBins(
            IExposureResponseFunctionModel exposureResponseFunctionModel,
            ICounterFactualValueModel counterFactualValueModel,
            List<ITargetIndividualExposure> exposures,
            List<PercentileInterval> defaultPercentileIntervals,
            double unitAlignmentFactor,
            bool useErfBins
        ) {
            return useErfBins
                ? computeBinsFromErf(exposureResponseFunctionModel, counterFactualValueModel, exposures, unitAlignmentFactor)
                : calculateExposureBinsFromPercentiles(exposureResponseFunctionModel, exposures, defaultPercentileIntervals);
        }

        private List<(ExposureInterval, PercentileInterval)> computeBinsFromErf(
            IExposureResponseFunctionModel exposureResponseFunctionModel,
            ICounterFactualValueModel counterFactualValueModel,
            List<ITargetIndividualExposure> exposures,
            double unitAlignmentFactor
        ) {
            var erf = exposureResponseFunctionModel.ExposureResponseFunction;

            var substance = erf.Substance;

            var weights = exposures
                .Select(c => c.SimulatedIndividual.SamplingWeight)
                .ToList();
            var allExposures = exposures
                .Select(c => c.GetSubstanceExposure(substance))
                .ToList();

            var exposureLevels = new List<double> {
                counterFactualValueModel.GetCounterFactualValue() * unitAlignmentFactor
            };
            var maximum = allExposures.Max();
            if (erf.HasErfSubGroups) {
                exposureLevels.AddRange(
                    erf.ErfSubgroups
                        .OrderBy(r => r.ExposureUpper ?? double.PositiveInfinity)
                        .Select(r => r.ExposureUpper * unitAlignmentFactor ?? double.NaN)
                    );
            }

            var percentages = allExposures
                .PercentagesWithSamplingWeights(weights, exposureLevels)
                .ToList();

            var result = getIntervals(exposureLevels, percentages);

            return result;
        }

        private List<(ExposureInterval, PercentileInterval)> calculateExposureBinsFromPercentiles(
            IExposureResponseFunctionModel exposureResponseFunctionModel,
            List<ITargetIndividualExposure> exposures,
            List<PercentileInterval> percentileIntervals
        ) {
            var erf = exposureResponseFunctionModel.ExposureResponseFunction;

            var substance = erf.Substance;

            var weights = exposures
                .Select(c => c.SimulatedIndividual.SamplingWeight)
                .ToList();
            var allExposures = exposures
                .Select(c => c.GetSubstanceExposure(substance))
                .ToList();

            var upperBounds = percentileIntervals
                .Select(r => r.Upper)
                .ToList();

            var exposureLevels = allExposures
                .PercentilesWithSamplingWeights(weights, upperBounds)
                .ToList();

            var result = new List<(ExposureInterval, PercentileInterval)>();
            for (int i = 0; i < exposureLevels.Count; i++) {
                var percentileInterval = percentileIntervals[i];
                var exposureInterval = new ExposureInterval() {
                    Lower = i == 0 ? 0 : exposureLevels[i - 1],
                    Upper = i == exposureLevels.Count - 1 ? double.NaN : exposureLevels[i]
                };
                result.Add((exposureInterval, percentileInterval));
            }
            return result;
        }

        private List<PercentileInterval> getPercentileIntervalsFromBinBoundaries(
            List<double> binBoundaries
        ) {
            if (binBoundaries.Any(r => r <= 0 || r >= 100)) {
                var msg = "Incorrect bin boundaries specified for EBD calculations, all bin boundaries should be greater than 0 and less than 100.";
                throw new Exception(msg);
            }

            // Make sure the specified boundaries are ordered
            binBoundaries = [.. binBoundaries
                .Where(r => r > 0 && r < 100)
                .Order()
            ];

            var lowerBinBoudaries = new List<double>(binBoundaries);
            lowerBinBoudaries.Insert(0, 0D);
            var upperBinBoudaries = new List<double>(binBoundaries) { 100D };

            var percentileIntervals = lowerBinBoudaries
                .Zip(upperBinBoudaries)
                .Select(r => new PercentileInterval(r.First, r.Second))
                .ToList();
            return percentileIntervals;
        }

        private static List<(ExposureInterval ExposureInterval, PercentileInterval PercentileInterval)> getIntervals(
            List<double> exposureLevels,
            List<double> percentages
        ) {
            var result = new List<(ExposureInterval, PercentileInterval)>();
            var lower = double.NaN;
            var lowerPercentile = double.NaN;
            for (int i = 0; i < exposureLevels.Count; i++) {
                var upper = exposureLevels[i];
                var upperPercentile = !double.IsNaN(upper) ? percentages[i] : double.NaN;
                var exposureInterval = new ExposureInterval() {
                    Lower = lower,
                    Upper = upper
                };
                var percentileInterval = new PercentileInterval() {
                    Lower = lowerPercentile,
                    Upper = upperPercentile
                };
                result.Add((exposureInterval, percentileInterval));
                lower = upper;
                lowerPercentile = upperPercentile;
            }
            return result;
        }

        private ExposureResponseResultRecord compute(
            IExposureResponseFunctionModel exposureResponseFunctionModel,
            ExposureInterval exposureInterval,
            PercentileInterval percentileInterval,
            WithinBinExposureRepresentationMethod withinBinExposureRepresentationMethod,
            TargetUnit targetUnit,
            double unitAlignmentFactor,
            bool useErfBins,
            int exposureBinId
        ) {
            var erf = exposureResponseFunctionModel.ExposureResponseFunction;

            var exposureLevel = withinBinExposureRepresentationMethod switch {
                WithinBinExposureRepresentationMethod.MinimumInBin =>
                    !double.IsNaN(exposureInterval.Lower) ? exposureInterval.Lower : exposureInterval.Upper,
                WithinBinExposureRepresentationMethod.MaximumInBin =>
                    !double.IsNaN(exposureInterval.Upper) ? exposureInterval.Upper : exposureInterval.Lower,
                WithinBinExposureRepresentationMethod.AverageInBin => exposureInterval.Average,
                _ => throw new NotImplementedException()
            };

            var response = exposureResponseFunctionModel
                    .Compute(exposureLevel * unitAlignmentFactor, useErfBins);
            var result = new ExposureResponseResultRecord {
                ExposureResponseFunction = erf,
                IsErfDefinedExposureBin = useErfBins,
                ExposureBinId = exposureBinId,
                ExposureInterval = exposureInterval,
                PercentileInterval = percentileInterval,
                ExposureLevel = exposureLevel,
                PercentileSpecificRisk = response
            };
            return result;
        }
    }
}
