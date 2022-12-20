using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;
using MCRA.General;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Simulation.Calculators.SingleValueConsumptionsCalculation {
    public sealed class SingleValueConsumptionsCollectionBuilder {

        /// <summary>
        /// Creates a single value food consumptions from the provided collection
        /// of raw single value consumptions, applying the required filters and unit
        /// conversions.
        /// </summary>
        /// <param name="population"></param>
        /// <param name="populationConsumptionSingleValues"></param>
        /// <param name="targetConsumptionIntakeUnit"></param>
        /// <param name="targetBodyWeightUnit"></param>
        /// <returns></returns>
        public ICollection<SingleValueConsumptionModel> Create(
            Population population,
            ICollection<PopulationConsumptionSingleValue> populationConsumptionSingleValues,
            ConsumptionIntakeUnit targetConsumptionIntakeUnit,
            BodyWeightUnit targetBodyWeightUnit
        ) {
            if (targetBodyWeightUnit != BodyWeightUnit.kg) {
                throw new NotImplementedException("Target body weight unit should be kg");
            }
            if (population != null && population.BodyWeightUnit != targetBodyWeightUnit) {
                throw new NotImplementedException("Population body weight unit should be kg (target)");
            }
            if (populationConsumptionSingleValues.Any(c => c.ConsumptionUnit.GetConsumptionUnit() != ConsumptionUnit.g)) {
                throw new NotImplementedException("Single value consumptions should be in g/day");
            }
            var populationBodyWeight = population?.NominalBodyWeight ?? double.NaN;
            var result = populationConsumptionSingleValues
                .GroupBy(c => (c.Food, c.Food.ProcessingFacetCode()))
                .Select(g => {
                    return new SingleValueConsumptionModel() {
                        Food = g.Key.Food.BaseFood ?? g.Key.Food,
                        ProcessingCorrectionFactor = double.NaN,
                        ProcessingTypes = g.Key.Food.ProcessingTypes,
                        MeanConsumption = getValue(g, ConsumptionValueType.MeanConsumption, targetConsumptionIntakeUnit, populationBodyWeight),
                        LargePortion = getValue(g, ConsumptionValueType.LargePortion, targetConsumptionIntakeUnit, populationBodyWeight),
                        Percentiles = getPercentiles(g, targetConsumptionIntakeUnit, populationBodyWeight),
                        BodyWeight = (population?.NominalBodyWeight != null && g.Any(r => !r.ConsumptionUnit.IsPerPerson()))
                            ? (double)population.NominalBodyWeight
                            : double.NaN
                    };
                })
                .ToList();
            return result;
        }

        private double getValue(
            IEnumerable<PopulationConsumptionSingleValue> values,
            ConsumptionValueType valueType,
            ConsumptionIntakeUnit consumptionUnit,
            double nominalBodyWeight
        ) {
            if (values != null) {
                var record = values.FirstOrDefault(r => r.ValueType == valueType);
                if (record != null) {
                    var unitConversionFactor = record.ConsumptionUnit.GetTargetUnitConversionFactor(consumptionUnit, nominalBodyWeight);
                    return unitConversionFactor * record.ConsumptionAmount;
                }
            }
            return double.NaN;
        }

        private List<(double, double)> getPercentiles(
            IEnumerable<PopulationConsumptionSingleValue> values,
            ConsumptionIntakeUnit consumptionUnit,
            double nominalBodyWeight
        ) {
            var percentiles = new List<(double, double)>();
            if (values != null) {
                var percentileValueRecords = values.Where(r => r.ValueType == ConsumptionValueType.Percentile || r.ValueType == ConsumptionValueType.LargePortion).ToList();
                foreach (var record in percentileValueRecords) {
                    if (record.Percentile != null) {
                        var unitConversionFactor = record.ConsumptionUnit.GetTargetUnitConversionFactor(consumptionUnit, nominalBodyWeight);
                        var value = unitConversionFactor * record.ConsumptionAmount;
                        percentiles.Add(((double)record.Percentile, value));
                    } else if (record.ValueType == ConsumptionValueType.LargePortion) {
                        var unitConversionFactor = record.ConsumptionUnit.GetTargetUnitConversionFactor(consumptionUnit, nominalBodyWeight);
                        var value = unitConversionFactor * record.ConsumptionAmount;
                        percentiles.Add((97.5, value));
                    }
                }
            }
            return percentiles;
        }
    }
}
