using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.KineticConversionCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.KineticConversionFactorCalculation;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class KineticConversionSummarySection : SummarySection {
        public override bool SaveTemporaryData => true;
        public List<KineticConversionSummaryRecord> Records { get; set; }

        public void Summarize(
            ICollection<ExposureRoute> exposureRoutes,
            ICollection<TargetUnit> targetUnits,
            ICollection<Compound> activeSubstances,
            IDictionary<Compound, IKineticConversionCalculator> kineticConversionCalculators,
            ICollection<KineticConversionFactorResultRecord> kineticConversionFactors,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound
        ) {
            var records = new List<KineticConversionSummaryRecord>();
            var conversionModelsLookup = kineticConversionFactors
                .ToDictionary(r => (r.Substance, r.ExposureRoute, r.TargetUnit.Target));
            foreach (var substance in activeSubstances) {
                var model = kineticConversionCalculators.ContainsKey(substance)
                    ? kineticConversionCalculators[substance] : null;
                foreach (var exposureRoute in exposureRoutes) {
                    foreach (var targetUnit in targetUnits) {
                        var conversionFactor = conversionModelsLookup
                            .ContainsKey((substance, exposureRoute, targetUnit.Target))
                                ? conversionModelsLookup[(substance, exposureRoute, targetUnit.Target)]
                                : null;
                        var record = new KineticConversionSummaryRecord() {
                            SubstanceCode = substance.Code,
                            SubstanceName = substance.Name,
                            ModelType = model.ModelType.GetDisplayName(),
                            ExposureRoute = exposureRoute.GetDisplayName(),
                            KineticConversionFactor = conversionFactor?.Factor,
                            KineticConversionFactors = [],
                            UncertaintyLowerBound = uncertaintyLowerBound,
                            UncertaintyUpperBound = uncertaintyUpperBound
                        };
                        records.Add(record);
                    }
                }
            }

            Records = records;
        }

        public void SummarizeUncertainty(
            ICollection<KineticConversionFactorResultRecord> kineticConversionFactors
        ) {
            var records = kineticConversionFactors
                .Select(c => new KineticConversionSummaryRecord() {
                    SubstanceCode = c.Substance.Code,
                    SubstanceName = c.Substance.Name,
                    ExposureRoute = c.ExposureRoute.GetDisplayName(),
                    KineticConversionFactor = c.Factor,
                })
                .ToList();
            updateFactors(records);
        }

        private void updateFactors(List<KineticConversionSummaryRecord> records) {
            foreach (var record in Records) {
                var factor = records.FirstOrDefault(c => c.ExposureRoute == record.ExposureRoute && c.SubstanceCode == record.SubstanceCode)?.KineticConversionFactor ?? 0;
                record.KineticConversionFactors.Add(factor);
            }
        }
    }
}
