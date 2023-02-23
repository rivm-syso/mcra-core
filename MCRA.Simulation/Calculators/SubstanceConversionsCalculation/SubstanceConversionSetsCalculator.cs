using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.Calculators.SubstanceConversionsCalculation {
    public sealed class SubstanceConversionSetsCalculator {

        public Dictionary<Compound, SubstanceConversionCollection> ComputeGeneralTranslationSets(
            ILookup<Compound, SubstanceConversion> substanceConversions
        ) {
            var result = new Dictionary<Compound, SubstanceConversionCollection>();
            foreach (var measuredSubstanceGrouping in substanceConversions) {
                var measuredSubstance = measuredSubstanceGrouping.Key;
                var substanceConversionCollection = computeSubstanceConversionCollection(measuredSubstance, measuredSubstanceGrouping.ToList(), null, false);
                result.Add(measuredSubstance, substanceConversionCollection);
            }
            return result;
        }

        public Dictionary<Compound, SubstanceConversionCollection> ComputeFoodSpecificTranslationSets(
            Food food,
            ILookup<Compound, SubstanceConversion> substanceConversions,
            IDictionary<(Food, Compound), SubstanceAuthorisation> substanceAuthorisations = null,
            bool useSubstanceAuthorisations = true
        ) {
            var result = new Dictionary<Compound, SubstanceConversionCollection>();

            HashSet<Compound> authorisedSubstances = null;
            if (substanceAuthorisations != null) {
                authorisedSubstances = substanceAuthorisations.Values
                    .Where(r => r.Food == food || (food.BaseFood != null && r.Food == food.BaseFood))
                    .Select(r => r.Substance)
                    .ToHashSet();
            }

            foreach (var measuredSubstanceTranslations in substanceConversions) {
                var measuredSubstance = measuredSubstanceTranslations.Key;
                var translationSets = computeSubstanceConversionCollection(measuredSubstance, measuredSubstanceTranslations.ToList(), authorisedSubstances, useSubstanceAuthorisations);
                result.Add(measuredSubstance, translationSets);
            }
            return result;
        }

        private static SubstanceConversionCollection computeSubstanceConversionCollection(
            Compound measuredSubstance,
            List<SubstanceConversion> translations,
            HashSet<Compound> authorisedSubstances,
            bool useSubstanceAuthorisations
        ) {
            var result = new SubstanceConversionCollection() {
                MeasuredSubstance = measuredSubstance,
                LinkedActiveSubstances = translations.ToDictionary(r => r.ActiveSubstance, r => r.ConversionFactor)
            };
            var authorisedTranslations = translations.Where(r => authorisedSubstances?.Contains(r.ActiveSubstance) ?? true).ToList();
            var candidateTranslations = (useSubstanceAuthorisations && authorisedTranslations.Any()) ? authorisedTranslations : translations;
            candidateTranslations = candidateTranslations.OrderBy(r => r.ActiveSubstance.Code, StringComparer.OrdinalIgnoreCase).ToList();
            if (candidateTranslations.All(r => r.IsExclusive)) {
                result.SubstanceTranslationSets = candidateTranslations
                    .Select(r => {
                        var substanceConversions = new Dictionary<Compound, double> {
                            { r.ActiveSubstance, r.ConversionFactor }
                        };
                        return new SubstanceConversionSet() {
                            TranslationProportion = 1D / candidateTranslations.Count,
                            PositiveSubstanceConversions = substanceConversions,
                            IsAuthorised = authorisedSubstances?.Contains(r.ActiveSubstance) ?? true
                        };
                    })
                    .ToList();
            } else if (translations.Count(r => r.IsExclusive) == 1) {
                var exclusiveRecord = translations.First(r => r.IsExclusive);
                result.SubstanceTranslationSets = candidateTranslations
                    .Select(r => {
                        var substanceConversions = new Dictionary<Compound, double>();
                        if (r.IsExclusive) {
                            substanceConversions.Add(r.ActiveSubstance, r.ConversionFactor);
                        } else {
                            var proportion = r.Proportion ?? 1D;
                            substanceConversions.Add(r.ActiveSubstance, r.ConversionFactor * proportion);
                            substanceConversions.Add(exclusiveRecord.ActiveSubstance, exclusiveRecord.ConversionFactor * (1D - proportion));
                        }
                        return new SubstanceConversionSet() {
                            TranslationProportion = 1D / candidateTranslations.Count,
                            PositiveSubstanceConversions = substanceConversions,
                            IsAuthorised = authorisedSubstances?.Contains(r.ActiveSubstance) ?? true
                        };
                    })
                    .ToList();
            } else {
                throw new Exception($"Illegal substance translations for substance {measuredSubstance.Name} ({measuredSubstance.Code})");
            }
            return result;
        }
    }
}
