using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Sbml;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Sbml.Objects;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class PbkModelDefinitionSummarySection : SummarySection {

        public SbmlModel SbmlModel { get; set; }
        public string ModelCode { get; set; }
        public string ModelName { get; set; }
        public List<PbkModelDefinitionSpeciesSummaryRecord> Records { get; set; }

        public void Summarize(PbkModelDefinition modelDefinition) {
            var records = new List<PbkModelDefinitionSpeciesSummaryRecord>();
            var sbmlModel = modelDefinition.KineticModelDefinition.SbmlModel;
            if (sbmlModel != null) {
                var compartmentsLookup = sbmlModel.Compartments.ToDictionary(r => r.Id);
                foreach (var species in sbmlModel.Species) {
                    var hasCompartment = compartmentsLookup.TryGetValue(species.Compartment, out var compartment);
                    var matrix = hasCompartment ? compartment.GetBiologicalMatrix() : BiologicalMatrix.Undefined;
                    var record = new PbkModelDefinitionSpeciesSummaryRecord() {
                        SpeciesCode = species.Id,
                        SpeciesName = species.Name,
                        CompartmentCode = compartment?.Id,
                        CompartmentName = compartment?.Name,
                        BiologicalMatrix = matrix != BiologicalMatrix.Undefined
                            ? compartment.GetBiologicalMatrix().GetShortDisplayName()
                            : null,
                        CompartmentVolumeUnit = sbmlModel
                            .GetCompartmentVolumeUnit(species.Compartment)
                            .GetShortDisplayName(),
                        SpeciesAmountUnit = sbmlModel
                            .GetSpeciesAmountUnit(species.Id)
                            .GetShortDisplayName()
                    };
                    records.Add(record);
                }
            } else {
                foreach (var output in modelDefinition.KineticModelDefinition.Outputs) {
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
            ModelCode = modelDefinition.IdModelDefinition;
            ModelName = modelDefinition.Name;
            SbmlModel = sbmlModel;
            Records = records;
        }
    }
}
