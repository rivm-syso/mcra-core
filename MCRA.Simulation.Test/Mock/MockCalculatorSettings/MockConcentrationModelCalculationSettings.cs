using MCRA.General;
using MCRA.General.Action.Settings.Dto;
using MCRA.Simulation.Calculators.ConcentrationModelCalculation;
using System.Collections.Generic;

namespace MCRA.Simulation.Test.Mock.MockCalculatorSettings {
    public class MockConcentrationModelCalculationSettings : IConcentrationModelCalculationSettings {
        public NonDetectsHandlingMethod NonDetectsHandlingMethod { get; set; }
        public ICollection<ConcentrationModelTypePerFoodCompoundDto> ConcentrationModelTypesPerFoodCompound { get; set; }
        public ConcentrationModelType DefaultConcentrationModel { get; set; }
        public double FractionOfLOR { get; set; }
        public double FractionOfMrl { get; set; }
        public bool IsFallbackMrl { get; set; }
        public bool CorrelateImputedValueWithSamplePotency { get; set; }
    }
}
