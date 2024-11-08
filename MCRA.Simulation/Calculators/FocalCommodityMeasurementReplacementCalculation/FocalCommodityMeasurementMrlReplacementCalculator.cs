using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;
using MCRA.General;

namespace MCRA.Simulation.Calculators.FocalCommodityMeasurementReplacementCalculation {

    public sealed class FocalCommodityMeasurementMrlReplacementCalculator : FocalCommodityMeasurementReplacementCalculatorBase {

        /// <summary>
        /// States whether the background measurements should be resplaced by a limit value.
        /// </summary>
        public IDictionary<(Food, Compound), ConcentrationLimit> MaximumResidueLimits { get; set; }

        /// <summary>
        /// The concentration unit of the concentration data.
        /// </summary>
        public ConcentrationUnit ConcentrationUnit { get; set; }

        public FocalCommodityMeasurementMrlReplacementCalculator(
            double focalCommodityScenarioOccurrencePercentage,
            IDictionary<(Food, Compound), ConcentrationLimit> maximumConcentrationLimits,
            ICollection<DeterministicSubstanceConversionFactor> substanceConversions,
            double focalCommodityConcentrationAdjustmentFactor,
            ConcentrationUnit concentrationUnit
        ) : base(
            substanceConversions,
            focalCommodityScenarioOccurrencePercentage,
            focalCommodityConcentrationAdjustmentFactor
        ) {
            MaximumResidueLimits = maximumConcentrationLimits;
            ConcentrationUnit = concentrationUnit;
        }

        protected override List<SampleCompoundRecord> drawImputationRecords(
            Food food,
            ICollection<Compound> substances,
            int numberOfSamples,
            double replacementFraction,
            IDictionary<Compound, List<DeterministicSubstanceConversionFactor>> substanceConversions,
            IRandom random
        ) {

            // If method is replace by limit value, then create mrl by substance lookup dictionary
            var substanceMrls = new Dictionary<Compound, double>();
            foreach (var substance in substances) {
                if (MaximumResidueLimits == null
                    || !MaximumResidueLimits.TryGetValue((food, substance), out var mrlRecord)
                    || double.IsNaN(mrlRecord.Limit)
                ) {
                    throw new Exception($"No limit value found for focal commodity combination {food.Name} ({food.Code}) and {substance.Name} ({substance.Code}).");
                }
                var unitCorrection = mrlRecord.ConcentrationUnit.GetConcentrationUnitMultiplier(ConcentrationUnit);
                substanceMrls.Add(substance, unitCorrection * mrlRecord.Limit);
            }

            // Create the specific number of sample compound records
            var result = new List<SampleCompoundRecord>(numberOfSamples);
            for (int i = 0; i < numberOfSamples; i++) {
                if (random.NextDouble() <= replacementFraction) {
                    // Add active substance record(s) based on MRL
                    var sampleCompoundRecord = new SampleCompoundRecord() {
                        SampleCompounds = []
                    };
                    foreach (var measuredSubstance in substances) {
                        if (substanceConversions?.ContainsKey(measuredSubstance) ?? false) {
                            var activeSubstanceConversions = substanceConversions[measuredSubstance];
                            foreach (var substanceConversion in activeSubstanceConversions) {
                                var sampleCompound = new SampleCompound() {
                                    MeasuredSubstance = measuredSubstance,
                                    ActiveSubstance = substanceConversion.ActiveSubstance,
                                    Residue = FocalCommodityConcentrationAdjustmentFactor * substanceConversion.ConversionFactor * substanceMrls[measuredSubstance],
                                    ResType = ResType.VAL,
                                    Loq = double.NaN,
                                    Lod = double.NaN,
                                };
                                sampleCompoundRecord.SampleCompounds.Add(substanceConversion.ActiveSubstance, sampleCompound);
                            }
                        } else {
                            var sampleCompound = new SampleCompound() {
                                MeasuredSubstance = measuredSubstance,
                                ActiveSubstance = measuredSubstance,
                                Residue = FocalCommodityConcentrationAdjustmentFactor * substanceMrls[measuredSubstance],
                                ResType = ResType.VAL,
                                Loq = double.NaN,
                                Lod = double.NaN,
                            };
                            sampleCompoundRecord.SampleCompounds.Add(measuredSubstance, sampleCompound);
                        }
                    }
                    result.Add(sampleCompoundRecord);
                } else {
                    // Add zero-concentration active substance record(s)
                    var sampleCompoundRecord = createZeroConcentrationsFocalCommoditySample(substances, substanceConversions);
                    result.Add(sampleCompoundRecord);
                }
            }
            return result;
        }
    }
}
