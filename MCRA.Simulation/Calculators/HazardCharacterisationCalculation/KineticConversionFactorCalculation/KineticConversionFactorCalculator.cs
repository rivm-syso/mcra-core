using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.KineticModelCalculation;

namespace MCRA.Simulation.Calculators.HazardCharacterisationCalculation.KineticConversionFactorCalculation {
    public sealed class KineticConversionFactorCalculator : IKineticConversionFactorCalculator {

        private readonly KineticModelCalculatorFactory _kineticModelCalculatorFactory;
        private readonly Individual _nominalIndividual;

        public KineticConversionFactorCalculator(
            KineticModelCalculatorFactory kineticModelCalculatorFactory,
            double nominalBodyWeight
        ) {
            _kineticModelCalculatorFactory = kineticModelCalculatorFactory;
            _nominalIndividual = new Individual(0) {
                BodyWeight = nominalBodyWeight,
            };
        }

        public double ComputeKineticConversionFactor(
            double dose,
            TargetUnit hazardDoseUnit,
            Compound substance,
            ExposureType exposureType,
            TargetUnit targetUnit,
            IRandom generator
        ) {
            //External, reverse dosimetry, backwards
            if (targetUnit.TargetLevelType == TargetLevelType.External) {
                if (hazardDoseUnit.TargetLevelType == TargetLevelType.Internal) {
                    // Test system target level is internal
                    var kineticModelCalculator = _kineticModelCalculatorFactory
                        .CreateHumanKineticModelCalculator(substance);
                    var externalDose = kineticModelCalculator
                        .Reverse(
                            _nominalIndividual,
                            dose,
                            hazardDoseUnit,
                            ExposurePathType.Oral,
                            targetUnit.ExposureUnit,
                            exposureType,
                            generator
                        );
                    return externalDose / dose;
                } else {
                    // Test system is on the same level as the target system (i.e., external)
                    // Oral, Dermal, Inhalation
                    return 1;
                }
            } else {
                // Internal, forward dosimetry
                if (hazardDoseUnit.TargetLevelType == TargetLevelType.External) {
                    // Test system target level is external
                    var kineticModelCalculator = _kineticModelCalculatorFactory
                        .CreateHumanKineticModelCalculator(substance);
                    var doseAtTarget = kineticModelCalculator
                        .Forward(
                            _nominalIndividual,
                            dose,
                            hazardDoseUnit.Target.ExposureRoute.GetExposurePath(),
                            hazardDoseUnit.ExposureUnit,
                            targetUnit,
                            exposureType,
                            generator
                        );
                    return doseAtTarget / dose;
                } else if (hazardDoseUnit.TargetLevelType == TargetLevelType.Internal) {
                    // Test system target level is at the same level as the target (i.e., internal)
                    return 1;
                } else {
                    // Undefined
                    return double.NaN;
                }
            }
        }
    }
}
