using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;
using MCRA.Simulation.OutputGeneration.ActionSummaries.HumanMonitoringData;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class TargetExposuresBySubstanceSection : DistributionCompoundSectionBase {
        public TargetLevelType TargetLevel { get; set; }

        public List<DistributionCompoundRecord> Records { get; set; }

        public List<SubstanceTargetExposurePercentilesRecord> SubstanceBoxPlotRecords { get; set; }

        public void Summarize(
            ICollection<AggregateIndividualExposure> aggregateIndividualExposures,
            ICollection<AggregateIndividualDayExposure> aggregateIndividualDayExposures,
            ICollection<Compound> substances,
            List<string> indexOrder,
            bool isPerPerson
        ) {
            TargetLevel = TargetLevelType.Internal;
            if (aggregateIndividualExposures != null) {
                SubstanceBoxPlotRecords = SummarizeBoxPotChronic(aggregateIndividualExposures, substances, isPerPerson)
                    .OrderBy(x => indexOrder.IndexOf(x.SubstanceCode))
                    .ToList();
            } else {
                SubstanceBoxPlotRecords = SummarizeBoxPotAcute(aggregateIndividualDayExposures, substances, isPerPerson)
                    .OrderBy(x => indexOrder.IndexOf(x.SubstanceCode))
                    .ToList();
            }
        }

        public void Summarize(
            ICollection<DietaryIndividualDayIntake> dietaryIndividualDayIntakes,
            ICollection<Compound> substances,
            List<string> indexOrder,
            ExposureType exposureType,
            bool isPerPerson
        ) {
            TargetLevel = TargetLevelType.External;
            if (exposureType == ExposureType.Acute) {
                SubstanceBoxPlotRecords = SummarizeBoxPotAcute(dietaryIndividualDayIntakes, substances, isPerPerson)
                    .OrderBy(x => indexOrder.IndexOf(x.SubstanceCode))
                    .ToList(); 
            } else {
                SubstanceBoxPlotRecords = SummarizeBoxPotChronic(dietaryIndividualDayIntakes, substances, isPerPerson)
                    .OrderBy(x => indexOrder.IndexOf(x.SubstanceCode))
                    .ToList();
            }
        }
    }
}
