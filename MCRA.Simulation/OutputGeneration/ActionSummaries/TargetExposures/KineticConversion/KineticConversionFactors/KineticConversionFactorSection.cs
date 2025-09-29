using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.KineticConversionFactorCalculation;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class KineticConversionFactorSection : SummarySection {
        public override bool SaveTemporaryData => true;
        public List<KineticConversionFactorRecord> Records { get; set; }

        public void Summarize(
            ICollection<KineticConversionFactorResultRecord> kineticConversionFactors,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound
        ) {
            Records = kineticConversionFactors
                .Select(c => new KineticConversionFactorRecord() {
                    SubstanceCode = c.Substance.Code,
                    SubstanceName = c.Substance.Name,
                    ExposureRoute = c.ExposureRoute.GetDisplayName(),
                    KineticConversionFactor = c.Factor,
                    KineticConversionFactors = [],
                    UncertaintyLowerBound = uncertaintyLowerBound,
                    UncertaintyUpperBound = uncertaintyUpperBound
                })
                .ToList();
        }

        public void SummarizeUncertainty(
            ICollection<KineticConversionFactorResultRecord> kineticConversionFactors
        ) {
            var records = kineticConversionFactors
                .Select(c => new KineticConversionFactorRecord() {
                    SubstanceCode = c.Substance.Code,
                    SubstanceName = c.Substance.Name,
                    ExposureRoute = c.ExposureRoute.GetDisplayName(),
                    KineticConversionFactor = c.Factor,
                })
                .ToList();
            updateFactors(records);
        }

        private void updateFactors(List<KineticConversionFactorRecord> records) {
            foreach (var record in Records) {
                var factor = records.FirstOrDefault(c => c.ExposureRoute == record.ExposureRoute && c.SubstanceCode == record.SubstanceCode)?.KineticConversionFactor ?? 0;
                record.KineticConversionFactors.Add(factor);
            }
        }
    }
}
