using System.Collections.Generic;
using System.ComponentModel;
using MCRA.General.Action.Settings.Dto;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.DietaryExposureImputationCalculation;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class CompoundExposureImputationPercentileSection : SummarySection {

        [DisplayName("Percentiles")]
        public IntakePercentileSection IntakePercentileSection { get; set; }

        [DisplayName("Retain & Refine percentiles")]
        public IntakePercentileSection RRIntakePercentileSection { get; set; }

    }
}
