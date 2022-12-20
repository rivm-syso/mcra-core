using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.HazardCharacterisationCalculation;
using MCRA.Simulation.Calculators.RiskCalculation.ForwardCalculation;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Simulation.Calculators.RiskCalculation {
    public class RiskCalculatorBase {

        /// <summary>
        /// Calculate individual effects i.c. critical effect dose, margin of exposure, equivalent animal dose.
        /// Use the original critical effect dose; i.e., the critical effect dose defined on the original animal 
        /// (or in vitro system) of the dose response model.
        /// Note that the critical effectdose is corrected.
        /// A correction for (Exposure) Per Person is needed because exposures are at kg per bw level or per person,
        /// For the margin of exposure this is not needed in principal.
        /// Maybe a correction as applied her is incorrect and there should be one value for all humans
        /// </summary>
        /// <param name="individualEffects"></param>
        /// <param name="substance"></param>
        /// <param name="hazardCharacterisation"></param>
        /// <param name="intakeUnit"></param>
        /// <param name="isPerPerson"></param>
        /// <param name="seed"></param>
        /// <returns></returns>
        protected (Compound, List<IndividualEffect>) CalculateRisk(
            List<IndividualEffect> individualEffects,
            Compound substance,
            IHazardCharacterisationModel hazardCharacterisation,
            TargetUnit intakeUnit,
            bool isPerPerson
        ) {
            if (individualEffects.Any(c => c.IntraSpeciesDraw == 0)) {
                throw new System.Exception("Random draw contains zeros");
            }
            foreach (var item in individualEffects) {
                item.CriticalEffectDose = hazardCharacterisation.DrawIndividualHazardCharacterisation(item.IntraSpeciesDraw) * (isPerPerson ? item.CompartmentWeight : 1);
                item.EquivalentTestSystemDose = item.ExposureConcentration / hazardCharacterisation.CombinedAssessmentFactor;
            }
            // Forward calculation
            var model = hazardCharacterisation?.TestSystemHazardCharacterisation?.DoseResponseRelation;
            if (model != null) {
                var modelEquation = model.DoseResponseModelEquation;
                var modelParameters = model.DoseResponseModelParameterValues;
                var modelDoseUnit = hazardCharacterisation.TestSystemHazardCharacterisation.DoseUnit;
                var modelDoseUnitCorrectionFactor = modelDoseUnit.GetDoseAlignmentFactor(intakeUnit, substance.MolecularMass);
                var rModel = new RModelHealthImpact();
                //rModel.CalculateEffectValue(individualEffects, modelEquation, modelParameters, modelDoseUnitCorrectionFactor);
            }
            return (substance, individualEffects);
        }

    }
}
