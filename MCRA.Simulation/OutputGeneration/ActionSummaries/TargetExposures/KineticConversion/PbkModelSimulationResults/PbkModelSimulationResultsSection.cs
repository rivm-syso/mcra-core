using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.OutputGeneration {
    public class PbkModelSimulationResultsSection : SummarySection {
        public override bool SaveTemporaryData => true;

        public List<PbkModelSimulationSummaryRecord> KineticModelRecords { get; set; } = [];

        public void Summarize(
            Compound substance,
            KineticModelInstance kineticModelInstance,
            ICollection<ExposureRoute> routes,
            List<TargetUnit> targets
        ) {
            var targetUnit = targets.FirstOrDefault();
            var kineticModelRecord = new PbkModelSimulationSummaryRecord() {
                ModelCode = kineticModelInstance.KineticModelDefinition.Id,
                ModelName = kineticModelInstance.KineticModelDefinition.Name,
                ModelInstanceCode = kineticModelInstance.IdModelInstance,
                SubstanceCode = substance.Code,
                SubstanceName = substance.Name,
                DoseUnit = string.Join(", ", kineticModelInstance.KineticModelDefinition.Forcings
                     .Select(r => r.DoseUnit.GetShortDisplayName()).Distinct()),
                Routes = string.Join(", ", routes.Select(c => c.GetShortDisplayName())),
                Output = targetUnit.Target.GetDisplayName(),
                OutputUnit = targetUnit.GetShortDisplayName(),
                TimeUnit = kineticModelInstance.KineticModelDefinition.TimeScale.GetShortDisplayName()
            };
            KineticModelRecords.Add(kineticModelRecord);
        }
    }
}
