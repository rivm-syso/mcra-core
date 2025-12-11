using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Simulation.Calculators.BodIndicatorModels {
    public class BodConversionsCalculator {

        /// <summary>
        /// Recursively computes derived burden of disease indicator models
        /// based on the specified conversions.
        /// </summary>
        /// <param name="bodIndicatorModels"></param>
        /// <param name="conversionsLookup"></param>
        /// <returns></returns>
        public List<DerivedBodIndicatorModel> Compute(
            ICollection<IBodIndicatorModel> bodIndicatorModels,
            ILookup<BodIndicator, BodIndicatorConversion> conversionsLookup
        ) {
            var results = new List<DerivedBodIndicatorModel>();
            foreach (var model in bodIndicatorModels) {
                if (conversionsLookup.Contains(model.BodIndicator)) {
                    var conversions = conversionsLookup[model.BodIndicator];
                    foreach (var conversion in conversions) {
                        if (model is DerivedBodIndicatorModel) {
                            var derivedBod = model as DerivedBodIndicatorModel;
                            if (!derivedBod.Conversions.Contains(conversion)) {
                                var derivedBodModel = new DerivedBodIndicatorModel() {
                                    BurdenOfDisease = new BurdenOfDisease() {
                                        BodIndicator = conversion.ToIndicator,
                                        Effect = derivedBod.BurdenOfDisease.Effect,
                                        Population = derivedBod.BurdenOfDisease.Population,
                                        Value = model.GetBodIndicatorValue() * conversion.Value,
                                    },
                                    SourceIndicator = derivedBod.SourceIndicator,
                                    Conversions = [.. derivedBod.Conversions.Union([conversion])],
                                };
                                results.Add(derivedBodModel);

                                // Get recursive records for derived model
                                var recursiveRecords = Compute(
                                    [derivedBodModel],
                                    conversionsLookup
                                );
                                results.AddRange(recursiveRecords);
                            }
                        } else {
                            var derivedBodModel = new DerivedBodIndicatorModel() {
                                BurdenOfDisease = new BurdenOfDisease() {
                                    BodIndicator = conversion.ToIndicator,
                                    Effect = model.Effect,
                                    Population = model.Population,
                                    Value = model.GetBodIndicatorValue() * conversion.Value,
                                },
                                SourceIndicator = model,
                                Conversions = [conversion],
                            };
                            results.Add(derivedBodModel);

                            // Get recursive records for derived model
                            var recursiveRecords = Compute(
                                [derivedBodModel],
                                conversionsLookup
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
