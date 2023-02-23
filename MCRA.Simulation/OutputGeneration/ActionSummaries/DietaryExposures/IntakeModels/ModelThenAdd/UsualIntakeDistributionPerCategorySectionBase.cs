using MCRA.Simulation.Calculators.IntakeModelling.ModelThenAddIntakeModelCalculation;

namespace MCRA.Simulation.OutputGeneration {
    public abstract class UsualIntakeDistributionPerCategorySectionBase : SummarySection {

        public override bool SaveTemporaryData => true;

        public List<CategorizedIndividualExposure> IndividualExposuresByCategory { get; set; }

        public List<Category> Categories { get; set; }
    }
}
