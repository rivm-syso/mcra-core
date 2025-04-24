using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.DietaryExposureCalculation.IndividualDietaryExposureCalculation;

namespace MCRA.Simulation.Calculators.IntakeModelling.ModelThenAddIntakeModelCalculation {

    /// <summary>
    /// A partial intake model of model-then-add.
    /// </summary>
    public sealed class ModelThenAddPartialIntakeModel {

        /// <summary>
        /// The index of this partial model.
        /// </summary>
        public int ModelIndex { get; set; }

        /// <summary>
        /// The modelled foods sub-category.
        /// </summary>
        public ICollection<Food> FoodsAsMeasured { get; set; }

        /// <summary>
        /// The fitted model for this category.
        /// </summary>
        public IntakeModel IntakeModel { get; set; }

        /// <summary>
        /// The individual usual exposures modelled by this model.
        /// </summary>
        public List<ModelAssistedIntake> MTAModelAssistedIntakes { get; set; }

        /// <summary>
        /// The OIMs used for modelling.
        /// </summary>
        public List<DietaryIndividualIntake> IndividualIntakes { get; set; }

    }
}
