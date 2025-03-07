namespace MCRA.General.ModuleDefinitions.Settings {
    public partial class TargetExposuresModuleConfig {

        /// <summary>
        /// Returns true when PBK models are used / required as input.
        /// </summary>
        public bool RequirePbkModels => TargetDoseLevelType == TargetLevelType.Internal
            && (InternalModelType == InternalModelType.PBKModelOnly
                || InternalModelType == InternalModelType.PBKModel);

        /// <summary>
        /// Returns true when kinetic conversion factors are used / required as input.
        /// </summary>
        public bool RequireKineticConversionFactors => TargetDoseLevelType == TargetLevelType.Internal
            && (InternalModelType == InternalModelType.ConversionFactorModel
                || InternalModelType == InternalModelType.PBKModel);

        /// <summary>
        /// Returns true when absorption factors are used / required as input.
        /// </summary>
        public bool RequireAbsorptionFactors => TargetDoseLevelType == TargetLevelType.Systemic;

    }
}
