using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualDayConcentrationCalculation;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.OutputGeneration {

    /// <summary>
    /// Acute risk assessment
    /// Summarizes an exposure matrix for acute risk assessment, including nondietary exposure if relevant. 
    /// This table can be downloaded (limited to approx. 100,000 records).
    /// </summary>
    public sealed class HbmIndividualDaySubstanceConcentrationsSection : SummarySection {

        public int TruncatedIndividualDaysCount { get; set; }

        public List<HbmIndividualSubstanceConcentrationsRecord> Records = new();

        public void Summarize(
            ICollection<HbmIndividualDayCollection> hbmIndividualDayCollections,
            ICollection<Compound> substances,
            HumanMonitoringSamplingMethod samplingMethod
        ) {
            var limit = 100000;
            var results = new List<HbmIndividualSubstanceConcentrationsRecord>(limit);
            var summarizedIndividualDaysCount = 0;
            foreach (var collection in hbmIndividualDayCollections) {
                foreach (var day in collection.HbmIndividualDayConcentrations) {
                    var individual = day.Individual;
                    foreach (var compound in substances) {
                        if (day.ConcentrationsBySubstance.TryGetValue(compound, out var concentration)) {
                            results.Add(new HbmIndividualSubstanceConcentrationsRecord() {
                                SimulatedIndividualDayId = $"{day.Individual.Code}-{day.Day}",
                                HumanMonitoringSurveyIndividualCode = day.Individual.Code,
                                HumanMonitoringSurveyDay = day.Day,
                                Bodyweight = individual.BodyWeight,
                                SamplingWeight = individual.SamplingWeight,
                                SubstanceCode = compound.Code,
                                Concentration = concentration.Concentration,
                                BiologicalMatrix = collection.TargetUnit.BiologicalMatrix.GetDisplayName(),
                                SamplingType = samplingMethod.SampleTypeCode
                            });
                        }
                    }
                    summarizedIndividualDaysCount++;
                    if (results.Count > limit) {
                        TruncatedIndividualDaysCount = summarizedIndividualDaysCount;
                        break;
                    }
                }
                Records.AddRange(results);
            }
        }
    }
}
