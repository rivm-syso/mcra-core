using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.HazardCharacterisationCalculation.KineticConversionFactorCalculation;
using MCRA.Simulation.Calculators.KineticConversionFactorModels;

namespace MCRA.Simulation.Test.Mock.MockCalculators {
    class MockKineticConversionFactorCalculator : IKineticConversionFactorCalculator {

        private readonly double _absorptionFactor;
        private readonly Dictionary<Compound, double> _substanceAbsorptionFactors;
        private readonly List<IKineticConversionFactorModel> _kineticConversionFactors;

        public MockKineticConversionFactorCalculator(
            double defaultAbsorptionFactor = 1D,
            Dictionary<Compound, double> substanceAbsorptionFactors = null,
            List<KineticConversionFactor> kineticConversionFactors = null
        ) {
            _absorptionFactor = defaultAbsorptionFactor;
            _substanceAbsorptionFactors = substanceAbsorptionFactors;
            _kineticConversionFactors = kineticConversionFactors?
                .Select(r => KineticConversionFactorCalculatorFactory.Create(r, false))
                .ToList();
        }

        public double ComputeKineticConversionFactor(
            double dose,
            TargetUnit hazardDoseUnit,
            Compound substance,
            ExposureType exposureType,
            TargetUnit targetUnit,
            IRandom generator
        ) {
            var substanceAmountConversionFactor = hazardDoseUnit.SubstanceAmountUnit
                .GetMultiplicationFactor(targetUnit.SubstanceAmountUnit);
            var concentrationMassUnitAlignmentFactor = hazardDoseUnit.ConcentrationMassUnit
                .GetMultiplicationFactor(targetUnit.ConcentrationMassUnit, double.NaN);

            if (hazardDoseUnit.Target == targetUnit.Target) {
                // No kinetic conversion needed (only unit alignment)
                return concentrationMassUnitAlignmentFactor
                    * substanceAmountConversionFactor;
            }
            if (targetUnit.TargetLevelType == TargetLevelType.External) {
                if (hazardDoseUnit.Target.TargetLevelType == TargetLevelType.Internal) {
                    // Absorption factor (reverse)
                    var absorptionFactor = getAbsorptionFactor(substance);
                    return concentrationMassUnitAlignmentFactor
                        * substanceAmountConversionFactor
                        * (1D / absorptionFactor);
                } else {
                    return concentrationMassUnitAlignmentFactor * substanceAmountConversionFactor;
                }
            } else if (targetUnit.TargetLevelType == TargetLevelType.Internal) {
                if (hazardDoseUnit.TargetLevelType == TargetLevelType.External) {
                    // Absorption factor (forward)
                    var absorptionFactor = getAbsorptionFactor(substance);
                    return concentrationMassUnitAlignmentFactor
                        * substanceAmountConversionFactor
                        * absorptionFactor;
                } else {
                    return concentrationMassUnitAlignmentFactor * substanceAmountConversionFactor;
                }
            } else {
                throw new Exception("Unknown target dose level.");
            }
        }

        private double getAbsorptionFactor(Compound substance) {
            return _substanceAbsorptionFactors != null
                && _substanceAbsorptionFactors.ContainsKey(substance)
                ? _substanceAbsorptionFactors[substance]
                : _absorptionFactor;
        }
    }
}
