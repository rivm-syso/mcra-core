using System;
using Castle.Components.DictionaryAdapter;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.General.OpexProductDefinitions.Dto;
using MCRA.Simulation.Actions.TargetExposures;
using MCRA.Simulation.Calculators.KineticConversionFactorModels;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using MCRA.Utils.ExtensionMethods;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Calculator.TargetUnitCalculation {
    /// <summary>
    /// TargetUnitCalculation calculator
    /// </summary>
    [TestClass]
    public class TargetUnitCalculationTest {

        /// <summary>
        /// Test TargetUnitCalculation. .
        /// </summary>
        [TestMethod]
        [DataRow(BiologicalMatrix.Urine, InternalModelType.ConversionFactorModel, ExpressionType.None, false, "µg/L urine")]
        [DataRow(BiologicalMatrix.Urine, InternalModelType.ConversionFactorModel, ExpressionType.Creatinine, true, "µg/L creatinine urine")]
        [DataRow(BiologicalMatrix.Urine, InternalModelType.PBKModel, ExpressionType.None, false, "µg/L urine")]
        [DataRow(BiologicalMatrix.Urine, InternalModelType.PBKModelOnly, ExpressionType.None, false, "µg/L urine")]
        [DataRow(BiologicalMatrix.Blood, InternalModelType.ConversionFactorModel, ExpressionType.None, false, "µg/L blood")]
        [DataRow(BiologicalMatrix.Blood, InternalModelType.ConversionFactorModel, ExpressionType.Lipids, true, "µg/L lipids blood")]
        [DataRow(BiologicalMatrix.Blood, InternalModelType.PBKModel, ExpressionType.None, false, "µg/L blood")]
        [DataRow(BiologicalMatrix.Blood, InternalModelType.PBKModelOnly, ExpressionType.None, false, "µg/L blood")]
        [DataRow(BiologicalMatrix.Liver, InternalModelType.ConversionFactorModel, ExpressionType.None, false, "µg/L liver")]
        [DataRow(BiologicalMatrix.Liver, InternalModelType.ConversionFactorModel, ExpressionType.None, true, "µg/L liver")]
        public void TargetUnitCalculation_Test(
            BiologicalMatrix biologicalMatrix,
            InternalModelType internalModelType,
            ExpressionType expressionType,
            bool standardisation,
            string expected
        ) {
            var project = new ProjectDto();
            var config = project.TargetExposuresSettings;

            var substances = MockSubstancesGenerator.Create(3);
            var dietaryExposureUnit = TargetUnit.FromExternalExposureUnit(ExternalExposureUnit.ugPerKgBWPerDay);
            var exposureRoutes = new[] { ExposurePathType.Dermal, ExposurePathType.Oral, ExposurePathType.Inhalation };
            var kineticConversionFactors = MockKineticModelsGenerator.CreateKineticConversionFactors(
                substances,
                exposureRoutes,
                TargetUnit.FromInternalDoseUnit(DoseUnit.ugPerL, biologicalMatrix)
            );
            foreach (var kcf in kineticConversionFactors) {
                kcf.BiologicalMatrixTo = biologicalMatrix;
                kcf.ExpressionTypeTo = expressionType;
            }
            var kineticConversionFactorModels = kineticConversionFactors?
                .Select(c => KineticConversionFactorCalculatorFactory
                    .Create(c, false)
                ).ToList();

            var data = new ActionData() {
                DietaryExposureUnit = TargetUnit.FromExternalExposureUnit(ExternalExposureUnit.ugPerKgBWPerDay),
                KineticConversionFactorModels = kineticConversionFactorModels,
            };
            var settings = new TargetExposuresModuleSettings(config);
            config.TargetDoseLevelType = TargetLevelType.Internal;
            config.CodeCompartment = biologicalMatrix.ToString();
            config.InternalModelType = internalModelType;
            config.CreatinineStandardisationUrine = standardisation;
            config.LipidsStandardisationBlood = standardisation;
            var targetExposureUnit = TargetUnitCalculator.Create(
                config,
                data.DietaryExposureUnit.ExposureUnit,
                data.KineticConversionFactorModels,
                settings
            );
            Assert.AreEqual(expected, targetExposureUnit.ToString());
        }
    }
}
