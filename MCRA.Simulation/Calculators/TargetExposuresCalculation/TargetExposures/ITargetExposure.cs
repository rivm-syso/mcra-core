using MCRA.Data.Compiled.Objects;
using System.Collections.Generic;

namespace MCRA.Simulation.Calculators.TargetExposuresCalculation {
    public interface ITargetExposure {
        Individual Individual { get; }
        double CompartmentWeight { get; }
        double RelativeCompartmentWeight { get; }
        IDictionary<Compound, ISubstanceTargetExposure> TargetExposuresBySubstance { get; }
        double TotalAmountAtTarget(IDictionary<Compound, double> relativePotencyFactors, IDictionary<Compound, double> membershipProbabilities);
        double TotalConcentrationAtTarget(IDictionary<Compound, double> relativePotencyFactors, IDictionary<Compound, double> membershipProbabilities, bool isPerPerson);
        bool IsPositiveExposure();

        //IDictionary<Food, IIntakePerModelledFood> IntakesPerModelledFood(IDictionary<Compound, double> relativePotencyFactors, IDictionary<Compound, double> membershipProbabilities, bool isPerPerson);
    }
}
