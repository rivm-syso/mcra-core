using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {
    public class ExposuresAndHazardsByAgeRecord {
        public double RiskRatio { get; set; }
        public double Exposure { get; set; }
        public double? Age { get; set; }
    }
}
