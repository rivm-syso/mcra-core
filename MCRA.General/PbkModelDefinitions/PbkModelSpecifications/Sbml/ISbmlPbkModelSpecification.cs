namespace MCRA.General.PbkModelDefinitions.PbkModelSpecifications.Sbml {
    public interface ISbmlPbkModelSpecification : IPbkModelSpecification {
        Dictionary<ExposureRoute, (SbmlPbkModelSpecies Species, bool IsFallback)> GetRouteInputSpecies(bool fallbackSystemic = false);
    }
}