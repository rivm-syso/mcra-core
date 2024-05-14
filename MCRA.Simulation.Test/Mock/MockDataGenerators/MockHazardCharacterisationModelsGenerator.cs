using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.HazardCharacterisationCalculation;

namespace MCRA.Simulation.Test.Mock.MockDataGenerators {

    /// <summary>
    /// Class for generating mock hazard characterisation models.
    /// </summary>
    public static class MockHazardCharacterisationModelsGenerator {

        /// <summary>
        /// Creates a hazard characterisation model collection with one single item.
        /// </summary>
        public static ICollection<HazardCharacterisationModelCompoundsCollection> CreateSingle(
            Effect effect,
            ICollection<Compound> substances,
            TargetUnit targetUnit,
            double interSystemConversionFactor = 1,
            double intraSystemConversionFactor = 1,
            double kineticConversionFactor = 1,
            ExposurePathType exposureRoute = ExposurePathType.Oral,
            int seed = 1,
            bool ageDependent = false
        ) {
            var random = new McraRandomGenerator(seed);
            var target = new ExposureTarget(exposureRoute.GetExposureRoute());
            var result = substances.ToDictionary(
                s => s,
                s => {
                    var dose = LogNormalDistribution.Draw(random, 2, 1);
                    return CreateSingle(
                        effect,
                        s,
                        dose,
                        targetUnit,
                        interSystemConversionFactor,
                        intraSystemConversionFactor,
                        kineticConversionFactor,
                        ageDependent
                    );
                }
            );

            return new List<HazardCharacterisationModelCompoundsCollection> {
                new HazardCharacterisationModelCompoundsCollection {
                     TargetUnit = targetUnit,
                     HazardCharacterisationModels = result
                }
            };
        }

        /// <summary>
        /// Creates a dictionary of target hazard dose model for each substance
        /// </summary>
        /// <param name="effect"></param>
        /// <param name="substances"></param>
        /// <param name="interSystemConversionFactor"></param>
        /// <param name="intraSystemConversionFactor"></param>
        /// <param name="kineticConversionFactor"></param>
        /// <param name="exposureRoute"></param>
        /// <param name="seed"></param>
        /// <returns></returns>
        public static IDictionary<Compound, IHazardCharacterisationModel> Create(
            Effect effect,
            ICollection<Compound> substances,
            double interSystemConversionFactor = 1,
            double intraSystemConversionFactor = 1,
            double kineticConversionFactor = 1,
            ExposureRoute exposureRoute = ExposureRoute.Oral,
            int seed = 1,
            bool ageDependent = false
        ) {
            var random = new McraRandomGenerator(seed);
            var target = new ExposureTarget(exposureRoute);
            var exposureUnit = new ExposureUnitTriple(
                SubstanceAmountUnit.Milligrams,
                ConcentrationMassUnit.Kilograms,
                TimeScaleUnit.PerDay
            );
            var result = substances.ToDictionary(
                s => s,
                s => {
                    var dose = LogNormalDistribution.Draw(random, 2, 1);
                    return CreateSingle(
                        effect,
                        s,
                        dose,
                        new TargetUnit(target, exposureUnit),
                        interSystemConversionFactor,
                        intraSystemConversionFactor,
                        kineticConversionFactor,
                        ageDependent
                    );
                }
            );
            return result;
        }

        /// <summary>
        /// Creates a random target hazard dose model for the specified substance.
        /// </summary>
        /// <param name="effect"></param>
        /// <param name="substance"></param>
        /// <param name="value"></param>
        /// <param name="target"></param>
        /// <param name="unit"></param>
        /// <param name="interSpeciesFactor"></param>
        /// <param name="intraSpeciesFactor"></param>
        /// <param name="kineticConversionFactor"></param>
        /// <param name="ageDependent"></param>
        /// <returns></returns>
        public static IHazardCharacterisationModel CreateSingle(
            Effect effect,
            Compound substance,
            double value,
            TargetUnit targetUnit,
            double interSpeciesFactor = 1,
            double intraSpeciesFactor = 1,
            double kineticConversionFactor = 1,
            bool ageDependent = false
        ) {
            var ages = new double[] { 0, 10, 20, 30, 40, 50, 60, 70, 80 };
            return new HazardCharacterisationModel() {
                Effect = effect,
                Substance = substance,
                TargetUnit = targetUnit,
                TestSystemHazardCharacterisation = new TestSystemHazardCharacterisation() {
                    HazardDose = value * interSpeciesFactor * intraSpeciesFactor,
                    InterSystemConversionFactor = 1D / interSpeciesFactor,
                    IntraSystemConversionFactor = 1D / intraSpeciesFactor,
                    KineticConversionFactor = kineticConversionFactor,
                },
                Value = value,
                HCSubgroups = ageDependent
                    ? ages
                        .Select(r => new HCSubgroup() {
                            AgeLower = r,
                            Value = 0.5 * value + 0.5 * (r / ages.Max()) * value
                        })
                        .ToList()
                    : null
            };
        }
    }
}
