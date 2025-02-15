using MCRA.Utils.Sbml.Objects;

namespace MCRA.General.Sbml {

    public static class SbmlModelParameterExtensions {

        public static PbkModelParameterType GetParameterType(this SbmlModelParameter parameter) {
            var types = parameter.BqmIsResources?
                .Select(r => PbkModelParameterTypeConverter
                    .FromUri(
                        r,
                        defaultType: PbkModelParameterType.Undefined,
                        allowInvalidString: true
                    )
                )
                .Where(r => r != PbkModelParameterType.Undefined)
                .Distinct()
                .ToList();
            if (types?.Count > 1) {
                throw new Exception($"Incorrect annotation for parameter {parameter.Id}: parameter is linked to multiple types.");
            } else if (types?.Count == 1) {
                return types.First();
            }
            return PbkModelParameterType.Undefined;
        }

        public static bool IsOfType(
            this SbmlModelParameter parameter,
            PbkModelParameterType type
        ) {
            // TODO: consider implementation of option for checking recursive
            // based on parent types
            var parameterType = GetParameterType(parameter);
            return parameterType == type;
        }
    }
}
