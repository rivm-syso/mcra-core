using MCRA.Utils.ExtensionMethods;
using MCRA.Simulation.Calculators.SingleValueRisksCalculation;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class SingleValueRisksBySourceSubstanceSection : ActionSummaryBase {

        public List<SingleValueRisksBySourceSubstanceRecord> Records { get; set; }

        public void Summarize(
                ICollection<SingleValueRiskCalculationResult> results
            ) {
            Records = results
                .Select(c => {
                    return new SingleValueRisksBySourceSubstanceRecord() {
                        SourceName = c.Source.Name,
                        SourceCode = c.Source.Code,
                        ExposureRoute = c.Source.Route.GetShortDisplayName(),
                        SubstanceName = c.Substance.Name,
                        SubstanceCode = c.Substance.Code,
                        ExposureValue = c.Exposure,
                        HazardCharacterisationValue = c.HazardCharacterisation,
                        HazardQuotient = c.HazardQuotient,
                        MarginOfExposure = c.MarginOfExposure,
                        PotencyOrigin = c.Origin.GetShortDisplayName()
                    };
                })
                .OrderByDescending(c => c.HazardQuotient)
                .ToList();
        }
    }
}
