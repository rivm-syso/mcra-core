using MCRA.Data.Compiled.Objects;
using MCRA.General;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class FoodsSummarySection : SummarySection {

        public List<FoodsSummaryRecord> Records { get; set; }

        public void Summarize(ICollection<Food> foods) {
            Records = foods
                .Select(c => {
                    return new FoodsSummaryRecord() {
                        Code = c.Code,
                        Name = c.Name,
                        CodeParent = c.Parent?.Code,
                        BaseFoodCode = c.BaseFood?.Code,
                        BaseFoodName = c.BaseFood?.Name,
                        TreatmentCodes = c.FoodFacets != null ? string.Join("$", c.FoodFacets.Select(r => r.FullCode)) : null,
                        TreatmentNames = c.FoodFacets != null ? string.Join("$", c.FoodFacets.Select(r => r.Name)) : null,
                        DefaultUnitWeightRacQualifiedValue = c.GetDefaultUnitWeight(UnitWeightValueType.UnitWeightRac),
                        DefaultUnitWeightEpQualifiedValue = c.GetDefaultUnitWeight(UnitWeightValueType.UnitWeightEp),
                        LocationUnitWeightsRacLocations = c.FoodUnitWeights?
                            .Where(r => r.ValueType == UnitWeightValueType.UnitWeightRac)
                            .Select(r => r.Location)
                            .ToList(),
                        LocationUnitWeightsRacValues = c.FoodUnitWeights?
                            .Where(r => r.ValueType == UnitWeightValueType.UnitWeightRac)
                            .Select(r => r.QualifiedValue)
                            .ToList(),
                        LocationUnitWeightsEpLocations = c.FoodUnitWeights?
                            .Where(r => r.ValueType == UnitWeightValueType.UnitWeightEp)
                            .Select(r => r.Location)
                            .ToList(),
                        LocationUnitWeightsEpValues = c.FoodUnitWeights?
                            .Where(r => r.ValueType == UnitWeightValueType.UnitWeightEp)
                            .Select(r => r.QualifiedValue)
                            .ToList(),
                    };
                })
                .OrderBy(c => c.Name, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
    }
}
