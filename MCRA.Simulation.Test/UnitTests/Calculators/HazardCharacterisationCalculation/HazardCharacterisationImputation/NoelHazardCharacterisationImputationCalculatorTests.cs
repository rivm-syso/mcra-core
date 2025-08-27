using MCRA.Utils.Statistics;
using MCRA.General;
using MCRA.Simulation.Calculators.HazardCharacterisationCalculation.HazardCharacterisationImputation;
using MCRA.Simulation.Calculators.HazardCharacterisationCalculation.HazardDoseTypeConversion;
using MCRA.Simulation.Test.Mock.MockCalculators;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;

namespace MCRA.Simulation.Test.UnitTests.Calculators.HazardCharacterisationCalculation.HazardCharacterisationImputation {

    /// <summary>
    /// TargetDoseCalculation calculator
    /// </summary>
    [TestClass]
    public class NoelHazardCharacterisationImputationCalculatorTests {

        /// <summary>
        /// Test initialisation of the Noel hazard characterisation imputation calculator.
        /// Checks whether the noels are correctly loaded.
        /// </summary>
        [TestMethod]
        public void NoelHazardCharacterisationImputationCalculator_TestInitialisation() {
            var substances = FakeSubstancesGenerator.Create(1);
            var effect = FakeEffectsGenerator.Create(1).First();
            var kineticConversionFactorCalculator = new MockKineticConversionFactorCalculator();
            var interSpeciesFactorModels = FakeInterSpeciesFactorModelsGenerator.Create(
                substances,
                [],
                effect,
                10D);
            var intraSpeciesFactorModels = FakeIntraSpeciesFactorModelsGenerator.Create(substances, effect, defaultFactor: 10D);
            var calculator = new NoelHazardCharacterisationImputationCalculator(
                effect,
                50,
                interSpeciesFactorModels,
                kineticConversionFactorCalculator,
                intraSpeciesFactorModels
            );

            Assert.AreEqual(_noelsCramerClassI.Length, calculator.NoelsCramerClassI.Count);
            Assert.AreEqual(_noelsCramerClassII.Length, calculator.NoelsCramerClassII.Count);
            Assert.AreEqual(_noelsCramerClassIII.Length, calculator.NoelsCramerClassIII.Count);
            Assert.AreEqual(_noelsCramerClassUnknown.Length, calculator.NoelsCramerClassUnknown.Count);

            Assert.AreEqual(_noelsCramerClassI.Average(), calculator.NoelsCramerClassI.Average(), 1e-4);
            Assert.AreEqual(_noelsCramerClassII.Average(), calculator.NoelsCramerClassII.Average(), 1e-4);
            Assert.AreEqual(_noelsCramerClassIII.Average(), calculator.NoelsCramerClassIII.Average(), 1e-4);
            Assert.AreEqual(_noelsCramerClassUnknown.Average(), calculator.NoelsCramerClassUnknown.Average(), 1e-4);

            var nominalValueCramerClassI = 1D / _noelsCramerClassI.Average(r => 1 / r);
            var nominalValueCramerClassII = 1D / _noelsCramerClassII.Average(r => 1 / r);
            var nominalValueCramerClassIII = 1D / _noelsCramerClassIII.Average(r => 1 / r);
            var nominalValueCramerClassUnknown = 1D / _noelsCramerClassUnknown.Average(r => 1 / r);
        }

        /// <summary>
        /// Checks correct imputation of a substance with unknown Cramer class.
        /// </summary>
        [TestMethod()]
        public void NoelHazardCharacterisationImputationCalculator_TestUnbiasedCramerUnknown() {
            var substances = FakeSubstancesGenerator.Create(1);
            var effect = FakeEffectsGenerator.Create(1).First();
            var kineticConversionFactorCalculator = new MockKineticConversionFactorCalculator();
            var interSpeciesFactorModels = FakeInterSpeciesFactorModelsGenerator.Create(
                substances,
                [],
                effect,
                10D);
            var intraSpeciesFactorModels = FakeIntraSpeciesFactorModelsGenerator.Create(
                substances,
                effect,
                defaultFactor: 10D);
            var calculator = new NoelHazardCharacterisationImputationCalculator(
                effect,
                50,
                interSpeciesFactorModels,
                kineticConversionFactorCalculator,
                intraSpeciesFactorModels);
            var targetUnit = TargetUnit.FromExternalExposureUnit(ExternalExposureUnit.mgPerKgBWPerDay);
            var hazardDoseTypeConverter = new HazardDoseConverter(PointOfDepartureType.Noael, targetUnit.ExposureUnit);
            var imputeNominal = calculator.ImputeNominal(substances.First(), hazardDoseTypeConverter, targetUnit, null);

            var nominalValueCramerClassUnknown = 1D / _noelsCramerClassUnknown.Average(r => 1 / r);
            var expected = nominalValueCramerClassUnknown / 100;
            Assert.AreEqual(expected, imputeNominal.Value, 1e-5);
        }

        /// <summary>
        /// Checks correct imputation of a substance with Cramer class 1.
        /// </summary>
        [TestMethod()]
        public void NoelHazardCharacterisationImputationCalculator_TestNominalImputation2() {
            var substances = FakeSubstancesGenerator.Create(1, null, cramerClasses: [1]);
            var effect = FakeEffectsGenerator.Create(1).First();
            var kineticConversionFactorCalculator = new MockKineticConversionFactorCalculator();
            var interSpeciesFactorModels = FakeInterSpeciesFactorModelsGenerator.Create(
                substances,
                [],
                effect,
                10D);
            var intraSpeciesFactorModels = FakeIntraSpeciesFactorModelsGenerator.Create(substances,
                effect,
                defaultFactor: 10D);
            var calculator = new NoelHazardCharacterisationImputationCalculator(
                effect,
                50,
                interSpeciesFactorModels,
                kineticConversionFactorCalculator,
                intraSpeciesFactorModels);
            var targetUnit = TargetUnit.FromExternalExposureUnit(ExternalExposureUnit.mgPerKgBWPerDay);
            var hazardDoseTypeConverter = new HazardDoseConverter(PointOfDepartureType.Noael, targetUnit.ExposureUnit);
            var imputeNominal = calculator.ImputeNominal(substances.First(), hazardDoseTypeConverter, targetUnit, null);

            var nominalValueCramerClassI = 1D / _noelsCramerClassI.Average(r => 1 / r);
            var expected = nominalValueCramerClassI / 100;
            Assert.AreEqual(expected, imputeNominal.Value, 1e-5);
        }

        /// <summary>
        /// Checks correct imputation of a substance with Cramer class 1 including target intake unit conversion.
        /// </summary>
        [TestMethod()]
        public void NoelHazardCharacterisationImputationCalculator_TestNominalImputation3() {
            var substances = FakeSubstancesGenerator.Create(1, null, cramerClasses: [1]);
            var effect = FakeEffectsGenerator.Create(1).First();
            var kineticConversionFactorCalculator = new MockKineticConversionFactorCalculator();
            var interSpeciesFactorModels = FakeInterSpeciesFactorModelsGenerator.Create(
                substances,
                [],
                effect,
                10D);
            var intraSpeciesFactorModels = FakeIntraSpeciesFactorModelsGenerator.Create(
                substances,
                effect,
                defaultFactor: 10D);
            var calculator = new NoelHazardCharacterisationImputationCalculator(
                effect,
                50,
                interSpeciesFactorModels,
                kineticConversionFactorCalculator,
                intraSpeciesFactorModels
            );
            var targetUnit = TargetUnit.FromExternalExposureUnit(ExternalExposureUnit.gPerKgBWPerDay);
            var hazardDoseTypeConverter = new HazardDoseConverter(PointOfDepartureType.Noael, targetUnit.ExposureUnit);
            var imputeNominal = calculator.ImputeNominal(substances.First(), hazardDoseTypeConverter, targetUnit, null);

            var nominalValueCramerClassI = 1D / _noelsCramerClassI.Average(r => 1 / r);
            var expected = nominalValueCramerClassI / 100 / 1000;
            Assert.AreEqual(expected, imputeNominal.Value, 1e-5);
        }

        /// <summary>
        /// Checks correct imputation of a substance with Cramer class 1 including target intake unit conversion.
        /// </summary>
        [TestMethod()]
        public void NoelHazardCharacterisationImputationCalculator_TestUncertaintyImputation3() {
            var substances = FakeSubstancesGenerator.Create(1, null, cramerClasses: [1]);
            var effect = FakeEffectsGenerator.Create(1).First();
            var kineticConversionFactorCalculator = new MockKineticConversionFactorCalculator(.8);
            var interSpeciesFactorModels = FakeInterSpeciesFactorModelsGenerator.Create(
                substances,
                [],
                effect,
                10D);
            var intraSpeciesFactorModels = FakeIntraSpeciesFactorModelsGenerator.Create(substances,
                effect,
                defaultFactor: 10D);
            var calculator = new NoelHazardCharacterisationImputationCalculator(effect,
                50,
                interSpeciesFactorModels,
                kineticConversionFactorCalculator,
                intraSpeciesFactorModels
            );

            var targetUnit = TargetUnit.FromExternalExposureUnit(ExternalExposureUnit.gPerKgBWPerDay);
            var hazardDoseTypeConverter = new HazardDoseConverter(PointOfDepartureType.Noael, targetUnit.ExposureUnit);
            var imputeNominal = calculator.ImputeNominal(substances.First(), hazardDoseTypeConverter, targetUnit, null);

            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var uncertains = new List<double>();
            for (int i = 0; i < 1000; i++) {
                var imputed = calculator.ImputeUncertaintyRun(substances.First(), hazardDoseTypeConverter, targetUnit, random, null);
                uncertains.Add(imputed.Value);
            }

            var lower = uncertains.Percentile(2.5);
            var upper = uncertains.Percentile(97.5);

            var nominalValueCramerClassI = 1D / _noelsCramerClassI.Average(r => 1 / r);
            Assert.AreEqual(nominalValueCramerClassI * .1 * .1 * .001, imputeNominal.Value, 1e-5);
            Assert.IsTrue(imputeNominal.Value > lower);
            Assert.IsTrue(imputeNominal.Value < upper);
        }

        #region Mocks & constants

        private static double[] _noelsCramerClassI = [
            242 , 110 , 33.33333333 , 250 , 1001 , 500 , 5.5 , 1458 , 100 , 887 , 510 , 100 , 18 , 60 , 6883 , 1644 , 41.66666667 , 409.3333333 , 38.33333333 , 175 , 7203 , 355 , 1478.333333 , 33.33333333 , 41.66666667 , 295.6666667 , 59 , 51.33333333 , 170 , 41.66666667 , 2218 , 1250 , 83.33333333 , 62.5 , 250 , 491.6666667 , 590 , 16.66666667 , 0.2 , 0.466666667 , 86 , 100 , 1441 , 3.333333333 , 5241 , 300 , 8.4 , 4.8 , 166.6666667 , 166.6666667 , 12.5 , 295.6666667 , 295.6666667 , 41.66666667 , 45.33333333 , 178 , 100 , 100 , 250 , 26.43333333 , 3502 , 550 , 15 , 720 , 295.6666667 , 25 , 25 , 288 , 3442 , 201.3333333 , 26.66666667 , 20.83333333 , 8.333333333 , 300 , 1441 , 150 , 150 , 33.33333333 , 1774 , 3.333333333 , 295.6666667 , 1.566666667 , 105.3333333 , 975.6666667 , 0.018 , 718 , 1472 , 250 , 250 , 10 , 1600 , 296 , 166.6666667 , 156 , 8.4 , 72 , 3.333333333 , 187 , 16.66666667 , 16.66666667 , 250 , 360 , 100 , 89 , 3 , 840 , 153 , 26.66666667 , 3.333333333 , 835 , 591.3333333 , 86 , 833.3333333 , 10.66666667 , 10 , 4 , 1000 , 784 , 360 , 360 , 1478.333333 , 3602 , 3600 , 12 , 720 , 666.6666667 , 181 , 129 , 103.3333333 , 83.33333333 , 196.6666667 , 0.5 , 1000 , 46 , 360 , 1441 , 100
        ];

        private static double[] _noelsCramerClassII = [
            53 , 1.6 , 16.53333333 , 41.66666667 , 25 , 10.1 , 89 , 41.66666667 , 750 , 30 , 150 , 500 , 200 , 1441 , 50 , 6 , 5.333333333 , 30 , 5 , 166.6666667 , 6.666666667 , 6.666666667 , 50 , 23 , 360 , 1.666666667 , 0.333333333 , 1.666666667
        ];

        private static double[] _noelsCramerClassIII = [
            39 , 138 , 790 , 16 , 90 , 2.6 , 0.833333333 , 4 , 20.66666667 , 166.6666667 , 390.3333333 , 83.33333333 , 1100 , 16.66666667 , 333.3333333 , 100 , 103.3333333 , 115 , 10 , 10 , 58.33333333 , 2.5 , 266 , 110 , 59 , 621 , 10 , 190 , 25 , 0.066666667 , 25 , 2.5 , 0.333333333 , 6 , 0.3 , 0.2 , 2 , 25 , 20.66666667 , 7.5 , 2.5 , 10 , 3.333333333 , 316 , 83.33333333 , 33.33333333 , 65.33333333 , 1 , 500 , 1 , 333.3333333 , 172 , 133.3333333 , 237 , 1376 , 2 , 2 , 24.66666667 , 0.9 , 36 , 3.5 , 0.03 , 2.5 , 0.18 , 83.33333333 , 10 , 10 , 2.5 , 2.5 , 5 , 1.166666667 , 10.41666667 , 4.166666667 , 360 , 40 , 12 , 0.1 , 1 , 50 , 18 , 33.33333333 , 36 , 18.66666667 , 50 , 24 , 0.666666667 , 5 , 15 , 21 , 40 , 10 , 86 , 166.6666667 , 16.66666667 , 2.8 , 25 , 3.166666667 , 9.6 , 400 , 1 , 0.236666667 , 1 , 10 , 148 , 25 , 0.055 , 23.33333333 , 13 , 52 , 15 , 2 , 16.66666667 , 2.2 , 41.66666667 , 10 , 3 , 83.33333333 , 5 , 2.9 , 5 , 6.666666667 , 10 , 1 , 5 , 147 , 130 , 47 , 2.5 , 150 , 0.4 , 50 , 3 , 62 , 133.3333333 , 15 , 3.4 , 0.5 , 0.5 , 1.5 , 50 , 8.45 , 390 , 3760 , 40 , 402.3333333 , 12.5 , 24 , 864 , 3.333333333 , 3 , 0.333333333 , 33.33333333 , 20 , 100 , 14.8 , 15 , 1442 , 11.33333333 , 5 , 5.666666667 , 5 , 0.3 , 0.333333333 , 3.333333333 , 1 , 0.23 , 8 , 0.005 , 10 , 43 , 25 , 2 , 0.2 , 0.4 , 50 , 126 , 2 , 0.05 , 138 , 3775 , 50 , 333.3333333 , 66.66666667 , 10.33333333 , 0.133333333 , 4 , 50 , 3 , 10 , 5.333333333 , 3.1 , 150 , 20.23333333 , 0.19 , 0.05 , 473 , 1 , 7 , 5 , 8.333333333 , 2 , 60 , 200 , 75 , 15 , 0.2 , 3 , 0.083333333 , 15 , 0.566666667 , 35 , 1 , 1716 , 0.666666667 , 0.1 , 5 , 5 , 2.666666667 , 41.66666667 , 41.66666667 , 8 , 2 , 40 , 1 , 10 , 0.5 , 300 , 0.133333333 , 10 , 0.005 , 1 , 0.25 , 0.25 , 0.333333333 , 0.08 , 0.333333333 , 2.5 , 3.333333333 , 0.333333333 , 1 , 0.3 , 1180 , 10 , 30 , 14 , 203 , 0.75 , 73 , 10 , 150 , 41.66666667 , 50 , 25 , 10 , 7.833333333 , 5 , 300 , 2.5 , 10 , 3.333333333 , 100 , 1.25 , 30 , 66.66666667 , 5 , 13.33333333 , 100 , 30 , 56.66666667 , 50 , 62.66666667 , 0.033333333 , 0.25 , 400 , 12.5 , 1 , 0.2 , 5 , 5 , 8.333333333 , 33.33333333 , 14 , 0.025 , 7.5 , 5 , 116.6666667 , 4.166666667 , 15 , 5 , 12 , 40 , 0.17 , 0.2 , 6.666666667 , 25 , 0.2 , 33.33333333 , 125 , 30 , 36 , 4 , 43 , 1 , 363 , 43.33333333 , 16 , 105.3333333 , 55 , 133.3333333 , 10 , 1 , 0.836666667 , 16.66666667 , 0.333333333 , 36 , 12.5 , 0.5 , 2.5 , 1.266666667 , 0.3 , 1670 , 1150 , 0.733333333 , 4.166666667 , 1 , 1.8 , 0.04 , 8.333333333 , 123.3333333 , 0.59 , 41.66666667 , 75 , 3 , 233.3333333 , 3 , 37 , 94 , 1297 , 8.333333333 , 2 , 47 , 2 , 6.2 , 0.35 , 1073 , 592 , 20 , 20 , 0.5 , 86 , 100 , 33.33333333 , 5 , 24 , 1.666666667 , 3.5 , 12 , 0.833333333 , 4.433333333 , 5 , 2 , 5 , 16.66666667 , 5 , 0.666666667 , 30 , 300 , 0.833333333 , 385 , 25 , 400 , 0.03 , 13.33333333 , 12 , 0.193333333 , 40 , 5 , 10 , 0.38 , 360 , 23 , 18 , 0.52 , 720 , 0.016666667 , 30 , 3442 , 3602 , 25 , 200 , 2.2 , 154.6666667 , 311 , 226 , 600 , 2.32 , 7 , 7.5 , 0.1 , 72 , 0.113333333 , 33.33333333 , 62 , 4.666666667 , 8.333333333 , 25 , 6 , 103.3333333 , 0.166666667 , 17 , 50 , 1.25 , 1.666666667 , 10 , 1 , 8 , 5 , 1000 , 5.333333333 , 55 , 0.75 , 0.044 , 0.013333333 , 3 , 13.33333333 , 33.33333333 , 1 , 14.8 , 1.333333333 , 1000 , 125 , 6.133333333 , 294 , 33.33333333 , 3 , 0.33 , 793 , 10 , 16 , 10 , 7.333333333 , 1 , 16.66666667 , 24.3 , 10 , 10 , 1 , 0.02
        ];

        private static double[] _noelsCramerClassUnknown {
            get {
                return _noelsCramerClassI.Concat(_noelsCramerClassII).Concat(_noelsCramerClassIII).ToArray();
            }
        }

        #endregion
    }
}
