using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.HazardCharacterisationCalculation.KineticConversionFactorCalculation;
using System;
using System.Collections.Generic;

namespace MCRA.Simulation.Test.Mock.MockCalculators {
    class MockKineticConversionFactorCalculator : IKineticConversionFactorCalculator {

        private readonly double _absorptionFactor;
        private readonly Dictionary<Compound, double> _substanceAbsorptionFactors;

        public TargetLevelType TargetDoseLevel { get; private set; }

        public MockKineticConversionFactorCalculator(
            TargetLevelType targetDoseLevel,
            double defaultAbsorptionFactor,
            Dictionary<Compound, double> substanceAbsorptionFactors = null
        ) {
            TargetDoseLevel = targetDoseLevel;
            _absorptionFactor = defaultAbsorptionFactor;
            _substanceAbsorptionFactors = substanceAbsorptionFactors;
        }

        public double ComputeKineticConversionFactor(
            double testSystemHazardDose,
            TargetUnit doseUnit,
            Compound substance,
            string testSystemSpecies,
            string testSystemOrgan,
            ExposureRouteType testSystemExposureRoute,
            ExposureType exposureType,
            IRandom random
        ) {
            return compute(substance, testSystemExposureRoute, TargetDoseLevel);
        }

        public double ComputeKineticConversionFactor(
            double internalHazardDose,
            TargetUnit intakeUnit,
            Compound substance,
            string testSystemSpecies,
            string testSystemOrgan,
            ExposureRouteType testSystemExposureRoute,
            ExposureType exposureType,
            TargetLevelType targetDoseLevelType,
            IRandom random
        ) {
            return compute(substance, testSystemExposureRoute, targetDoseLevelType);
        }

        private double compute(
            Compound substance,
            ExposureRouteType testSystemExposureRoute,
            TargetLevelType targetDoseLevelType
        ) {
            double absorptionFactor = _absorptionFactor;
            _substanceAbsorptionFactors?.TryGetValue(substance, out absorptionFactor);

            if (targetDoseLevelType == TargetLevelType.External) {
                if (testSystemExposureRoute == ExposureRouteType.AtTarget) {
                    return 1 / absorptionFactor;
                } else {
                    return 1;
                }
            } else if (targetDoseLevelType == TargetLevelType.Internal) {
                if (testSystemExposureRoute == ExposureRouteType.AtTarget) {
                    return 1;
                } else {
                    return absorptionFactor;
                }
            } else {
                throw new Exception("Unknown target dose level.");
            }
        }
    }
}
