using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class IndividualConcentrationCorrelationsBySubstanceRecord {

        [DisplayName("Substance name")]
        public string SubstanceName { get; set; }

        [DisplayName("Substance code")]
        public string SubstanceCode { get; set; }

        [Display(AutoGenerateField = false)]
        public List<HbmVsModelledIndividualDayConcentrationRecord> MonitoringVersusModelExposureRecords { get; set; }

        [DisplayName("Number of records")]
        public int TotalRecords {
            get {
                return MonitoringVersusModelExposureRecords.Count;
            }
        }

        [DisplayName("Both zero")]
        public int AllZeros {
            get {
                return MonitoringVersusModelExposureRecords.Count(r => r.ModelledExposure == 0 && r.MonitoringConcentration == 0);
            }
        }

        [DisplayName("Positive monitoring only")]
        public int OnlyPositiveMonitoringConcentrations {
            get {
                return MonitoringVersusModelExposureRecords.Count(r => r.ModelledExposure == 0 && r.MonitoringConcentration > 0);
            }
        }

        [DisplayName("Positive exposure only")]
        public int OnlyPositiveModelledExposures {
            get {
                return MonitoringVersusModelExposureRecords.Count(r => r.ModelledExposure > 0 && r.MonitoringConcentration == 0);
            }
        }

        [DisplayName("Unmatched modelled individual days")]
        public int UnmatchedModelExposures { get; set; }

        [DisplayName("Unmatched monitoring individual days")]
        public int UnmatchedMonitoringConcentrations { get; set; }

        [DisplayName("Pearson correlation modelled versus monitoring")]
        [Description("Pearson correlation modelled versus monitoring computed of log-transformed modelled and observed exposures (positives only).")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Pearson { get; set; }

        [DisplayName("Spearman rank correlation modelled versus monitoring")]
        [Description("Spearman rank correlation modelled versus monitoring of modelled and observed exposures.")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Spearman { get; set; }
    }
}
