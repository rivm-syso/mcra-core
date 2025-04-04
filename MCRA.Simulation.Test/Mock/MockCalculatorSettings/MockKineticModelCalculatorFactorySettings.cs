﻿using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Calculators.KineticModelCalculation;

namespace MCRA.Simulation.Test.Mock.MockCalculatorSettings {
    public sealed class MockKineticModelCalculatorFactorySettings : IKineticModelCalculatorFactorySettings {

        public MockKineticModelCalculatorFactorySettings() {
            var dto = new KineticModelsModuleConfig();
        }

        public string CodeModel { get; set; }

        public string CodeSubstance { get; set; }

        public string CodeCompartment { get; set; }

        public int NumberOfDays { get; set; }

        public int NumberOfDosesPerDay { get; set; }

        public int NumberOfDosesPerDayNonDietaryOral { get; set; }

        public int NumberOfDosesPerDayNonDietaryDermal { get; set; }

        public int NumberOfDosesPerDayNonDietaryInhalation { get; set; }

        public int NonStationaryPeriod { get; set; }

        public bool UseParameterVariability { get; set; }

        public bool SpecifyEvents { get; set; }

        public string SelectedEvents { get; set; }
    }
}
