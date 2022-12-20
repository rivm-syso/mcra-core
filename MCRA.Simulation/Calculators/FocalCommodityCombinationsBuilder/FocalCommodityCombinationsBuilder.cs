using MCRA.Data.Compiled.Objects;
using MCRA.General.Action.Settings.Dto;
using System.Collections.Generic;

namespace MCRA.Simulation.Calculators.FocalCommodityCombinationsBuilder {
    public class FocalCommodityCombinationsBuilder {

        public static ICollection<(Food Food, Compound Substance)> Create(
            ICollection<FocalFoodDto> selectedCodes,
            IDictionary<string, Food> foods,
            IDictionary<string, Compound> substances
        ) {
            var result = new HashSet<(Food, Compound)>();

            foreach (var selectedCode in selectedCodes) {
                if (!string.IsNullOrEmpty(selectedCode.CodeFood) && !string.IsNullOrEmpty(selectedCode.CodeSubstance)) {
                    // Food and substance are specified
                    if (foods.TryGetValue(selectedCode.CodeFood, out var food)
                        && substances.TryGetValue(selectedCode.CodeSubstance, out var substance)
                    ) {
                        // Both food and substance are in the provided dictionaries
                        result.Add((food, substance));
                    }
                } else if (!string.IsNullOrEmpty(selectedCode.CodeFood)) {
                    // Only code food is specified, all substances should be selected for this food
                    if (foods.TryGetValue(selectedCode.CodeFood, out var food)) {
                        foreach (var substance in substances.Values) {
                            result.Add((food, substance));
                        }
                    }
                } else if (!string.IsNullOrEmpty(selectedCode.CodeSubstance)) {
                    // Only code substance is specified, all foods should be selected for this substance
                    if (substances.TryGetValue(selectedCode.CodeSubstance, out var substance)) {
                        foreach (var food in foods.Values) {
                            result.Add((food, substance));
                        }
                    }
                }
            }

            return result;
        }

    }
}
