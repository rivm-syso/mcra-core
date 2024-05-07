using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Calculators.KineticModelCalculation.DesolvePbkModelCalculators.ChlorpyrifosKineticModelCalculation;
using MCRA.Simulation.Calculators.KineticModelCalculation.PbpkModelCalculation;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
            var substances = MockSubstancesGenerator.Create(3);
            var substance = substances.First();
            var routes = new List<ExposurePathType>() { ExposurePathType.Dietary };
            var absorptionFactors = routes.ToDictionary(r => r, r => .1);
            var individuals = MockIndividualsGenerator.Create(2, 2, random, useSamplingWeights: true);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var individualDayExposures = MockExternalExposureGenerator.CreateExternalIndividualDayExposures(individualDays, substances, routes, seed);

            var instance = MockKineticModelsGenerator
                .CreateFakeChlorpyrifosKineticModelInstance("FakeCPFInstance", substances);
            instance.NumberOfDays = 10;
            instance.NumberOfDosesPerDay = 1;
            instance.CompartmentCodes = new List<string> { "O_CV" };
            instance.NonStationaryPeriod = 0;

            var model = new ChlorpyrifosKineticModelCalculator(instance, absorptionFactors);
            var internalExposures = model.CalculateIndividualDayTargetExposures(
                individualDayExposures,
                routes,
                ExposureUnitTriple.FromDoseUnit(DoseUnit.mgPerKg),
                new List<TargetUnit> { TargetUnit.FromInternalDoseUnit(DoseUnit.ugPerL) },
                new ProgressState(),
                random
            );

            Assert.AreEqual(4, internalExposures.First().IndividualDaySubstanceTargetExposures.Count);
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
        public void ChlorpyrifosKineticModelCalculator_TestCalculateIndividualTargetExposures_ACLM2() {
            int seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = MockSubstancesGenerator.Create(3);
            var substance = substances.First();
            substance.MolecularMass = 350.59;
            var routes = new List<ExposurePathType>() { ExposurePathType.Dietary };
            var absorptionFactors = routes.ToDictionary(r => r, r => .1);
            var individuals = MockIndividualsGenerator.Create(1, 2, random, useSamplingWeights: false);
            var bodyWeight = 70d;
            foreach (var individual in individuals) {
                individual.BodyWeight = bodyWeight;
            }
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var individualExposures = MockExternalExposureGenerator.CreateExternalIndividualExposures(individualDays, substances, routes, seed);
            foreach (var item in individualExposures) {
                foreach (var exp in item.ExternalIndividualDayExposures) {
                    var result = new Dictionary<ExposurePathType, ICollection<IIntakePerCompound>>();
                    var intakesPerCompound = new List<AggregateIntakePerCompound> {
                        new AggregateIntakePerCompound() {
                            Compound = substance,
                            Amount = .5 * bodyWeight,
                        }
                    };
                    result[ExposurePathType.Dietary] = intakesPerCompound
                        .Cast<IIntakePerCompound>()
                        .ToList();
                    exp.ExposuresPerRouteSubstance = result;
                }
            }

            var instance = MockKineticModelsGenerator
                .CreateFakeChlorpyrifosKineticModelInstance("FakeCPFInstance", substances);
            instance.NumberOfDays = 10;
            instance.NumberOfDosesPerDay = 1;
            instance.CompartmentCodes = new List<string> { "O_ACL" };
            instance.NonStationaryPeriod = 0;
            instance.SpecifyEvents = false;
            instance.SelectedEvents = new int[5] { 1, 2, 3, 4, 5 };

            var model = new ChlorpyrifosKineticModelCalculator(instance, absorptionFactors);
            var internalExposures = model.CalculateIndividualTargetExposures(
                individualExposures,
                routes,
                ExposureUnitTriple.FromDoseUnit(DoseUnit.mgPerKg),
                new List<TargetUnit> { TargetUnit.FromInternalDoseUnit(DoseUnit.ugPerL) },
                new ProgressState(),
                random
            );
            var simulated = internalExposures[0]
                .IndividualSubstanceTargetExposures
                .SelectMany(r => r.SubstanceTargetExposures)
                .Select(c => (SubstanceTargetExposurePattern)c)
                .ToList();
            // This unit test only rubs as long as there are 9 outputs defined in the xml
            var exposure = simulated.Last().TargetExposuresPerTimeUnit[100].Exposure * 1000 / substance.MolecularMass;
            Assert.AreEqual(107.00, exposure, 1e-1);
        }

        /// <summary>
        /// Compartment is Id output in unittest
        /// </summary>
        [TestMethod]
        public void ChlorpyrifosKineticModelCalculator_TestCalculateIndividualTargetExposures_CV() {
            int seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = MockSubstancesGenerator.Create(3);
            var parentSubstance = substances.First();
            var metabolites = substances.Skip(1).ToList();
            parentSubstance.MolecularMass = 350.59;
            var routes = new List<ExposurePathType>() { ExposurePathType.Dietary };
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
                    var result = new Dictionary<ExposurePathType, ICollection<IIntakePerCompound>>();
                    var intakesPerCompound = new List<AggregateIntakePerCompound> {
                        new AggregateIntakePerCompound() {
                            Compound = parentSubstance,
                            Amount = .5 * BW,
                        }
                    };
                    result[ExposurePathType.Dietary] = intakesPerCompound.Cast<IIntakePerCompound>().ToList();
                    exp.ExposuresPerRouteSubstance = result;
                }
            }
            var kineticModelDefinition = MCRAKineticModelDefinitions.Definitions["PBK_Chlorpyrifos_V1"];
            var instance = MockKineticModelsGenerator
                .CreateFakeChlorpyrifosKineticModelInstance("FakeCPFInstance", substances);
            instance.NumberOfDays = 10;
            instance.NumberOfDosesPerDay = 1;
            instance.CompartmentCodes = new List<string> { "O_CV" };
            instance.NonStationaryPeriod = 0;
            instance.SpecifyEvents = false;
            instance.SelectedEvents = new int[5] { 1, 2, 3, 4, 5 };

            var model = new ChlorpyrifosKineticModelCalculator(instance, absorptionFactors);
            var internalExposures = model.CalculateIndividualTargetExposures(
                individualExposures,
                routes,
                ExposureUnitTriple.FromDoseUnit(DoseUnit.mgPerKg),
                new List<TargetUnit> { TargetUnit.FromInternalDoseUnit(DoseUnit.ugPerL) },
                new ProgressState(),
                random
            );
            var simulated = internalExposures[0]
                .IndividualSubstanceTargetExposures
                .SelectMany(r => r.SubstanceTargetExposures)
                .Select(c => (SubstanceTargetExposurePattern)c)
                .ToList();
            // This unit test only runs as long as there are 9 outputs defined in the xml
            var exposure = simulated.Single(c => c.Substance == parentSubstance).TargetExposuresPerTimeUnit[100].Exposure * 1000 / parentSubstance.MolecularMass;
            Assert.AreEqual(0.011, exposure, 1e-1);
        }
        
        /// <summary>
        /// Test whether model contains metabolites or not
        /// </summary>
        [TestMethod]
        public void ChlorpyrifosKineticModelCalculator_TestMetabolitesOrNot() {
            var instances = getInstance();
            // 0, 1 => PBKA => 0, 1, 2
            Assert.IsTrue(instances[0].HasMetabolites());
            // 3 => PBKB => 3
            Assert.IsFalse(instances[1].HasMetabolites());
            // 4, 5 => PBKC => 4, 5
            Assert.IsFalse(instances[2].HasMetabolites());
            // 6 => PBKD => 6, 7, 0
            Assert.IsTrue(instances[3].HasMetabolites());
            // 8 => PKBE => 9
            Assert.IsTrue(instances[4].HasMetabolites());
            // 1 => PKBF => 1
            Assert.IsFalse(instances[5].HasMetabolites());
            // 1 => PBKG => 10, 11
            Assert.IsTrue(instances[6].HasMetabolites());
        }

        /// <summary>
        /// OK
        /// (0): 0, 1 => PBKA => 0, 1, 2
        /// (1): 3 => PBKB => 3
        /// (2): 4, 5 => PBKC => 4, 5
        /// (4): 8 => PKBE => 9
        /// </summary>
        [TestMethod]
        public void ChlorpyrifosKineticModelCalculator_TestValidOrNot1() {
            var instances = getInstance();
            var selectedInstances = new List<KineticModelInstance>(){
                instances[0],
                instances[1],
                instances[2],
                instances[4]
            };
            var warning = checkPBKmodels(selectedInstances);
            Assert.AreEqual(0, warning.Count);
        }

        /// <summary>
        /// Not OK
        /// (0): 0, 1 => PBKA => 0, 1, 2
        /// (5): 1 => PKBF => 1
        /// </summary>
        [TestMethod]
        public void ChlorpyrifosKineticModelCalculator_TestValidOrNot2() {
            var instances = getInstance();
            var selectedInstances = new List<KineticModelInstance>() {
                instances[0],
                instances[5]
            };
            var warning = checkPBKmodels(selectedInstances);
            Assert.AreEqual(2, warning.Count);
        }

        /// <summary>
        /// Not OK
        /// (0): 0, 1 => PBKA => 0, 1, 2
        /// (6): 1 => PBKG => 10, 11
        /// </summary>
        [TestMethod]
        public void ChlorpyrifosKineticModelCalculator_TestValidOrNot3() {
            var instances = getInstance();
            var selectedInstances = new List<KineticModelInstance>() {
                instances[0],
                instances[6]
            };
            var warning = checkPBKmodels(selectedInstances);
            Assert.AreEqual(2, warning.Count);
        }

        /// <summary>
        /// Not OK
        /// (0): 0, 1 => PBKA => 0, 1, 2
        /// (5): 1 => PKBF => 1
        /// (6): 1 => PBKG => 10, 11
        /// </summary>
        [TestMethod]
        public void ChlorpyrifosKineticModelCalculator_TestValidOrNot4() {
            var instances = getInstance();
            var selectedInstances = new List<KineticModelInstance>() {
                instances[0],
                instances[5],
                instances[6]
            };
            var warning = checkPBKmodels(selectedInstances);
            Assert.AreEqual(2, warning.Count);
        }
        /// <summary>
        /// Not OK, this one should be possible when 1 is not metabolized to 2, we don't know so throw warning
        /// (0): 0, 1 => PBKA => 0, 1, 2
        /// (3): 6 => PBKD => 1, 6, 7
        /// </summary>
        [TestMethod]
        public void ChlorpyrifosKineticModelCalculator_TestValidOrNot5() {
            var instances = getInstance();
            var selectedInstances = new List<KineticModelInstance>() {
                instances[0],
                instances[3],
            };
            var warning = checkPBKmodels(selectedInstances);
            Assert.AreEqual(1, warning.Count);
        }

        /// <summary>
        /// Not OK
        /// (3): 6 => PBKD => 1, 6, 7
        /// (6): 1 => PBKG => 10, 11
        /// </summary>
        [TestMethod]
        public void ChlorpyrifosKineticModelCalculator_TestValidOrNot6() {
            var instances = getInstance();
            var selectedInstances = new List<KineticModelInstance>() {
                instances[3],
                instances[6],
            };
            var warning = checkPBKmodels(selectedInstances);
            Assert.AreEqual(1, warning.Count);
        }

        /// <summary>
        /// Not OK
        /// (4): 8 => PKBE => 9
        /// (7): 9 => PKBH => 8 
        /// </summary>
        [TestMethod]
        public void ChlorpyrifosKineticModelCalculator_TestValidOrNot7() {
            var instances = getInstance();
            var selectedInstances = new List<KineticModelInstance>() {
                instances[4],
                instances[7],
            };
            var warning = checkPBKmodels(selectedInstances);
            Assert.AreEqual(2, warning.Count);
        }

        /// <summary>
        /// Not OK, this one should be possible when 1 is not metabolized to 2, we dont know so throw warning
        /// (8): 0, 1 => PBKI => 2
        /// (9): 6 => PBKDJ => 1, 7 
        /// </summary>
        [TestMethod]
        public void ChlorpyrifosKineticModelCalculator_TestValidOrNot8() {
            var instances = getInstance();
            var selectedInstances = new List<KineticModelInstance>() {
                instances[8],
                instances[9],
            };
            var warning = checkPBKmodels(selectedInstances);
            Assert.AreEqual(1, warning.Count);
        }

        /// <summary>
        /// Not OK, both models can be combined
        /// (1): 3 => PBKB => 3
        /// (10): 4 => PBKK => 3 
        /// </summary>
        [TestMethod]
        public void ChlorpyrifosKineticModelCalculator_TestValidOrNot9() {
            var instances = getInstance();
            var selectedInstances = new List<KineticModelInstance>() {
                instances[1],
                instances[10],
            };
            var warning = checkPBKmodels(selectedInstances);
            Assert.AreEqual(1, warning.Count);
        }

        /// <summary>
        /// Not OK, both models can be combined
        /// (0): 0, 1 => PBKA => 0, 1, 2
        /// (11): 2 => PBKL => 2 
        /// </summary>
        [TestMethod]
        public void ChlorpyrifosKineticModelCalculator_TestValidOrNot10() {
            var instances = getInstance();
            var selectedInstances = new List<KineticModelInstance>() {
                instances[0],
                instances[11],
            };
            var warning = checkPBKmodels(selectedInstances);
            Assert.AreEqual(1, warning.Count);
        }

        /// <summary>
        /// Not OK, circular
        /// (4): 8 => PKBE => 9
        /// (12): 7 => PBKM => 8
        /// (13): 9 => PBKN => 7
        /// </summary>
        [TestMethod]
        public void ChlorpyrifosKineticModelCalculator_TestValidOrNot11() {
            var instances = getInstance();
            var selectedInstances = new List<KineticModelInstance>() {
                instances[4],
                instances[12],
                instances[13],
            };
            var warning = checkPBKmodels(selectedInstances);
            Assert.AreEqual(3, warning.Count);
        }

        /// <summary>
        /// Not OK
        /// (1): 3 => PBKB => 3 
        /// (14): 4 => PBKO => 3, 2
        /// </summary>
        [TestMethod]
        public void ChlorpyrifosKineticModelCalculator_TestValidOrNot12() {
            var instances = getInstance();
            var selectedInstances = new List<KineticModelInstance>() {
                instances[1],
                instances[14],
            };
            var warning = checkPBKmodels(selectedInstances);
            Assert.AreEqual(1, warning.Count);
        }

        private List<string> checkPBKmodels(List<KineticModelInstance> selectedInstances) {
            var warningsList = new List<string>();
            var parents = selectedInstances
                .SelectMany(c => c.KineticModelSubstances
                    .Where(r => r.SubstanceDefinition.IsInput)
                    .Select(r => r.Substance)
                )
                .GroupBy(c => c)
                .ToDictionary(c => c.Key, c => c.Count());
            var parentSubstances = parents.Keys.Distinct().ToList();
            foreach (var parent in parents) {
                if (parent.Value > 1) {
                    var instancesCodes = new List<string>();
                    foreach (var item in selectedInstances) {
                        if (item.Substances.Contains(parent.Key)) {
                            instancesCodes.Add(item.IdModelInstance);
                        }
                    }
                    warningsList.Add($"Parent substance {parent.Key.Code} is found in {parent.Value} models: {string.Join(", ", instancesCodes)}.");
                }
            }

            var metabolitesSet = selectedInstances
                .Where(c => c.KineticModelSubstances != null)
                .SelectMany(c => c.KineticModelSubstances
                .Select(s=>s.Substance))
                .ToList();
            metabolitesSet.AddRange(selectedInstances
                .Where(c => c.KineticModelSubstances == null)
                .SelectMany(c => c.Substances).ToList());

            var metabolites = metabolitesSet
                .GroupBy(c => c)
                .ToDictionary(c => c.Key, c => c.Count());

            var metabolitesSubstances = metabolites.Keys.Distinct().ToList();
            foreach (var metabolite in metabolites) {
                if (metabolite.Value > 1) {
                    var instancesCodes = new List<string>();
                    foreach (var item in selectedInstances) {
                        if (item.KineticModelSubstances != null && item.KineticModelSubstances.Select(c => c.Substance).Contains(metabolite.Key)) {
                            instancesCodes.Add(item.IdModelInstance);
                        }
                        if (item.Substances.Contains(metabolite.Key)) {
                            instancesCodes.Add(item.IdModelInstance);
                        } 
                    }
                    warningsList.Add($"Metabolite substance {metabolite.Key.Code} is found in {metabolite.Value} models: {string.Join(", ", instancesCodes.Distinct())}.");
                }
            }

            if (warningsList.Count == 0) {
                foreach (var instance1 in selectedInstances) {
                    foreach (var instance2 in selectedInstances) {
                        if (instance1 != instance2) {
                            var outputSubstances = instance1.KineticModelSubstances != null
                                ? instance1.KineticModelSubstances.Select(c => c.Substance).ToList()
                                : instance1.Substances;
                            var inputSubstances = instance2.Substances;
                            var union = outputSubstances.Intersect(inputSubstances).ToList();
                            if (union.Any()) {
                                var substancesCodes = union.Select(c => c.Code).ToList();
                                warningsList.Add($"Substance(s) {string.Join(", ", substancesCodes)} is found in models: {instance1.IdModelInstance} (output)  and {instance2.IdModelInstance} (input).");
                            }
                        }
                    }
                }
            }
            return warningsList;
        }

        private static KineticModelInstance fakeInstance(
            string idModelInstance,
            string[] kmSubstanceIds,
            Compound[] substances,
            bool[] isInput
        ) {
            var kineticModelSubstances = kmSubstanceIds
                .Select((r,ix) => new KineticModelSubstance() {
                    Substance = substances[ix],
                    SubstanceDefinition = new KineticModelSubstanceDefinition() {
                        Id = r,
                        Name = r,
                        Description = r,
                        IsInput = isInput[ix]
                    }
                })
                .ToList();
            var result = new KineticModelInstance() {
                IdModelInstance = idModelInstance,
                KineticModelSubstances = kineticModelSubstances,
                KineticModelDefinition = new KineticModelDefinition() {
                    Name = $"DEF_{idModelInstance}"
                }
            };
            return result;
        }

        /// <summary>
        /// (0): 0, 1 => PBKA => 0, 1, 2
        /// (1): 3 => PBKB => 3
        /// (2): 4, 5 => PBKC => 4, 5
        /// (3): 6 => PBKD => 1, 6, 7 
        /// (4): 8 => PKBE => 9
        /// (5): 1 => PKBF => 1
        /// (6): 1 => PBKG => 10, 11
        /// (7): 9 => PKBH => 8 
        /// (8): 0, 1 => PBKI => 2
        /// (9): 6 => PBKJ => 1, 7 
        /// (10): 4 => PBKK => 3 
        /// (11): 2 => PBKL => 2 
        /// (12): 7 => PBKM => 8
        /// (13): 9 => PBKN => 7
        /// (14): 4 => PBKO => 3, 2
        /// </summary>
        /// <returns></returns>
        private static List<KineticModelInstance> getInstance() {
            var substances = MockSubstancesGenerator.Create(12);

            var selectedInstances = new List<KineticModelInstance> {
                // 0, 1 => PBKA => 0, 1, 2
                fakeInstance(
                    "PBK_A",
                    new[] { "P", "M1", "M2" },
                    new[] { substances[0], substances[1], substances[2] },
                    new[] { true, true, false }
                ),

                // 3 => PBKB => 3
                fakeInstance(
                    "PBK_A",
                    new[] { "P3" },
                    new[] { substances[3] },
                    new[] { true }
                ),

                // 4, 5 => PBKC => 4, 5
                fakeInstance(
                    "PBK_C",
                    new[] { "P4", "P5" },
                    new[] { substances[4], substances[5] },
                    new[] { true, true }
                ),

                // 6 => PBKD => 6, 7, 1
                fakeInstance(
                    "PBK_D",
                    new[] { "P6", "M7", "M1" },
                    new[] { substances[6], substances[7], substances[1] },
                    new[] { true, false, false }
                ),

                // 8 => PKBE => 9
                fakeInstance(
                    "PBK_E",
                    new[] { "P8", "M9" },
                    new[] { substances[8], substances[9] },
                    new[] { true, false }
                ),

                // 1 => PKBF => 1
                fakeInstance(
                    "PBK_F",
                    new[] { "P1" },
                    new[] { substances[1] },
                    new[] { true }
                ),

                // 1 => PBKG => 10, 11
                fakeInstance(
                    "PBK_G",
                    new[] { "P1", "M10", "M11" },
                    new[] { substances[1], substances[10], substances[11] },
                    new[] { true, false, false }
                ),

                // 9 => PKBH => 8
                fakeInstance(
                    "PBK_H",
                    new[] { "P9", "M8" },
                    new[] { substances[9], substances[8] },
                    new[] { true, false }
                ),

                // 0, 1 => PBKI =>  2
                fakeInstance(
                    "PBK_I",
                    new[] { "P0", "P1", "M2" },
                    new[] { substances[0], substances[1], substances[2] },
                    new[] { true, true, false }
                ),

                // 6 => PBKJ => 7, 1
                fakeInstance(
                    "PBK_J",
                    new[] { "P6", "M7", "M1" },
                    new[] { substances[6], substances[7], substances[1] },
                    new[] { true, false, false }
                ),

                // 4 => PBKK => 3
                fakeInstance(
                    "PBK_K",
                    new[] { "P4", "M3" },
                    new[] { substances[4], substances[3] },
                    new[] { true, false }
                ),

                // 2 => PBKL => 2
                fakeInstance(
                    "PBK_L",
                    new[] { "P2" },
                    new[] { substances[2] },
                    new[] { true }
                ),

                // 7 => PKBM => 8
                fakeInstance(
                    "PBK_M",
                    new[] { "P7", "M8" },
                    new[] { substances[7], substances[8] },
                    new[] { true, false }
                ),

                // 9 => PKBN => 7
                fakeInstance(
                    "PBK_N",
                    new[] { "P9", "M7" },
                    new[] { substances[9], substances[7] },
                    new[] { true, false }
                ),

                // 4 => PBKO => 3, 2
                fakeInstance(
                    "PBK_O",
                    new[] { "P4", "M3", "M2" },
                    new[] { substances[4], substances[3], substances[2] },
                    new[] { true, false, false }
                )
            };

            return selectedInstances;
        }
    }
}
