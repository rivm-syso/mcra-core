using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Simulation.OutputGeneration {

    /// <summary>
    /// Summarizes the concentrations of modelled foods from input data
    /// </summary>
    public class SamplesByFoodSubstanceSection : SummarySection {

        private double _lowerPercentage;
        private double _upperPercentage;
        public List<SamplesByFoodSubstanceRecord> ConcentrationInputDataRecords { get; set; }

        public int TotalNumberOfSamples { get; set; }
        public int NumberOfDetects { get; set; }
        public int NumberOfCensoredValues { get; set; }

        public bool HasAuthorisations { get; set; }

        public int NumberOfCompoundsWithConcentrations {
            get {
                return this.ConcentrationInputDataRecords?
                    .Where(r => r.TotalCount - r.MissingValuesCount > 0)
                    .Select(r => r.CompoundCode)
                    .Distinct()
                    .Count() ?? 0;
            }
        }

        public void Summarize(
            ICollection<SampleCompoundCollection> sampleCompoundCollections,
            ICollection<(Food Food, Compound Substance)> focalCommodityCombinations,
            double lowerPercentage,
            double upperPercentage
        ) {
            _lowerPercentage = lowerPercentage;
            _upperPercentage = upperPercentage;
            HasAuthorisations = sampleCompoundCollections.Any(r => r.SampleCompoundRecords.Any(scr => !scr.AuthorisedUse));

            var records = sampleCompoundCollections
                .AsParallel()
                .WithDegreeOfParallelism(20)
                .SelectMany(sampleCompoundCollection => summarizeFoodSamples(focalCommodityCombinations, sampleCompoundCollection))
                .ToList();

            ConcentrationInputDataRecords = records
                .OrderBy(c => c.CompoundName, System.StringComparer.OrdinalIgnoreCase)
                .ThenBy(c => c.FoodName, System.StringComparer.OrdinalIgnoreCase)
                .ToList();
            ConcentrationInputDataRecords.TrimExcess();

            NumberOfDetects = ConcentrationInputDataRecords.Sum(r => r.PositivesCount);
            TotalNumberOfSamples = ConcentrationInputDataRecords.Sum(r => r.TotalCount);
            NumberOfCensoredValues = TotalNumberOfSamples - NumberOfDetects;
        }

        protected List<SamplesByFoodSubstanceRecord> summarizeFoodSamples(
            ICollection<(Food Food, Compound Substance)> focalCommodityCombinations,
            SampleCompoundCollection sampleCompoundCollection
        ) {
            var allSamplesCount = sampleCompoundCollection.SampleCompoundRecords.Count;
            var recordsForFood = sampleCompoundCollection.SampleCompoundRecords
                .SelectMany(scr => scr.SampleCompounds)
                .GroupBy(sc => sc.Key)
                .SelectMany(sbas => {
                    var activeSubstance = sbas.Key;
                    var samplesByActiveSubstance = sbas.Select(r => r.Value).ToList();
                    var samplesByMeasuredSubstance = samplesByActiveSubstance.GroupBy(r => (r.MeasuredSubstance, r.IsExtrapolated));
                    var recordsForActiveSubstance = samplesByMeasuredSubstance
                        .Select(sbms => {
                            var measuredSubstance = sbms.Key.MeasuredSubstance;
                            var detects = sbms
                                .Where(c => !c.IsMissingValue && !c.IsCensoredValue)
                                .Select(c => c.Residue)
                                .ToList();
                            var positives = sbms
                                .Where(c => c.IsPositiveResidue)
                                .Select(c => c.Residue)
                                .ToList();
                            var zerosCount = sbms.Where(c => c.IsZeroConcentration).Count();
                            var positivesCount = positives.Count();
                            var detectsCount = detects.Count;
                            var censoredValues = sbms.Where(c => !c.IsMissingValue && c.IsCensoredValue);
                            var censoredValuesCount = censoredValues.Count();
                            var missingValueCount = allSamplesCount - (detectsCount + censoredValuesCount);
                            var percentiles = new double[2];
                            if (detects.Any()) {
                                percentiles = detects.Percentiles(_lowerPercentage, _upperPercentage);
                            }
                            var isFocalCombination = focalCommodityCombinations != null
                                && (focalCommodityCombinations.Contains((sampleCompoundCollection.Food, sbms.Key.MeasuredSubstance))
                                || focalCommodityCombinations.Contains((sampleCompoundCollection.Food, activeSubstance)));

                            var loqs = censoredValues.Where(r => r.IsNonQuantification && !double.IsNaN(r.Loq)).ToList();
                            var lods = censoredValues.Where(r => r.IsNonDetect && !double.IsNaN(r.Lod)).ToList();
                            return new SamplesByFoodSubstanceRecord() {
                                FoodName = sampleCompoundCollection.Food.Name,
                                FoodCode = sampleCompoundCollection.Food.Code,
                                CompoundName = activeSubstance.Name,
                                CompoundCode = activeSubstance.Code,
                                MeasuredCompoundCode = measuredSubstance?.Code,
                                MeasuredCompoundName = measuredSubstance?.Name,
                                Extrapolated = sbms.Key.IsExtrapolated,
                                FocalCombination = isFocalCombination,
                                TotalCount = allSamplesCount,
                                ZerosCount = zerosCount,
                                PositivesCount = positivesCount,
                                CensoredValuesCount = censoredValuesCount,
                                MissingValuesCount = missingValueCount,
                                MeanAllResidues = detectsCount == 0 ? (double?)null : detects.Sum() / (detectsCount + censoredValuesCount),
                                MeanLORs = censoredValues.Any() ? censoredValues.Select(r => r.Lor).Average() : double.NaN,
                                MeanLODs = lods.Any() ? lods.Select(r => r.Lod).Average() : double.NaN,
                                MeanLOQs = loqs.Any() ? loqs.Select(r => r.Loq).Average() : double.NaN,
                                MeanPositiveResidues = positives.Any() ? positives.Average() : (double?)null,
                                LowerPercentilePositives = detects.Any() ? percentiles[0] : double.NaN,
                                UpperPercentilePositives = detects.Any() ? percentiles[1] : double.NaN,
                                Minimum = detects.Any() ? detects.Min() : double.NaN,
                                Maximum = detects.Any() ? detects.Max() : double.NaN,
                            };
                        })
                        .ToList();
                    return recordsForActiveSubstance;
                })
                .ToList();
            return recordsForFood;
        }
    }
}
