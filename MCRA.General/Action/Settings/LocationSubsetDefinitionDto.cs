namespace MCRA.General.Action.Settings {
    public class LocationSubsetDefinitionDto {

        public virtual List<string> LocationSubset { get; set; } = new();

        public virtual bool AlignSubsetWithPopulation { get; set; }

        public virtual bool IncludeMissingValueRecords { get; set; }

    }
}
