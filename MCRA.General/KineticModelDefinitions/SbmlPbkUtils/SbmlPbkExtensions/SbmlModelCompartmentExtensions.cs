using MCRA.Utils.Sbml.Objects;

namespace MCRA.General.Sbml {

    public static class SbmlModelCompartmentExtensions {

        private static readonly Dictionary<string, ExposurePathType> _inputAliases = new(StringComparer.OrdinalIgnoreCase) {
            { @"http://identifiers.org/UBERON:0001555", ExposurePathType.Oral },
            { @"http://identifiers.org/SBO:0000247", ExposurePathType.Dermal }
        };

        public static bool IsInputCompartment(this SbmlModelCompartment compartment) {
            var result = compartment.IsOralInputCompartment() || compartment.IsDermalInputCompartment();
            return result;
        }

        public static ExposurePathType GetExposurePathType(this SbmlModelCompartment compartment) {
            var resource = compartment.BqbIsResources?
                .FirstOrDefault(r => _inputAliases.ContainsKey(r));
            if (resource != null) {
                var result = _inputAliases[resource];
                return result;
            }
            return ExposurePathType.Undefined;
        }

        public static bool IsOralInputCompartment(this SbmlModelCompartment compartment) {
            var oralDoseCompartmentIdentifiers = _inputAliases
                .Where(r => r.Value == ExposurePathType.Oral)
                .Select(r => r.Key)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            var result = compartment.BqbIsResources?
                .Any(r => oralDoseCompartmentIdentifiers.Contains(r)) ?? false;
            return result;
        }

        public static bool IsDermalInputCompartment(this SbmlModelCompartment compartment) {
            var dermalDoseCompartmentIdentifiers = _inputAliases
                .Where(r => r.Value == ExposurePathType.Oral)
                .Select(r => r.Key)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            var result = compartment.BqbIsResources?
                .Any(r => dermalDoseCompartmentIdentifiers.Contains(r)) ?? false;
            return result;
        }
    }
}
