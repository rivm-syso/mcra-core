using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.HazardCharacterisationCalculation.HazardCharacterisationsFromIviveCalculation;
using MCRA.Simulation.Calculators.KineticModelCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.AggregateExposures;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.HazardCharacterisationCalculation.HazardCharacterisationTimeCourseCalculation {
    public class HazardDosePbkTimeCourseCalculator {

        private readonly double _nominalBodyWeight;

        public HazardDosePbkTimeCourseCalculator(double nominalBodyWeight) {
            _nominalBodyWeight = nominalBodyWeight;
        }

        public List<HazardDosePbkTimeCourse> Compute(
            ICollection<IHazardCharacterisationModel> hazardCharacterisationModels,
            ExposureType exposureType,
            KineticModelCalculatorFactory kineticModelCalculatorFactory,
            TargetUnit targetDoseUnit,
            IRandom kineticModelRandomGenerator
        ) {
            var result = new List<HazardDosePbkTimeCourse>();
            foreach (var model in hazardCharacterisationModels) {
                var kineticModelCalculator = kineticModelCalculatorFactory?
                    .CreateHumanKineticModelCalculator(model.Substance);

                if (kineticModelCalculator == null) {
                    continue;
                }

                var individual = new Individual(0) {
                    BodyWeight = _nominalBodyWeight,
                };
                double externalDose;
                TargetUnit internalDoseUnit;
                TargetUnit externalDoseUnit;

                // TODO: this will only summarize the time courses for the scenarios in which the test system HC is
                // internal and the HC is external. It does not cover:
                // - the scenario in which the test system is external and the HC is internal;
                if (model.TestSystemHazardCharacterisation?.Organ != null) {
                    // Use the target dose, without intra-species factor and kinetic conversion factor
                    externalDose = model.Value / model.TestSystemHazardCharacterisation.IntraSystemConversionFactor;
                    externalDoseUnit = targetDoseUnit;
                    internalDoseUnit = TargetUnit.FromInternalDoseUnit(
                        model.TestSystemHazardCharacterisation.DoseUnit,
                        BiologicalMatrixConverter.FromString(model.TestSystemHazardCharacterisation.Organ)
                    );
                } else if (model is IviveHazardCharacterisation) {
                    var iviveRecord = model as IviveHazardCharacterisation;
                    // Use the target dose, without intra-species factor and kinetic conversion factor
                    externalDose = model.Value / iviveRecord.NominalIntraSpeciesConversionFactor;
                    externalDoseUnit = model.TargetUnit;
                    internalDoseUnit = iviveRecord.InternalTargetUnit;
                } else {
                    // TODO: time course drilldown not implemented for this scenario
                    continue;
                }

                var exposure = ExternalIndividualDayExposure
                    .FromSingleDose(
                        externalDoseUnit.ExposureRoute.GetExposurePath(),
                        model.Substance,
                        externalDose,
                        externalDoseUnit.ExposureUnit,
                        individual
                    );

                var substanceTargetExposure = kineticModelCalculator
                    .Forward(
                        exposure,
                        externalDoseUnit.ExposureRoute.GetExposurePath(),
                        externalDoseUnit.ExposureUnit,
                        internalDoseUnit,
                        exposureType,
                        kineticModelRandomGenerator
                    );

                var aggregateIndividualExposure = new AggregateIndividualExposure() {
                    Individual = individual,
                    IndividualSamplingWeight = 1D,
                    InternalTargetExposures = new Dictionary<ExposureTarget, Dictionary<Compound, ISubstanceTargetExposure>>() {
                            {
                                internalDoseUnit.Target,
                                new Dictionary<Compound, ISubstanceTargetExposure>() {
                                    { model.Substance, substanceTargetExposure }
                                }
                            }
                        },
                    ExternalIndividualDayExposures = [exposure]
                };
                var record = new HazardDosePbkTimeCourse() {
                    AggregateIndividualExposure = aggregateIndividualExposure,
                    HazardCharacterisation = model,
                    InternalTargetUnit = internalDoseUnit,
                    ExternalTargetUnit = externalDoseUnit
                };
                result.Add(record);
            }
            return result;
        }
    }
}
