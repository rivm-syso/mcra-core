using MCRA.General;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Actions.TargetExposures;
using MCRA.Simulation.Calculators.KineticConversionFactorModels;

namespace MCRA.Simulation.Calculators.TargetExposuresCalculation {
    public class TargetUnitCalculator {
        public static TargetUnit Create(
            TargetExposuresModuleConfig config,
            ExposureUnitTriple exposureUnit,
            ICollection<IKineticConversionFactorModel> kineticConversionFactorModels,
            TargetExposuresModuleSettings settings
        ) {

            // Determine target (from compartment selection) and appropriate internal exposure unit
            var codeCompartment = config.TargetDoseLevelType == TargetLevelType.Systemic ? BiologicalMatrix.WholeBody.ToString() : config.CodeCompartment;
            var biologicalMatrix = BiologicalMatrixConverter.FromString(codeCompartment, BiologicalMatrix.WholeBody);

            var target = new ExposureTarget(biologicalMatrix);
            var timeScale = config.ExposureType == ExposureType.Acute
                ? TimeScaleUnit.Peak
                : TimeScaleUnit.SteadyState;
            var standardiseTarget = (biologicalMatrix != BiologicalMatrix.Urine && biologicalMatrix != BiologicalMatrix.Blood)
                ? false
                : (biologicalMatrix == BiologicalMatrix.Urine ? settings.CreatinineStandardisationUrine : settings.LipidsStandardisationBlood);

            // Determine target and exposure unit
            if (config.InternalModelType == InternalModelType.ConversionFactorModel && kineticConversionFactorModels.Any()) {
                
                //Only one biological matrix with expression type is allowed, currently select the first one.
                var expressionTypes = kineticConversionFactorModels
                    .Select(c => c.ConversionRule)
                    .Where(c => c.BiologicalMatrixTo == biologicalMatrix && standardiseTarget
                        ? c.ExpressionTypeTo != ExpressionType.None
                        : c.ExpressionTypeTo == ExpressionType.None
                    )
                    .GroupBy(c => c.ExpressionTypeTo)
                    .Select(c => c.First())
                    .Select(c => (
                        expressionType: c.ExpressionTypeTo,
                        doseUnitTo: c.DoseUnitTo)
                    ).ToList();

                var expressionTypeMessage = standardiseTarget ? "(standardised)" : "(unstandardised)";
                if (!expressionTypes.Any()) {
                    throw new Exception($"For biological matrix {biologicalMatrix.ToString()} {expressionTypeMessage} no kinetic conversion factors are found." +
                        $" Please supply expression type in the kinetic-conversion-factor datasource");
                }
                exposureUnit = expressionTypes.First().doseUnitTo;
                exposureUnit.TimeScaleUnit = timeScale;
                target = new ExposureTarget(biologicalMatrix, expressionTypes.Single().expressionType);
            } else if (config.InternalModelType == InternalModelType.PBKModel || config.InternalModelType == InternalModelType.PBKModelOnly) {
                //TODO this is only valid for unstandardised biological matrices as long as PBK models do not correct for creatinine or lipids
                //Currently take the target unit of the PBK model,
                var concentrationUnit = biologicalMatrix.GetTargetConcentrationUnit();
                exposureUnit = new ExposureUnitTriple(
                    concentrationUnit.GetSubstanceAmountUnit(),
                    concentrationUnit.GetConcentrationMassUnit(),
                    timeScale
                );
            }
            var targetUnit = new TargetUnit(target, exposureUnit);
            return targetUnit;
        }
    }
}
