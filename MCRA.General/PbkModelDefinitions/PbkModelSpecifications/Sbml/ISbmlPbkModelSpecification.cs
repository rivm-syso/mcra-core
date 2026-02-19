namespace MCRA.General.PbkModelDefinitions.PbkModelSpecifications.Sbml {
    public interface ISbmlPbkModelSpecification : IPbkModelSpecification {
        Dictionary<ExposureRoute, SbmlPbkModelSpecies> GetRouteInputSpecies(bool fallbackSystemic = false);
    }
}