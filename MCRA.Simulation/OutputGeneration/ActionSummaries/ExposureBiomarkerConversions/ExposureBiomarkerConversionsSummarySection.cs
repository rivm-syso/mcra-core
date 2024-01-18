using MCRA.Data.Compiled.Objects;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ExposureBiomarkerConversionsSummarySection : SummarySection {
        public List<ExposureBiomarkerConversionRecord> Records { get; set; }
        public void Summarize(ICollection<ExposureBiomarkerConversion> exposureBiomarkerConversions) {
            Records = exposureBiomarkerConversions.Select(c => new ExposureBiomarkerConversionRecord() {
                SubstanceNameFrom = c.SubstanceFrom.Name,
                SubstanceCodeFrom = c.SubstanceFrom.Code,
                SubstanceNameTo = c.SubstanceTo.Name,
                SubstanceCodeTo = c.SubstanceTo.Code,
                BiologicalMatrix = c.BiologicalMatrix.GetDisplayName(),
                ExpressionTypeFrom = c.ExpressionTypeFrom.GetDisplayName(),
                ExpressionTypeTo = c.ExpressionTypeTo.GetDisplayName(),
                UnitFrom = c.UnitFrom.GetShortDisplayName(),
                UnitTo = c.UnitTo.GetShortDisplayName(),
                Factor = c.Factor,
                Distribution = c.Distribution.GetDisplayName(),
                VariabilityUpper = c.VariabilityUpper ?? double.NaN,
            }).ToList();
        }
    }
}