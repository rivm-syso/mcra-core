using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.BodIndicatorModels;

namespace MCRA.Simulation.Calculators.EnvironmentalBurdenOfDiseaseCalculation {
    public class BoDConversionsCalculator {

        public List<(BurdenOfDisease, List<BodIndicatorConversion>)> GetBurdenOfDiseaseBasedOnConversion(
            List<BurdenOfDisease> burdensOfDisease,
            List<BodIndicatorConversion> bodIndicatorConversions,
            Effect effect,
            BodIndicator bodIndicator,
            List<BodIndicatorConversion> foundConversions
        ) {
            var results = new List<(BurdenOfDisease, List<BodIndicatorConversion>)>();
            var burdenOfDisease = burdensOfDisease.SingleOrDefault(r => r.BodIndicator == bodIndicator && r.Effect == effect);
            if (burdenOfDisease != null) {
                results.Add((burdenOfDisease, foundConversions));
            } else {
                var conversions = bodIndicatorConversions
                    .Where(c => c.ToIndicator == bodIndicator)
                    .ToList();
                foreach (var conversion in conversions) {
                    foundConversions.Add(conversion);
                    results.AddRange(
                        GetBurdenOfDiseaseBasedOnConversion(
                            burdensOfDisease,
                            bodIndicatorConversions,
                            effect,
                            conversion.FromIndicator,
                            foundConversions
                        )
                    );
                    foundConversions = [];
                }
            }
            return results;
        }

        public List<IBodIndicatorValueModel> GetDerivedBurdensOfDisease(
            ICollection<IBodIndicatorValueModel> bodIndicatorValueModels,
            List<BodIndicatorConversion> bodIndicatorConversions
        ) {
            var results = new List<IBodIndicatorValueModel>();
            var conversionsLookup = bodIndicatorConversions
                .ToLookup(r => r.FromIndicator);
            foreach (var model in bodIndicatorValueModels) {
                if (conversionsLookup.Contains(model.BurdenOfDisease.BodIndicator)) {
                    var conversions = conversionsLookup[model.BurdenOfDisease.BodIndicator];
                    foreach (var conversion in conversions) {
                        BurdenOfDisease derivedBurdenOfDisease;
                        if (model.BurdenOfDisease is DerivedBurdenOfDisease) {
                            var derivedBod = model.BurdenOfDisease as DerivedBurdenOfDisease;
                            if (!derivedBod.Conversions.Contains(conversion)) {
                                derivedBurdenOfDisease = new DerivedBurdenOfDisease() {
                                    BodIndicator = conversion.ToIndicator,
                                    Effect = derivedBod.Effect,
                                    Population = derivedBod.Population,
                                    Value = derivedBod.Value * conversion.Value,
                                    SourceIndicator = derivedBod.SourceIndicator,
                                    Conversions = [.. derivedBod.Conversions.Union([conversion])],
                                };
                                model.BurdenOfDisease = derivedBurdenOfDisease;
                                results.Add(model);

                                // Get recursive records
                                var recursiveRecords = GetDerivedBurdensOfDisease(
                                    [model],
                                    bodIndicatorConversions
                                );
                                results.AddRange(recursiveRecords);
                            }
                        } else {
                            derivedBurdenOfDisease = new DerivedBurdenOfDisease() {
                                BodIndicator = conversion.ToIndicator,
                                Effect = model.BurdenOfDisease.Effect,
                                Population = model.BurdenOfDisease.Population,
                                Value = model.BurdenOfDisease.Value * conversion.Value,
                                SourceIndicator = model.BurdenOfDisease,
                                Conversions = [conversion],
                            };
                            model.BurdenOfDisease = derivedBurdenOfDisease;
                            results.Add(model);

                            // Get recusrive records
                            var recursiveRecords = GetDerivedBurdensOfDisease(
                                [model],
                                bodIndicatorConversions
                            );
                            results.AddRange(recursiveRecords);
                        }
                    }
                }
            }

            return results;
        }
    }
}
