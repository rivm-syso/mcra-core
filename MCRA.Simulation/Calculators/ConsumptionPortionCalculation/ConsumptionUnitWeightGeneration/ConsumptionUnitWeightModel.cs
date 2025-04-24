using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.Calculators.DietaryExposureCalculation.IndividualDietaryExposureCalculation.ConsumptionUnitWeightGeneration {
    public abstract class ConsumptionUnitWeightModel {
        public bool IsModellingUncertainty { get; protected set; }
        public abstract void CalculateParameters(FoodConsumptionQuantification consumptionUnit);
        public abstract double GenerateUnitWeight(IRandom random);
        public abstract void StartModellingUncertainty(IRandom random);
        public abstract void StopModelingUncertainty();
    }
}
