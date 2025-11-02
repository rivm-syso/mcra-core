using MCRA.Utils.Sbml.Objects;

namespace MCRA.General.Sbml {

    public static class SbmlModelExtensions {

        public static SubstanceAmountUnit GetSpeciesAmountUnit(
            this SbmlModel model,
            string id
        ) {
            var unitsDictionary = model.UnitDefinitions;
            var species = model.Species.FirstOrDefault(r => r.Id == id);
            if (model.UnitDefinitions.TryGetValue(species.SubstanceUnits, out var value)) {
                return value.ToSubstanceAmountUnit();
            }
            return SubstanceAmountUnit.Undefined;
        }

        public static ConcentrationMassUnit GetCompartmentVolumeUnit(
            this SbmlModel model,
            string id
        ) {
            var unitsDictionary = model.UnitDefinitions;
            var compartment = model.Compartments.FirstOrDefault(r => r.Id == id);
            if (model.UnitDefinitions.TryGetValue(compartment.Units, out var value)) {
                return value.ToConcentrationMassUnit();
            }
            return ConcentrationMassUnit.Undefined;
        }
    }
}
