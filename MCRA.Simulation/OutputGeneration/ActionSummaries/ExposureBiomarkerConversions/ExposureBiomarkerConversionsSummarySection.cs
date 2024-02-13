using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ExposureBiomarkerConversionsSummarySection : SummarySection {
        public List<ExposureBiomarkerConversionRecord> Records { get; set; }
        public void Summarize(ICollection<ExposureBiomarkerConversion> exposureBiomarkerConversions) {
            if (exposureBiomarkerConversions.Any()) {
                Records = exposureBiomarkerConversions.Select(c => { 
                    var isAgeLower = c.EBCSubgroups.Any(c => c.AgeLower != null);
                    var isGender = c.EBCSubgroups.Any(c => c.Gender != GenderType.Undefined);
                    return new ExposureBiomarkerConversionRecord() {
                        SubstanceNameFrom = c.SubstanceFrom.Name,
                        SubstanceCodeFrom = c.SubstanceFrom.Code,
                        SubstanceNameTo = c.SubstanceTo.Name,
                        SubstanceCodeTo = c.SubstanceTo.Code,
                        BiologicalMatrix = c.BiologicalMatrix.GetDisplayName(),
                        ExpressionTypeFrom = c.ExpressionTypeFrom.GetDisplayName(),
                        ExpressionTypeTo = c.ExpressionTypeTo.GetDisplayName(),
                        UnitFrom = c.UnitFrom.GetShortDisplayName(),
                        UnitTo = c.UnitTo.GetShortDisplayName(),
                        Factor = c.ConversionFactor,
                        Distribution = c.Distribution.GetDisplayName(),
                        VariabilityUpper = c.VariabilityUpper ?? double.NaN,
                        IsAgeLower = isAgeLower,
                        IsGender = isGender,
                        Both = isAgeLower && isGender
                    };
                })
                .ToList();
            }
        }
    }
}