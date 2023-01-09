using MCRA.General;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Calculators.KineticModelCalculation.ChlorpyrifosKineticModelCalculation;
using MCRA.Simulation.Calculators.KineticModelCalculation.PbpkModelCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Simulation.Test.UnitTests.Calculators.KineticModelCalculation {

    /// <summary>
    /// KineticModelCalculation calculator
    /// </summary>
    [TestClass]
    public class ChlorpyrifosKineticModelCalculatorTests {

        /// <summary>
        /// ChlorpyrifosKineticModelCalculator: calculates individual day target exposures, V1, acute
        /// </summary>
        [TestMethod]
        public void ChlorpyrifosKineticModelCalculator_TestCalculateIndividualDayTargetExposuresCPFV1() {
            int seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = MockSubstancesGenerator.Create(1);
            var substance = substances.First();
            var routes = new List<ExposureRouteType>() { ExposureRouteType.Dietary };
            var absorptionFactors = routes.ToDictionary(r => r, r => .1);
            var individuals = MockIndividualsGenerator.Create(2, 2, random, useSamplingWeights: true);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var individualDayExposures = MockExternalExposureGenerator.CreateExternalIndividualDayExposures(individualDays, substances, routes, seed);

            var instance = MockKineticModelsGenerator.CreateFakeChlorpyrifosKineticModelInstance(substance);
            instance.NumberOfDays = 10;
            instance.NumberOfDosesPerDay = 1;
            instance.CodeCompartment = "O_CVM1";
            instance.NonStationaryPeriod = 0;

            var model = new ChlorpyrifosKineticModelCalculator(instance, absorptionFactors);
            var nominalCompartmentWeight = model.GetNominalRelativeCompartmentWeight();
            var internalExposures = model.CalculateIndividualDayTargetExposures(
                individualDayExposures,
                substance,
                routes,
                TargetUnit.FromDoseUnit(DoseUnit.mgPerKg, instance.CodeCompartment),
                model.GetNominalRelativeCompartmentWeight(),
                new ProgressState(),
                random
            );

            Assert.AreEqual(4, internalExposures.Count);
        }

        /// <summary>
        /// ChlorpyrifosKineticModelCalculator: ; simulate as in Berkely Madonna
        /// GDOSE = 0.5                        	; Dose in mg/kg bw(given dose)
        /// ODOSE = GDOSE* 1e-3 / MWP*1e6   	; Dose in umol/kg bw(oral dose)
        /// DOSE = ODOSE* BW; Dose in umol
        /// BW = 70
        /// MWP = 350.59
        /// Note MCRA converts de output vector in de PBPK calculator with molecular weight and BW, but the ODE output is identical to R and C dll
        /// </summary>
        [TestMethod]
        public void ChlorpyrifosKineticModelCalculator_TestCalculateIndividualTargetExposuresCPFV1() {
            int seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = MockSubstancesGenerator.Create(1);
            var substance = substances.First();
            substance.MolecularMass = 350.59;
            var routes = new List<ExposureRouteType>() { ExposureRouteType.Dietary };
            var absorptionFactors = routes.ToDictionary(r => r, r => .1);
            var individuals = MockIndividualsGenerator.Create(1, 2, random, useSamplingWeights: false);
            var BW = 70d;
            foreach (var individual in individuals) {
                individual.BodyWeight = BW;
            }
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var individualExposures = MockExternalExposureGenerator.CreateExternalIndividualExposures(individualDays, substances, routes, seed);
            foreach (var item in individualExposures) {
                foreach (var exp in item.ExternalIndividualDayExposures) {
                    var result = new Dictionary<ExposureRouteType, ICollection<IIntakePerCompound>>();
                    var intakesPerCompound = new List<AggregateIntakePerCompound>();
                    intakesPerCompound.Add(new AggregateIntakePerCompound() {
                        Compound = substance,
                        Exposure = .5 * BW,
                    });
                    result[ExposureRouteType.Dietary] = intakesPerCompound.Cast<IIntakePerCompound>().ToList() as ICollection<IIntakePerCompound>;
                    exp.ExposuresPerRouteSubstance = result;
                }
            }

            var instance = MockKineticModelsGenerator.CreateFakeChlorpyrifosKineticModelInstance(substance);
            instance.NumberOfDays = 10;
            instance.NumberOfDosesPerDay = 1;
            instance.CodeCompartment = "O_ACLM2";
            instance.NonStationaryPeriod = 0;
            instance.SpecifyEvents = false;
            instance.SelectedEvents = new int[5] { 1, 2, 3, 4, 5 };

            var model = new ChlorpyrifosKineticModelCalculator(instance, absorptionFactors);
            var relativeCompartmentWeight = model.GetNominalRelativeCompartmentWeight();
            var internalExposures = model.CalculateIndividualTargetExposures(
                individualExposures,
                substance,
                routes,
                TargetUnit.FromDoseUnit(DoseUnit.mgPerKg, instance.CodeCompartment),
                relativeCompartmentWeight,
                new ProgressState(),
                random
            );
            var simulated = internalExposures[0].SubstanceTargetExposure as SubstanceTargetExposurePattern;
            var exposure = simulated.TargetExposuresPerTimeUnit[100].Exposure * 1000 / substance.MolecularMass;
            Assert.AreEqual(265.85, exposure, 1e-1);
        }
    }
}
