using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.KineticConversionFactorModels;
using MCRA.Simulation.Calculators.KineticConversionCalculation;
using MCRA.Simulation.Calculators.PbpkModelCalculation;
using MCRA.Simulation.Objects;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation.KineticConversions {

    /// <summary>
    /// Calculator class for computing biological matrix concentrations
    /// for a target matrix based on a concentration value of a sampling
    /// method of another biological matrix using a simple conversion factor
    /// that is the same for all biological matrices other than the target
    /// biological matrix.
    /// </summary>
    public class TargetMatrixKineticConversionCalculator {

        /// <summary>
        /// Dictionary with relevant kinetic conversion factor models
        /// </summary>
        private readonly ILookup<(Compound, ExposureTarget, ExposureTarget), IKineticConversionFactorModel> _kineticConversionModels;

        /// <summary>
        /// Dictionary with relevant PBK models
        /// </summary>
        private readonly Dictionary<Compound, IKineticConversionCalculator> _kineticModelCalculators;

        /// <summary>
        /// Default (intermediate) external exposure route for reverse PBK model calculations
        /// translating one internal target concentration to another internal target concentration.
        /// </summary>
        private readonly ExposureRoute _reverseDoseDefaultExposureRoute;

        /// <summary>
        /// Creates a new instance of a <see cref="TargetMatrixKineticConversionCalculator"/>.
        /// Select only the conversion records for the given target biological matrix
        /// </summary>
        public TargetMatrixKineticConversionCalculator(
            ICollection<IKineticConversionFactorModel> kineticConversionFactors,
            ICollection<KineticModelInstance> kineticModelInstances = null,
            PbkSimulationSettings pbkSimulationSettings = null
        ) {
            _reverseDoseDefaultExposureRoute = ExposureRoute.Oral;
            _kineticConversionModels = kineticConversionFactors?
                .ToLookup(c => (
                    c.SubstanceFrom,
                    c.TargetFrom,
                    c.TargetTo
                ));
            var kmcFactory = new KineticConversionCalculatorFactory(kineticModelInstances);
            _kineticModelCalculators = kineticModelInstances?
                .ToDictionary(
                    c => c.InputSubstance,
                    c => kmcFactory.CreateHumanKineticModelCalculator(c.InputSubstance, pbkSimulationSettings)
                );
        }

        public ICollection<HbmSubstanceTargetExposure> GetSubstanceTargetExposures(
            HbmSubstanceTargetExposure sourceExposure,
            SimulatedIndividualDay individualDay,
            TargetUnit sourceUnit,
            ExposureType exposureType,
            TargetUnit targetUnit,
            double compartmentWeight,
            IRandom kineticModelParametersRandomGenerator,
            bool expectCompleteConversion
        ) {
            var substance = sourceExposure.Substance;
            var result = new List<HbmSubstanceTargetExposure>();
            if (_kineticModelCalculators?.TryGetValue(substance, out var instance) ?? false) {
                var resultRecord = new HbmSubstanceTargetExposure() {
                    SourceSamplingMethods = sourceExposure.SourceSamplingMethods,
                    Exposure = convertPbkModel(
                        individualDay.SimulatedIndividual,
                        sourceExposure.Exposure,
                        sourceUnit,
                        exposureType,
                        substance,
                        targetUnit,
                        instance as PbkKineticConversionCalculator,
                        kineticModelParametersRandomGenerator
                    ),
                    IsAggregateOfMultipleSamplingMethods = sourceExposure.IsAggregateOfMultipleSamplingMethods,
                    Substance = substance
                };
                result.Add(resultRecord);
            } else if (_kineticConversionModels?.Contains((substance, sourceUnit.Target, targetUnit.Target)) ?? false) {
                var conversions = _kineticConversionModels[(substance, sourceUnit.Target, targetUnit.Target)];
                var resultRecords = conversions
                    .Select(c => new HbmSubstanceTargetExposure() {
                        SourceSamplingMethods = sourceExposure.SourceSamplingMethods,
                        Exposure = convertConversionFactorModel(
                            individualDay.SimulatedIndividual,
                            sourceExposure.Exposure,
                            sourceUnit,
                            targetUnit,
                            c,
                            compartmentWeight
                        ),
                        IsAggregateOfMultipleSamplingMethods = sourceExposure.IsAggregateOfMultipleSamplingMethods,
                        Substance = c.SubstanceTo
                    })
                    .ToList();
                result.AddRange(resultRecords);
            } else if (expectCompleteConversion) {
                var msg = $"For substance {substance.Name}, code {substance.Code} no kinetic conversion factor or PBK model to convert matrix is found.";
                throw new Exception(msg);
            }
            return result;
        }

        /// <summary>
        /// Kinetic conversion based on PBK model.
        /// </summary>
        /// <returns></returns>
        private double convertPbkModel(
            SimulatedIndividual individual,
            double concentration,
            TargetUnit sourceUnit,
            ExposureType exposureType,
            Compound substance,
            TargetUnit targetUnit,
            PbkKineticConversionCalculator kineticModelCalculator,
            IRandom kineticModelParametersRandomGenerator
        ) {
            // Determine (intermediate) external unit
            var externalTargetUnit = (targetUnit.TargetLevelType == TargetLevelType.External)
                ? targetUnit
                : TargetUnit.FromExternalDoseUnit(DoseUnit.ugPerKgBWPerDay, _reverseDoseDefaultExposureRoute);

            // Compute external dose leading to source concentration
            var externalDose = kineticModelCalculator
                .Reverse(
                    individual,
                    concentration,
                    sourceUnit,
                    externalTargetUnit.ExposureRoute,
                    externalTargetUnit.ExposureUnit,
                    exposureType,
                    kineticModelParametersRandomGenerator
                );
            if (externalTargetUnit == targetUnit) {
                // Target is external -> return result
                return externalDose;
            } else {
                // Compute target exposure based on the found external dose
                var resultForward = kineticModelCalculator
                    .Forward(
                        individual,
                        externalDose,
                        externalTargetUnit.ExposureRoute,
                        externalTargetUnit.ExposureUnit,
                        targetUnit,
                        exposureType,
                        kineticModelParametersRandomGenerator
                    );
                return resultForward;
            }
        }

        private double convertConversionFactorModel(
           SimulatedIndividual individual,
           double concentration,
           TargetUnit sourceExposureUnit,
           TargetUnit targetUnit,
           IKineticConversionFactorModel record,
           double compartmentWeight
       ) {
            // Alignment factor for source-unit of concentration with from-unit of conversion record
            var sourceUnitAlignmentFactor = sourceExposureUnit.ExposureUnit.GetAlignmentFactor(
                record.UnitFrom,
                record.SubstanceFrom.MolecularMass,
                double.NaN
            );

            // Get conversion factor
            var age = individual.Age;
            var genderType = individual.Gender;
            var conversionFactor = record.GetConversionFactor(age, genderType);

            // Alignment factor for to-unit of the conversion record with the target unit
            var targetUnitAlignmentFactor = record.UnitTo.GetAlignmentFactor(
                targetUnit.ExposureUnit,
                record.SubstanceTo.MolecularMass,
                compartmentWeight
            );

            // The factor belongs to the combination of dose-unit-from and dose-unit-to.
            // Compute the result by aligning the (source) concentration with the dose-unit-from,
            // applying the conversion factor, and then aligning the result with the alignment
            // factor of the dose-unit-to with the target unit.
            var result = concentration * sourceUnitAlignmentFactor * conversionFactor * targetUnitAlignmentFactor;
            return result;
        }
    }
}
