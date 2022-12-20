using MCRA.Simulation.Calculators.IntakeModelling.ModelThenAddIntakeModelCalculation;
using System.Collections.Generic;

namespace MCRA.Simulation.OutputGeneration {
    public abstract class UsualIntakeDistributionPerCategorySectionBase : SummarySection {

        public override bool SaveTemporaryData => true;

        public List<CategorizedIndividualExposure> IndividualExposuresByCategory { get; set; }

        public List<Category> Categories { get; set; }
    }
}
