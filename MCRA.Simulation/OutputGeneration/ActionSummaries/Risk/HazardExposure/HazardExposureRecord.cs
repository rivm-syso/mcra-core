using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {
    public class HazardExposureRecord {

        [DisplayName("Substance name")]
        public string SubstanceName { get; set; }

        [DisplayName("Substance code")]
        public string SubstanceCode { get; set; }

        [Description("Target biological matrix.")]
        [DisplayName("Biological matrix")]
        public string BiologicalMatrix { get; set; }

        [Description("Unit of the exposure and hazard estimates.")]
        [DisplayName("Unit")]
        public string TargetUnit { get; set; }

        [Description("Expression type of the target.")]
        [DisplayName("Expression type")]
        public string ExpressionType { get; set; }

        [Display(AutoGenerateField = false)]
        public bool IsCumulativeRecord { get; set; }

        [Display(AutoGenerateField = false)]
        public double UncertaintyLowerLimit { get; set; }

        [Display(AutoGenerateField = false)]
        public double UncertaintyUpperLimit { get; set; }

        [Display(AutoGenerateField = false)]
        public UncertainDataPointCollection<double> ExposurePercentilesAll { get; set; }

        [Display(AutoGenerateField = false)]
        public UncertainDataPointCollection<double> ExposurePercentilesPositives { get; set; }

        [Display(AutoGenerateField = false)]
        public UncertainDataPointCollection<double> HazardCharacterisationPercentilesAll { get; set; }

        [Display(AutoGenerateField = false)]
        public UncertainDataPointCollection<double> RiskPercentilesPositives { get; set; }

        [Display(AutoGenerateField = false)]
        public UncertainDataPointCollection<double> RiskPercentilesUncertainties { get; set; }

        [Display(AutoGenerateField = false)]
        public UncertainDataPointCollection<double> RiskPercentilesUppers { get; set; }

        [Description("Median of the exposures of all {IndividualDayUnit} from the nominal run (nominal run).")]
        [DisplayName("Median exposure all {IndividualDayUnit}")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double MedianAllExposure {
            get {
                return ExposurePercentilesAll[1].ReferenceValue;
            }
        }

        [Description("Lower (LowerConfidenceBound) percentile of the exposures of all {IndividualDayUnit} (nominal run).")]
        [DisplayName("Exposure (LowerConfidenceBound) all {IndividualDayUnit}")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double LowerAllExposure {
            get {
                return ExposurePercentilesAll[0].ReferenceValue;
            }
        }

        [Description("Upper (UpperConfidenceBound) percentile of the exposures of all {IndividualDayUnit} (nominal run).")]
        [DisplayName("Exposure (UpperConfidenceBound) all {IndividualDayUnit}")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double UpperAllExposure {
            get {
                return ExposurePercentilesAll[2].ReferenceValue;
            }
        }

        [Description("Percentage of the {IndividualDayUnit} with positive exposure (nominal run).")]
        [DisplayName("Percentage {IndividualDayUnit} positive exposure")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double PercentagePositives { get; set; }

        [Description("Mean exposure of the {IndividualDayUnit} with exposure > 0 (nominal run).")]
        [DisplayName("Mean exposure {IndividualDayUnit} exposure > 0")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double MeanExposure { get; set; }

        [Description("Standard deviation (on log scale) of the exposure of the {IndividualDayUnit} with exposure > 0 (nominal run).")]
        [DisplayName("Stdev exposure {IndividualDayUnit} exposure > 0")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double StDevExposure { get; set; }

        [Description("Median exposure of the {IndividualDayUnit} with exposure > 0 (nominal run).")]
        [DisplayName("Median exposure {IndividualDayUnit} exposure > 0")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double MedianExposure {
            get {
                return ExposurePercentilesPositives[1].ReferenceValue;
            }
        }

        [Description("Lower percentile of the exposures of the {IndividualDayUnit} with exposure > 0 (nominal run).")]
        [DisplayName("Exposure (LowerConfidenceBound) {IndividualDayUnit} exposure > 0")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double LowerExposure {
            get {
                return ExposurePercentilesPositives[0].ReferenceValue;
            }
        }

        [Description("Upper percentile of the exposures of the {IndividualDayUnit} with exposure > 0 (nominal run).")]
        [DisplayName("Exposure (UpperConfidenceBound) {IndividualDayUnit} exposure > 0")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double UpperExposure {
            get {
                return ExposurePercentilesPositives[2].ReferenceValue;
            }
        }

        [Description("Lower uncertainty bound (LowerBound) of the lower (LowerConfidenceBound) percentile of the exposures of the {IndividualDayUnit} with exposure > 0.")]
        [DisplayName("Exposure (LowerConfidenceBound) {IndividualDayUnit} exposure > 0 - Unc (LowerBound)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double LowerExposure_UncLower {
            get {
                return ExposurePercentilesPositives[0].Percentile(UncertaintyLowerLimit);
            }
        }

        [Description("Upper uncertainty bound (UpperBound) of the upper (UpperConfidenceBound) percentile of the exposures of the {IndividualDayUnit} with exposure > 0.")]
        [DisplayName("Exposure (UpperConfidenceBound) {IndividualDayUnit} exposure > 0 - Unc (UpperBound)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double UpperExposure_UncUpper {
            get {
                return ExposurePercentilesPositives[2].Percentile(UncertaintyUpperLimit);
            }
        }

        [Description("Nominal hazard characterisation value.")]
        [DisplayName("HC")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double NominalHazardCharacterisation { get; set; }

        [Description("Mean hazard characterisation of all {IndividualDayUnit} (nominal run).")]
        [DisplayName("Mean HC")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double MeanHc { get; set; }

        [Description("Standard deviation on logscale of the hazard characterisations of all {IndividualDayUnit} (nominal run).")]
        [DisplayName("Stdev HC all {IndividualDayUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double StDevHc { get; set; }

        [Description("Median hazard characterisation of all {IndividualDayUnit} (nominal run).")]
        [DisplayName("Median HC")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double MedianHc {
            get {
                return HazardCharacterisationPercentilesAll[1].ReferenceValue;
            }
        }

        [Description("Lower (LowerConfidenceBound) percentile of the hazard characterisations of all {IndividualDayUnit} (nominal run).")]
        [DisplayName("HC (LowerConfidenceBound)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double LowerHc {
            get {
                return HazardCharacterisationPercentilesAll[0].ReferenceValue;
            }
        }

        [Description("Upper (UpperConfidenceBound) percentile of the hazard characterisations of all {IndividualDayUnit} (nominal run).")]
        [DisplayName("HC (UpperConfidenceBound)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double UpperHc {
            get {
                return HazardCharacterisationPercentilesAll[2].ReferenceValue;
            }
        }

        [Description("Uncertainty lower bound (LowerBound) of the lower (LowerConfidenceBound) percentile of the hazard characterisations of all {IndividualDayUnit}.")]
        [DisplayName("HC (LowerConfidenceBound) - Unc ({LowerBound})")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double LowerHc_UncLower {
            get {
                return HazardCharacterisationPercentilesAll[0].Percentile(UncertaintyLowerLimit);
            }
        }

        [Description("Uncertainty upper bound (UpperBound) of the upper (UpperConfidenceBound) percentile of the hazard characterisations of all {IndividualDayUnit}.")]
        [DisplayName("HC (UpperConfidenceBound) - Unc ({UpperBound})")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double UpperHc_UncUpper {
            get {
                return HazardCharacterisationPercentilesAll[2].Percentile(UncertaintyUpperLimit);
            }
        }

        [Description("Median risk characterisation ratio ({RiskMetric}) of the {IndividualDayUnit} with positive exposure (nominal run).")]
        [DisplayName("Median risk {IndividualDayUnit} exposure > 0 ({RiskMetricShort})")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double MedianRisk {
            get {
                return RiskPercentilesPositives[1].ReferenceValue;
            }
        }

        [Description("Lower {LowerConfidenceBound} percentile of the risk characterisation ratio ({RiskMetric}) of the {IndividualDayUnit} with positive exposure (nominal run).")]
        [DisplayName("Risk (LowerConfidenceBound) {IndividualDayUnit} exposure > 0 ({RiskMetricShort})")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double LowerRisk {
            get {
                return RiskPercentilesPositives[0].ReferenceValue;
            }
        }

        [Description("Upper {UpperConfidenceBound} percentile of the risk characterisation ratio ({RiskMetric}) of the {IndividualDayUnit} with positive exposure (nominal run).")]
        [DisplayName("Risk (UpperConfidenceBound) {IndividualDayUnit} exposure > 0 ({RiskMetricShort})")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double UpperRisk {
            get {
                return RiskPercentilesPositives[2].ReferenceValue;
            }
        }

        [Description("Lower (LowerBound) uncertainty bound of the lower {LowerConfidenceBound} percentile of the risk characterisation ratio ({RiskMetric}) of the {IndividualDayUnit} with positive exposure of the {IndividualDayUnit} with positive exposure.")]
        [DisplayName("Risk (LowerConfidenceBound) {IndividualDayUnit} exposure > 0 - Unc ({LowerBound}) ({RiskMetricShort})")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double LowerRisk_UncLower {
            get {
                return RiskPercentilesPositives[0].Percentile(UncertaintyLowerLimit);
            }
        }

        [Description("Upper (UpperBound) uncertainty bound of the upper {UpperConfidenceBound} percentile of the risk characterisation ratio ({RiskMetric}) of the {IndividualDayUnit} with positive exposure.")]
        [DisplayName("Risk (UpperConfidenceBound) {IndividualDayUnit} exposure > 0 - Unc ({UpperBound}) ({RiskMetricShort})")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double UpperRisk_UncUpper {
            get {
                return RiskPercentilesPositives[2].Percentile(UncertaintyUpperLimit);
            }
        }

        [Description("Median risk characterisation ratio ({RiskMetric}) of all {IndividualDayUnit} of all {IndividualDayUnit} (nominal run).")]
        [DisplayName("Median risk all {IndividualDayUnit} ({RiskMetricShort})")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double MedianAllRisk { get; set; }

        [Description("Lower (LowerConfidenceBound) percentile of the risk characterisation ratio ({RiskMetric}) of all {IndividualDayUnit} (nominal run).")]
        [DisplayName("Risk (LowerConfidenceBound) all {IndividualDayUnit} ({RiskMetricShort})")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double LowerAllRisk { get; set; }

        [Description("Upper (UpperConfidenceBound) percentile of the risk characterisation ratio ({RiskMetric}) of all {IndividualDayUnit} (nominal run).")]
        [DisplayName("Risk (UpperConfidenceBound) all {IndividualDayUnit} ({RiskMetricShort})")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double UpperAllRisk { get; set; }

        [Description("Median risk characterisation ratio ({RiskMetric}) of the p50 uncertainty percentiles of all {IndividualDayUnit}.")]
        [DisplayName("Median risk all {IndividualDayUnit} - Unc (p50) ({RiskMetricShort})")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double MedianAllRisk_UncMedian {
            get {
                return RiskPercentilesUncertainties[0].Percentile(50);
            }
        }

        [Description("Median of the lower (LowerConfidenceBound) uncertainty percentiles of the risk characterisation ratio ({RiskMetric}) of all {IndividualDayUnit}.")]
        [DisplayName("Risk (LowerConfidenceBound) all {IndividualDayUnit} - Unc (p50) ({RiskMetricShort})")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double LowerAllRisk_UncMedian {
            get {
                return RiskPercentilesUncertainties[1].Percentile(50);
            }
        }

        [Description("Median of the upper (UpperConfidenceBound) uncertainty percentiles of the risk characterisation ratio ({RiskMetric}) of all {IndividualDayUnit}.")]
        [DisplayName("Risk (UpperConfidenceBound) all {IndividualDayUnit} - Unc (p50) ({RiskMetricShort})")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double UpperAllRisk_UncMedian {
            get {
                return RiskPercentilesUncertainties[2].Percentile(50);
            }
        }
    }
}
