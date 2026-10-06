using MCRA.General.PbkModelDefinitions.PbkModelSpecifications;

namespace MCRA.General.PbkModelDefinitions.SbmlRoadRunnerWorker {
    /// <summary>A flattened, JSON-serializable snapshot of <see cref="IPbkModelParameterSpecification"/>.</summary>
    public sealed class SbmlPbkModelSpecificationParameterDto {
        public string Id { get; set; }
        public PbkModelParameterType Type { get; set; }
        public double? DefaultValue { get; set; }
        public bool IsInternalParameter { get; set; }
    }
}
