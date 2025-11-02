using MCRA.Utils.Sbml.Objects;

namespace MCRA.General.Sbml {

    public static class SbmlModelExtensions {

        public static TimeUnit GetModelTimeUnit(this SbmlModel model) {
            if (!string.IsNullOrEmpty(model.TimeUnits)) {
                if (!model.UnitDefinitions.TryGetValue(model.TimeUnits, out var unit)) {
                    throw new Exception($"Unit definition not found for model time unit [{model.TimeUnits}].");
                }
                return unit.ToTimeUnit();
            }
            throw new Exception("Model time unit not specified.");
        }

        public static SubstanceAmountUnit GetSpeciesAmountUnit(
            this SbmlModel model,
            string id
        ) {
            if (model.Species.TryGetValue(id, out var species)) {
                if (model.UnitDefinitions.TryGetValue(species.SubstanceUnits, out var value)) {
                    return value.ToSubstanceAmountUnit();
                }
                return SubstanceAmountUnit.Undefined;
            } else {
                throw new Exception($"Cannot determine amount unit of species with id [{id}]: species not found.");
            }
        }

        public static ConcentrationMassUnit GetCompartmentVolumeUnit(
            this SbmlModel model,
            string id
        ) {
            if (model.Compartments.TryGetValue(id, out var compartment)) {
                if (model.UnitDefinitions.TryGetValue(compartment.Units, out var value)) {
                    return value.ToConcentrationMassUnit();
                }
                return ConcentrationMassUnit.Undefined;
            } else {
                throw new Exception($"Cannot determine compartment volume unit of compartment with id [{id}]: compartment not found.");
            }
        }
    }
}
