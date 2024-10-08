using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class KineticConversionFactorSection : SummarySection {
        public override bool SaveTemporaryData => true;
        public List<KineticConversionFactorRecord> Records { get; set; }

        public void Summarize(
            IDictionary<(ExposurePathType, Compound), double> kineticConversionFactors,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound
        ) {
            Records = kineticConversionFactors.Select(c => new KineticConversionFactorRecord() {
                SubstanceCode = c.Key.Item2.Code,
                SubstanceName = c.Key.Item2.Name,
                ExposureRoute = c.Key.Item1.GetDisplayName(),
                KineticConversionFactor = c.Value,
                KineticConversionFactors = new List<double>(),
                UncertaintyLowerBound = uncertaintyLowerBound,
                UncertaintyUpperBound = uncertaintyUpperBound
            }).ToList();
        }

        public void SummarizeUncertainty(IDictionary<(ExposurePathType, Compound), double> kineticConversionFactors) {
            var records = kineticConversionFactors.Select(c => new KineticConversionFactorRecord() {
                SubstanceCode = c.Key.Item2.Code,
                SubstanceName = c.Key.Item2.Name,
                ExposureRoute = c.Key.Item1.GetDisplayName(),
                KineticConversionFactor = c.Value,
            }).ToList();
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
