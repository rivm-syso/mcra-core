using MCRA.General;
using System;

namespace MCRA.Simulation.OutputGeneration {
    public class OutputData : IOutput {
        public int id { get; set; }
        public int idTask { get; set; }
        public string Description { get; set; }
        public DateTime StartExecution { get; set; }
        public DateTime BuildDate { get; set; }
        public string BuildVersion { get; set; }
        public DateTime DateCreated { get; set; }
        public byte[] SectionHeaderData { get; set; }
        public byte[] OutputSummary { get; set; }
    }
}
