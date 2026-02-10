using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.PbkModelDefinitions.PbkModelSpecifications.DeSolve;
using MCRA.General.PbkModelDefinitions.PbkModelSpecifications.Sbml;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Sbml.Objects;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class PbkModelDefinitionSummarySection : SummarySection {

        public string ModelCode { get; set; }
        public string ModelName { get; set; }
        public List<PbkModelDefinitionSpeciesSummaryRecord> Records { get; set; }
        public SbmlModel SbmlModel { get; set; }

        public void Summarize(PbkModelDefinition modelDefinition) {
            ModelCode = modelDefinition.IdModelDefinition;
            ModelName = modelDefinition.Name;

            var records = new List<PbkModelDefinitionSpeciesSummaryRecord>();
            if (modelDefinition.KineticModelDefinition is SbmlPbkModelSpecification) {
                var sbmlModelSpecification = modelDefinition.KineticModelDefinition as SbmlPbkModelSpecification;
                foreach (var species in sbmlModelSpecification.Species) {
                    var matrix = species.Compartment.GetBiologicalMatrix();
                    var record = new PbkModelDefinitionSpeciesSummaryRecord() {
                        SpeciesCode = species.Id,
                        SpeciesName = species.Name,
                        CompartmentCode = species.Compartment?.Id,
                        CompartmentName = species.Compartment?.Name,
                        BiologicalMatrix = matrix != BiologicalMatrix.Undefined
                            ? species.Compartment.GetBiologicalMatrix().GetShortDisplayName()
                            : null,
                        CompartmentVolumeUnit = species.Compartment.Unit.GetShortDisplayName(),
                        SpeciesAmountUnit = species.SubstanceAmountUnit.GetShortDisplayName()
                    };
                    records.Add(record);
                }
                SbmlModel = sbmlModelSpecification.SbmlModel;
            } else {
                var deSolveModelSpecification = modelDefinition.KineticModelDefinition as DeSolvePbkModelSpecification;
                foreach (var output in deSolveModelSpecification.Outputs) {
                    var record = new PbkModelDefinitionSpeciesSummaryRecord() {
                        CompartmentCode = output.Id,
                        CompartmentName = output.Description,
                        BiologicalMatrix = output.BiologicalMatrix != BiologicalMatrix.Undefined
                            ? output.BiologicalMatrix.ToString()
                            : null,
                    };
                    records.Add(record);
                }
            }
            Records = records;
        }
    }
}
