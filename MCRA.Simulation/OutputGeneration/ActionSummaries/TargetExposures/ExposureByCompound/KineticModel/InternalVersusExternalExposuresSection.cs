using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.AggregateExposures;
using MCRA.Utils.Collections;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {
    public class InternalVersusExternalExposuresSection : SummarySection {
        public override bool SaveTemporaryData => true;

        public List<UncertainDataPointCollection<double>> AbsorptionFactorsPercentiles { get; set; }
        public List<string> AllExposureRoutes { get; set; }
        public List<TargetUnit> TargetUnits { get; set; }
        public ExposureUnitTriple ExternalExposureUnit { get; set; }

        public List<InternalVersusExternalExposureRecord> Records { get; set; }
        public string SubstanceName { get; set; }
        public double UncertaintyLowerLimit { get; set; }
        public double UncertaintyUpperLimit { get; set; }

        public void Summarize(
            Compound substance,
            ICollection<ExposurePathType> exposureRoutes,
            ICollection<AggregateIndividualExposure> targetExposures,
            IDictionary<(ExposurePathType, Compound), double> kineticConversionFactors,
            List<TargetUnit> targets,
            ExposureType exposureType,
            ExposureUnitTriple externalExposureUnit,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound
        ) {
            SubstanceName = substance.Name;
            ExternalExposureUnit = externalExposureUnit;
            UncertaintyLowerLimit = uncertaintyLowerBound;
            UncertaintyUpperLimit = uncertaintyUpperBound;
            TargetUnits = targets;
            var records = new List<InternalVersusExternalExposureRecord>();
            foreach (var item in targetExposures) {
                var internalExposures = new SerializableDictionary<ExposureTarget, double>();
                foreach (var target in targets) {
                    internalExposures.Add(
                        target.Target,
                        item.GetSubstanceExposure(target.Target, substance)
                    );
                }
                var record = new InternalVersusExternalExposureRecord() {
                    ExternalExposure = item.GetTotalExternalExposureForSubstance(
                        substance,
                        externalExposureUnit.IsPerUnit()
                    ),
                    TargetExposure = internalExposures
                };
                records.Add(record);
            }
            Records = records;

            AllExposureRoutes = [];
            AbsorptionFactorsPercentiles = [];
            foreach (var route in exposureRoutes) {
                if (!kineticConversionFactors.TryGetValue((route, substance), out var factor)) {
                    factor = double.NaN;
                }
                var absorptionFactorsPercentile = new UncertainDataPointCollection<double>() {
                    XValues = [0D],
                    ReferenceValues = [factor]
                };
                AbsorptionFactorsPercentiles.Add(absorptionFactorsPercentile);
                AllExposureRoutes.Add(route.GetShortDisplayName());
            }
        }

        public void SummarizeUncertainty(
            IDictionary<(ExposurePathType, Compound), double> kineticConversionFactors,
            Compound substance,
            ICollection<ExposurePathType> exposureRoutes
        ) {
            var counter = 0;
            foreach (var route in exposureRoutes) {
                if (!kineticConversionFactors.TryGetValue((route, substance), out var factor)) {
                    factor = double.NaN;
                }
                AbsorptionFactorsPercentiles[counter].AddUncertaintyValues([factor]);
                counter++;
            }
        }
    }
}
