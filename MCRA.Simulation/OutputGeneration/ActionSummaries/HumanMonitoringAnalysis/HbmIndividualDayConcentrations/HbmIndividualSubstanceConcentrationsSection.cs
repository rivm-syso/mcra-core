using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Simulation.OutputGeneration {

    /// <summary>
    /// Acute risk assessment
    /// Summarizes an exposure matrix for acute risk assessment, including nondietary exposure if relevant. 
    /// This table can be downloaded (limited to approx. 100.000 records).
    /// </summary>
    public sealed class HbmIndividualSubstanceConcentrationsSection : SummarySection {

        public int TruncatedIndividualDaysCount { get; set; }

        public List<HbmIndividualSubstanceConcentrationsRecord> Records = new List<HbmIndividualSubstanceConcentrationsRecord>();

        public void Summarize(
            ICollection<HbmIndividualConcentration> individualConcentrations,
            ICollection<Compound> selectedSubstances,
            HumanMonitoringSamplingMethod biologicalMatrix
        ) {
            var limit = 100000;
            var results = new List<HbmIndividualSubstanceConcentrationsRecord>(limit);
            var summarizedIndividualDaysCount = 0;

            foreach (var item in individualConcentrations) {
                var individual = item.Individual;
                foreach (var substance in selectedSubstances) {
                    var concentration = item.GetExposureForSubstance(substance);
                    if (concentration > 0) {
                        results.Add(new HbmIndividualSubstanceConcentrationsRecord() {
                            SimulatedIndividualDayId = $"{individual.Code}",
                            HumanMonitoringSurveyIndividualCode = individual.Code,
                            Bodyweight = individual.BodyWeight,
                            SamplingWeight = individual.SamplingWeight,
                            SubstanceCode = substance.Code,
                            Concentration = concentration,
                            BiologicalMatrix = biologicalMatrix.BiologicalMatrixCode,
                            SamplingType = biologicalMatrix.SampleTypeCode
                        });
                    }
                }
                summarizedIndividualDaysCount++;
                if (results.Count > limit) {
                    TruncatedIndividualDaysCount = summarizedIndividualDaysCount;
                    break;
                }
            }
            Records = results.ToList();
        }
    }
}
