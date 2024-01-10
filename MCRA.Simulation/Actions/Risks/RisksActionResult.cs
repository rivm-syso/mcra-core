using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Action;
using MCRA.Simulation.Action.UncertaintyFactorial;
using MCRA.Simulation.Calculators.ComponentCalculation.DriverSubstanceCalculation;
using MCRA.Simulation.Calculators.ComponentCalculation.ExposureMatrixCalculation;
using MCRA.Simulation.Calculators.HazardCharacterisationCalculation;
using MCRA.Simulation.Calculators.RiskCalculation;
using MCRA.Simulation.Calculators.SingleValueRisksCalculation;

namespace MCRA.Simulation.Actions.Risks {
    public class RisksActionResult : IActionResult {

        public ICollection<ExposureTarget> ExposureTargets { get; set; }

        public List<TargetUnit> TargetUnits { get; set; }

        public IHazardCharacterisationModel ReferenceDose { get; set; }

        public List<IndividualEffect> IndividualRisks { get; set; }

        public List<(ExposureTarget Target, Dictionary<Compound, List<IndividualEffect>> IndividualEffects)> IndividualEffectsBySubstanceCollections { get; set; }

        public Dictionary<Food, List<IndividualEffect>> IndividualEffectsByModelledFood { get; set; }

        public IDictionary<(Food, Compound), List<IndividualEffect>> IndividualEffectsByModelledFoodSubstance { get; set; }

        public ICollection<RiskDistributionPercentileRecord> RiskPercentiles { get; set; }

        public IUncertaintyFactorialResult FactorialResult { get; set; }

        public List<DriverSubstance> DriverSubstances { get; set; }

        public ExposureMatrix RiskMatrix { get; set; }
    }
}
