using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using MCRA.Utils.Statistics.Histograms;
using MCRA.General;

namespace MCRA.Simulation.OutputGeneration {

    /// <summary>
    /// Helper class for substances, relative contribution to the upper exposure distribution.
    /// </summary>
    public sealed class CompoundExposureDistributionRecord {
        private Guid _id = Guid.NewGuid();

        [Display(AutoGenerateField = false)]
        public string Id {
            get { return _id.ToString(); }
            set { _id = new Guid(value); }
        }

        [DisplayName("Substance name")]
        public string CompoundName { get; set; }

        [DisplayName("Substance code")]
        public string CompoundCode { get; set; }

        [Description("Number of days for acute or number of individuals for chronic with exposure > 0.")]
        [DisplayName("{IndividualDayUnit} with exposure")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int N { get; set; }

        [Description("Percentage of individual days (acute) or individuals (chronic) without exposure.")]
        [DisplayName("Percentage zero (%)")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double Percentage { get; set;  }

        [Description("Average exposure value on logscale for exposures > 0 (expressed per substance [not in equivalents of reference substance]).")]
        [DisplayName("Mu for {IndividualDayUnit} exposure > 0 (IntakeUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Mu { get; set; }

        [Description("Standard deviation of exposure values on logscale for exposures > 0 (expressed per substance [not in equivalents of reference substance]).")]
        [DisplayName("Sigma for {IndividualDayUnit} exposure > 0 ")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Sigma { get; set; }

        [Description("Relative potency factor. Exposures are unscaled.")]
        [DisplayName("RPF")]
        [DisplayFormat(DataFormatString = "{0:G2}")]
        public double RelativePotencyFactor { get; set; }

        [Description("Assessment group membership.")]
        [DisplayName("P(AG)")]
        [DisplayFormat(DataFormatString = "{0:G2}")]
        public double AssessmentGroupMembership { get; set; }

        [Display(AutoGenerateField = false)]
        public List <HistogramBin> HistogramBins { get; set; }

        [Display(AutoGenerateField = false)]
        public bool IsImputed { get; set; }
    }
}
