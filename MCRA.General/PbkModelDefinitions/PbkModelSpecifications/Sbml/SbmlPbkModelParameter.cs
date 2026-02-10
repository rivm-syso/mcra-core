using MCRA.General.PbkModelDefinitions.PbkModelSpecifications;
using MCRA.Utils.Sbml.Objects;

namespace MCRA.General.PbkModelDefinitions.PbkModelSpecifications.Sbml {
    public class SbmlPbkModelParameter : IPbkModelParameterSpecification {

        /// <summary>
        /// The SBML model parameter definition.
        /// </summary>
        public SbmlModelParameter SbmlModelParameter { get; set; }

        public SbmlUnitDefinition SbmlUnit { get; set; }

        /// <summary>
        /// Parameter unit string.
        /// </summary>
        public string Unit => SbmlUnit.GetUnitString();

        /// <summary>
        /// Gets/sets the parameter id.
        /// </summary>
        public string Id => SbmlModelParameter.Id;

        /// <summary>
        /// The name of the parameter.
        /// </summary>
        public string Name => SbmlModelParameter.Name;

        public string Description => Name;

        public PbkModelParameterType Type => GetParameterType();

        /// <summary>
        /// Specifies whether the parameter is a model constant.
        /// </summary>
        public bool IsConstant => SbmlModelParameter.IsConstant;

        /// <summary>
        /// Specifies whether the parameter is an internal variable.
        /// </summary>
        public bool IsVariable => !SbmlModelParameter.IsConstant;

        /// <summary>
        /// Specifies whether the parameter is an internal variable.
        /// </summary>
        public bool IsInternalParameter => IsVariable;

        /// <summary>
        /// Default value of the parameter.
        /// </summary>
        public double? DefaultValue => SbmlModelParameter.DefaultValue;

        /// <summary>
        /// Order number of the parameter.
        /// </summary>
        public int? Order => throw new NotImplementedException();

        /// <summary>
        /// Gets the type of the parameter.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public PbkModelParameterType GetParameterType() {

            // Try get parameter types via MIRIAM biological model qualifier (BQM_IS) annotations
            var types = SbmlModelParameter.BqmIsResources?
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
                throw new Exception($"Incorrect annotation for parameter {Id}: parameter is linked to multiple types.");
            } else if (types?.Count == 1) {
                return types.First();
            }

            // Try get types from alias strings
            var paramType = PbkModelParameterTypeConverter.FromString(
                Id,
                defaultType: PbkModelParameterType.Undefined,
                allowInvalidString: true
            );
            if (paramType != PbkModelParameterType.Undefined) {
                return paramType;
            }

            return PbkModelParameterType.Undefined;
        }

        public bool IsOfType(PbkModelParameterType type) {
            // TODO: consider implementation of option for checking recursive
            // based on parent types
            var parameterType = GetParameterType();
            return parameterType == type;
        }
    }
}
