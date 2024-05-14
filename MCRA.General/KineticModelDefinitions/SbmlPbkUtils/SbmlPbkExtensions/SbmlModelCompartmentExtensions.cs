using MCRA.Utils.Sbml.Objects;

namespace MCRA.General.Sbml {

    public static class SbmlModelCompartmentExtensions {

        private static readonly Dictionary<string, ExposurePathType> _inputAliases = new(StringComparer.OrdinalIgnoreCase) {
            { @"http://identifiers.org/UBERON:0001555", ExposurePathType.Oral },
            { @"http://identifiers.org/UBERON:0002097", ExposurePathType.Dermal },
            // TODO SBML PBK: now using venous blood for inhalation, should be Air/Lungs
            // (http://purl.obolibrary.org/obo/NCIT_C150891), but this does not
            // seems to work for the EuroMix SBML model (probably because it needs
            // continuous dosing events).
            { @"http://identifiers.org/UBERON:0013755", ExposurePathType.Inhalation }
        };

        public static bool IsInputCompartment(this SbmlModelCompartment compartment) {
            var result = compartment.IsOralInputCompartment() 
                || compartment.IsDermalInputCompartment() 
                || compartment.IsInhalationInputCompartment();
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
                .Where(r => r.Value == ExposurePathType.Dermal)
                .Select(r => r.Key)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            var result = compartment.BqbIsResources?
                .Any(r => dermalDoseCompartmentIdentifiers.Contains(r)) ?? false;
            // TODO SBML PBK: identify (main) dermal input compartment via annotations
            return result && compartment.Id.Equals("Skin_sc_e");
        }

        public static bool IsInhalationInputCompartment(this SbmlModelCompartment compartment) {
            var inhalationDoseCompartmentIdentifiers = _inputAliases
                .Where(r => r.Value == ExposurePathType.Inhalation)
                .Select(r => r.Key)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            var result = compartment.BqbIsResources?
                .Any(r => inhalationDoseCompartmentIdentifiers.Contains(r)) ?? false;
            return result;
        }
    }
}
