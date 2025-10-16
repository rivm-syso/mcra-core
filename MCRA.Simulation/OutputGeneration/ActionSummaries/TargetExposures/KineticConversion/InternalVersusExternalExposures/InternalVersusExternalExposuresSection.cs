using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.AggregateExposures;
using MCRA.Utils.Collections;

namespace MCRA.Simulation.OutputGeneration {
    public class InternalVersusExternalExposuresSection : SummarySection {
        public override bool SaveTemporaryData => true;

        public List<TargetUnit> TargetUnits { get; set; }
        public ExposureUnitTriple ExternalExposureUnit { get; set; }

        public double[] TotalExternalIndividualExposures { get; set; }
        public SerializableDictionary<ExposureTarget, double[]> TargetIndividualExposures { get; set; }

        public string SubstanceName { get; set; }

        public void Summarize(
            Compound substance,
            ICollection<AggregateIndividualExposure> targetExposures,
            List<TargetUnit> targets,
            ExposureUnitTriple externalExposureUnit
        ) {
            SubstanceName = substance.Name;
            ExternalExposureUnit = externalExposureUnit;
            TargetUnits = targets;

            TotalExternalIndividualExposures = targetExposures
                .Select(r => r.GetTotalExternalExposureForSubstance(
                    substance,
                    externalExposureUnit.IsPerUnit
                ))
                .ToArray();
            TargetIndividualExposures = [];
            foreach (var target in targets) {
                TargetIndividualExposures[target.Target] = targetExposures
                    .Select(r => r.GetSubstanceExposure(target.Target, substance))
                    .ToArray();
            }
        }
    }
}
