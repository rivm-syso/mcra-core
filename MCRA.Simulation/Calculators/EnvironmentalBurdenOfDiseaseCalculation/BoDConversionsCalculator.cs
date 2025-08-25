using System.Linq;
using CommandLine;
using MCRA.Data.Compiled.Objects;
using MCRA.General;

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

        public List<BurdenOfDisease> GetDerivedBurdensOfDisease(
            List<BurdenOfDisease> burdensOfDisease,
            List<BodIndicatorConversion> bodIndicatorConversions
        ) {
            var results = new List<BurdenOfDisease>();
            var conversionsLookup = bodIndicatorConversions
                .ToLookup(r => r.FromIndicator);
            foreach (var bod in burdensOfDisease) {
                if (conversionsLookup.Contains(bod.BodIndicator)) {
                    var conversions = conversionsLookup[bod.BodIndicator];
                    foreach (var conversion in conversions) {
                        BurdenOfDisease derivedBurdenOfDisease;
                        if (bod is DerivedBurdenOfDisease) {
                            var derivedBod = bod as DerivedBurdenOfDisease;
                            if (!derivedBod.Conversions.Contains(conversion)) {
                                derivedBurdenOfDisease = new DerivedBurdenOfDisease() {
                                    BodIndicator = conversion.ToIndicator,
                                    Effect = derivedBod.Effect,
                                    Population = derivedBod.Population,
                                    Value = derivedBod.Value * conversion.Value,
                                    SourceIndicator = derivedBod.SourceIndicator,
                                    Conversions = [.. derivedBod.Conversions.Union([conversion])],
                                };
                                results.Add(derivedBurdenOfDisease);

                                // Get recusrive records
                                var recursiveRecords = GetDerivedBurdensOfDisease(
                                    [derivedBurdenOfDisease],
                                    bodIndicatorConversions
                                );
                                results.AddRange(recursiveRecords);
                            }
                        } else {
                            derivedBurdenOfDisease = new DerivedBurdenOfDisease() {
                                BodIndicator = conversion.ToIndicator,
                                Effect = bod.Effect,
                                Population = bod.Population,
                                Value = bod.Value * conversion.Value,
                                SourceIndicator = bod,
                                Conversions = [conversion],
                            };
                            results.Add(derivedBurdenOfDisease);

                            // Get recusrive records
                            var recursiveRecords = GetDerivedBurdensOfDisease(
                                [derivedBurdenOfDisease],
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
