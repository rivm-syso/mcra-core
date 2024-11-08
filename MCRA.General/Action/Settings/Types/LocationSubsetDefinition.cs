namespace MCRA.General.Action.Settings {
    public class LocationSubsetDefinition {

        public virtual List<string> LocationSubset { get; set; } = [];

        public virtual bool AlignSubsetWithPopulation { get; set; }

        public virtual bool IncludeMissingValueRecords { get; set; }

    }
}
