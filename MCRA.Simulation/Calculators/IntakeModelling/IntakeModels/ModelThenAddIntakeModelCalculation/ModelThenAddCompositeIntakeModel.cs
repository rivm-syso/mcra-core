using System.Collections.Generic;

namespace MCRA.Simulation.Calculators.IntakeModelling.ModelThenAddIntakeModelCalculation {
    public class CompositeIntakeModel : IIntakeModel {

        /// <summary>
        /// The sub-models that make up this composite model.
        /// </summary>
        public List<ModelThenAddPartialIntakeModel> PartialModels { get; set; }

    }
}
