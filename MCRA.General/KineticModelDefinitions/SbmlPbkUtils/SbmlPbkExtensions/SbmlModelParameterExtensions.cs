using MCRA.Utils.Sbml.Objects;

namespace MCRA.General.Sbml {

    public static class SbmlModelParameterExtensions {

        public static bool IsBodyWeightParameter(this SbmlModelParameter parameter) {
            var result = parameter.BqbIsResources?
                .Any(r => r.EndsWith("NCIT_C81328", StringComparison.OrdinalIgnoreCase)) ?? false;
            return result;
        }

        public static bool IsBodySurfaceAreaParameter(this SbmlModelParameter parameter) {
            var result = parameter.BqbIsResources?
                .Any(r => r.EndsWith("NCIT_C25157", StringComparison.OrdinalIgnoreCase)) ?? false;
            return result;
        }
    }
}
