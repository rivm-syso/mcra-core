using MCRA.Simulation.Objects;

namespace MCRA.Simulation.Calculators.PbkModelCalculation {

    public class PbkSimulationOutput {

        public double AgeStart { get; set; }

        public double AgeEnd { get; set; }

        public List<(double Time, double BodyWeight)> BodyWeightTimeSeries { get; set; }

        public SimulatedIndividual SimulatedIndividual { get; set; }

        public List<SubstanceTargetExposureTimeSeries> SubstanceTargetLevelTimeSeries { get; set; }

    }
}
