using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Simulation.Objects {

    /// <summary>
    /// Stores info about individual, the day and the simulated ID
    /// </summary>
    /// <param name="individual">
    /// The source individual that is used for simulation
    /// </param>
    public sealed class SimulatedIndividual(Individual individual, int id) {
        public Individual Individual => individual;

        /// <summary>
        /// The simulation id for this simulated individual
        /// </summary>
        public int Id { get; } = id;

        public string Code => individual.Code;

        /// <summary>
        /// The sampling weight of the simulated individual.
        /// </summary>
        public double SamplingWeight { get; set; } = individual.SamplingWeight;

        /// <summary>
        /// The body weight of the simulated individual.
        /// </summary>
        public double BodyWeight { get; set; } = individual.BodyWeight;

        public double? Age { get; set; } = individual.Age;

        public double? Height  => individual.Height;

        public double? BodySurfaceArea => individual.BodySurfaceArea;

        public GenderType Gender { get; set; } = individual.Gender;

        public int NumberOfDaysInSurvey => individual.NumberOfDaysInSurvey;

        public bool MissingBodyWeight => double.IsNaN(individual.BodyWeight);

        public double Covariable => individual.Covariable;

        public string Cofactor => individual.Cofactor;

        public OccupationalScenario OccupationalScenario { get; set; }

        public Dictionary<IndividualProperty, IndividualPropertyValue> IndividualProperties { get; set; }
            = individual.IndividualPropertyValues.ToDictionary(r => r.IndividualProperty);

        public SimulatedIndividual Clone() {
            return new SimulatedIndividual(individual, Id) {
                SamplingWeight = SamplingWeight,
                Age = Age,
                BodyWeight = BodyWeight,
                Gender = Gender,
                OccupationalScenario = OccupationalScenario,
                IndividualProperties = IndividualProperties.Values
                    .ToDictionary(r => r.IndividualProperty)
            };
        }

        public override string ToString() {
            return $"[{GetHashCode():X8}] {Id:000} {Code} (Indiv: {Individual})";
        }
    }
}
