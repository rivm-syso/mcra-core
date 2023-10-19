using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.OutputGeneration {

    /// <summary>
    /// Reference substance summary record.
    /// </summary>
    public sealed class ReferenceDoseRecord {

        public string Name { get; set; }
        public string Code { get; set; }

        public ReferenceDoseRecord() { }

        public ReferenceDoseRecord(Compound substance) {
            Name = substance.Name;
            Code = substance.Code;
        }
    }
}
