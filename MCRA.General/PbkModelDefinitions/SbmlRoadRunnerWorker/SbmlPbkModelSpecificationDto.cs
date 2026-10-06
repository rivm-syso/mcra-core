using MCRA.General.PbkModelDefinitions.PbkModelSpecifications;
using MCRA.General.PbkModelDefinitions.PbkModelSpecifications.Sbml;

namespace MCRA.General.PbkModelDefinitions.SbmlRoadRunnerWorker {
    /// <summary>
    /// A flattened, JSON-serializable snapshot of the subset of <see cref="ISbmlPbkModelSpecification"/>
    /// needed to run SBML simulations, so it can be transferred to the out-of-process RoadRunner
    /// worker (which does not share any in-memory domain objects with the sender) as part of the
    /// <see cref="WorkerInitializationDto"/> payload.
    /// </summary>
    public sealed class SbmlPbkModelSpecificationDto {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsLifetimeModel { get; set; }
        public List<SbmlPbkModelSpecificationParameterDto> Parameters { get; set; } = new();

        /// <summary>Mirrors <see cref="IPbkModelSpecification.GetParameterDefinitionByType"/>.</summary>
        public SbmlPbkModelSpecificationParameterDto GetParameterDefinitionByType(PbkModelParameterType paramType) {
            return Parameters.FirstOrDefault(p => p.Type == paramType);
        }

        /// <summary>Builds a serializable snapshot from a PBK model specification.</summary>
        public static SbmlPbkModelSpecificationDto FromSpecification(ISbmlPbkModelSpecification specification) {
            if (specification == null) {
                return null;
            }
            return new SbmlPbkModelSpecificationDto {
                Id = specification.Id,
                Name = specification.Name,
                Description = specification.Description,
                IsLifetimeModel = specification.IsLifetimeModel(),
                Parameters = specification.GetParameters()
                    .Select(p => new SbmlPbkModelSpecificationParameterDto {
                        Id = p.Id,
                        Type = p.Type,
                        DefaultValue = p.DefaultValue,
                        IsInternalParameter = p.IsInternalParameter
                    })
                    .ToList()
            };
        }
    }
}
