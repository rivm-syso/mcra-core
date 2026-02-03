using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.OutputGeneration.Generic;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class OutdoorAirConcentrationsSummarySection : SubstanceConcentrationsSummarySection {

        public void Summarize(
            ICollection<AirConcentration> concentrations,
            AirConcentrationUnit concentrationUnit,
            double lowerPercentage,
            double upperPercentage
        ) {
            var simpleConcentrations = concentrations
                .Select(SimpleSubstanceConcentration.Clone)
                .ToList();
            ConcentrationUnit = concentrationUnit.GetShortDisplayName();
            Records = summarizeConcentrations(
                simpleConcentrations,
                lowerPercentage,
                upperPercentage
            );
            PercentileRecords = summarizeBoxPlotRecords(
                simpleConcentrations
            );
        }
    }
}
