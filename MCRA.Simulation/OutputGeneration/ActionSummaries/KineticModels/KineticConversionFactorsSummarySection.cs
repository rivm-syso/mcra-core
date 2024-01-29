using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class KineticConversionFactorsSummarySection : SummarySection {

        public List<KineticConversionFactorSummaryRecord> KineticConversionFactorRecords { get; set; }

        /// <summary>
        /// Summarize kinetic model conversion factors
        /// </summary>
        /// <param name="conversionFactors"></param>
        /// <param name="substances"></param>
        public void Summarize(
            ICollection<KineticConversionFactor> conversionFactors,
            ICollection<Compound> substances
        ) {
            KineticConversionFactorRecords = new List<KineticConversionFactorSummaryRecord>();
            foreach (var substance in substances) {
                if (conversionFactors.Any()) {
                    foreach (var record in conversionFactors) {
                        var result = new KineticConversionFactorSummaryRecord() {
                            SubstanceCodeFrom = substance.Code,
                            SubstanceNameFrom = substance.Name,
                            BiologicalMatrixFrom = record.BiologicalMatrixFrom != BiologicalMatrix.Undefined
                                ? record.BiologicalMatrixFrom.GetDisplayName() : null,
                            ExposureRouteFrom = record.ExposureRouteFrom != ExposureRoute.Undefined
                                ? record.ExposureRouteFrom.GetShortDisplayName() : null,
                            DoseUnitFrom = record.DoseUnitFrom.GetShortDisplayName(),
                            ExpressionTypeFrom = record.ExpressionTypeFrom != ExpressionType.None
                                ? record.ExpressionTypeFrom.GetDisplayName() : null,
                            SubstanceCodeTo = record.SubstanceTo.Code,
                            SubstanceNameTo = record.SubstanceTo.Name,
                            BiologicalMatrixTo = record.BiologicalMatrixTo != BiologicalMatrix.Undefined
                                ? record.BiologicalMatrixTo.GetDisplayName() : null,
                            ExposureRouteTo = record.ExposureRouteTo != ExposureRoute.Undefined
                                ? record.ExposureRouteTo.GetShortDisplayName() : null,
                            DoseUnitTo = record.DoseUnitTo.GetShortDisplayName(),
                            ExpressionTypeTo = record.ExpressionTypeTo != ExpressionType.None
                                ? record.ExpressionTypeTo.GetDisplayName() : null,
                            ConversionFactor = record.ConversionFactor
                        };
                        KineticConversionFactorRecords.Add(result);
                    }
                }
            }
        }
    }
}
