namespace MCRA.General.OpexProductDefinitions.Dto {
    /// <summary>
    /// OPEX definition for absorptions with content that should exactly match the OPEX absorption.csv file.
    /// </summary>
    public record Absorption(
    ) {
        // NOTE: all properties are in lowerCamelCase, to get the correct casing when used in the OPEX R script
        public string use{ get; init; }
        public string substance{ get; init; }
        public string crop{ get; init; }
        public double productConcentration { get; init; }
        public double dilutionConcentration { get; init; }
        public bool defaultDermal { get; init; }
        public int apDermal { get; init; }
        public int dfr0 { get; init; }
        public int dt50 { get; init; }
        public bool worker { get; init; }
        public int dfrSpecWorker { get; init; }
        public int dt50FoliarWorker { get; init; }
        public int dt50AirWorker { get; init; }
        public int dt50SoilWorker { get; init; }
        public bool res { get; init; }
        public int dfrSpecRes { get; init; }
        public int dt50Res { get; init; }
        public bool bys { get; init; }
        public int dfrSpecBys { get; init; }
        public int dt50Bys { get; init; }
        public string idSubstance { get; init; }
        public string idCrop { get; init; }
    };
}
