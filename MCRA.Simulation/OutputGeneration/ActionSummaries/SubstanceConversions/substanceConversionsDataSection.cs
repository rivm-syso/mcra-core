using MCRA.Data.Compiled.Objects;
using System.ComponentModel;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class SubstanceConversionsDataSection : SummarySection {

        [DisplayName("Total translations")]
        [Description("Total number of substance translations.")]
        public int TotalConversionRulesCount { get; set; }

        [DisplayName("Total measured substance definitions")]
        [Description("Number of measured substances with a substance translation.")]
        public int TotalMeasuredSubstanceDefinitionsCount { get; set; }

        [DisplayName("Complex substance translations")]
        [Description("Measured substances to which more than one active substance can be allocated.")]
        public int TotalComplexSubstanceConversionsCount { get; set; }

        [DisplayName("Identity translations")]
        [Description("Measured substances precisely one active substance translation, being the measured substance itself.")]
        public int TotalIdentityTranslationsCount { get; set; }

        public List<SubstanceConversionsDataRecord> Records { get; set; }

        public void Summarize(ICollection<SubstanceConversion> residueDefinitions) {
            var groupedResidueDefinitions = residueDefinitions.GroupBy(r => r.MeasuredSubstance);
            TotalConversionRulesCount = residueDefinitions.Count;
            TotalMeasuredSubstanceDefinitionsCount = residueDefinitions.Select(r => r.MeasuredSubstance).Distinct().Count();
            TotalComplexSubstanceConversionsCount = groupedResidueDefinitions.Count(r => r.Count() > 1);
            TotalIdentityTranslationsCount = groupedResidueDefinitions
                .Count(r => r.Count() == 1 && r.Key == r.First().ActiveSubstance);
            Records = residueDefinitions
                .Select(r => new SubstanceConversionsDataRecord() {
                    MeasuredSubstanceCode = r.MeasuredSubstance.Code,
                    MeasuredSubstanceName = r.MeasuredSubstance.Name,
                    ActiveSubstanceCode = r.ActiveSubstance.Code,
                    ActiveSubstanceName = r.ActiveSubstance.Name,
                    ConversionFactor = r.ConversionFactor,
                    IsExclusive = r.IsExclusive,
                    Proportion = r.Proportion,
                })
                .ToList();
        }
    }
}
