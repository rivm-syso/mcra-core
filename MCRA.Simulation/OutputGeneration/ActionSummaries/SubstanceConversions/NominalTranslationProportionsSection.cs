using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.SubstanceConversionsCalculation;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class NominalTranslationProportionsSection : SummarySection {
        public List<NominalTranslationProportionRecord> Records { get; set; }

        public void Summarize(ICollection<SubstanceConversion> substanceConversions) {
            var calculator = new SubstanceConversionSetsCalculator();
            var nominalTranslations = calculator.ComputeGeneralTranslationSets(substanceConversions.ToLookup(r => r.MeasuredSubstance));
            Records = nominalTranslations
                .Values
                .SelectMany(r => r.SubstanceTranslationSets, (r, s) => {
                    var substances = r.LinkedActiveSubstances.Keys.OrderBy(sc => sc.Name);
                    var conversionFactors = substances
                        .Select(sc => s.PositiveSubstanceConversions.ContainsKey(sc) ? s.PositiveSubstanceConversions[sc] : 0D)
                        .ToList();
                    return new NominalTranslationProportionRecord() {
                        MeasuredSubstanceCode = r.MeasuredSubstance.Code,
                        MeasuredSubstanceName = r.MeasuredSubstance.Name,
                        ActiveSubstanceCodes = substances.Select(c => c.Code).ToList(),
                        ActiveSubstanceNames = substances.Select(c => c.Name).ToList(),
                        ConversionFactors = conversionFactors,
                        Proportion = s.TranslationProportion,
                    };
                })
                .ToList();
        }
    }
}
