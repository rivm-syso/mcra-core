namespace MCRA.General.Action.Settings.Dto {
    public class LocationSubsetDefinitionDto {

        public virtual List<string> LocationSubset { get; set; } = new List<string>();

        public virtual bool AlignSubsetWithPopulation { get; set; }

        public virtual bool IncludeMissingValueRecords { get; set; }

    }
}
