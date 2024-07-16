using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class DustAdherenceAmountsDataRecord {
        [DisplayName("Subgroup id")]
        public string idSubgroup { get; set; }

        [Description("The lower bound of the age group.")]
        [DisplayName("Age lower")]
        public double? AgeLower { get; set; }

        [Description("The sex group.")]
        [DisplayName("Sex")]
        public string Sex { get; set; }

        [Description("The distribution type.")]
        [DisplayName("Distribution")]
        public string DistributionType { get; set; }

        [Description("Either the mean value of the distribution if a distriubution is specified. Otherwise the constant value of the parameter")]
        [DisplayName("Amount")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Value { get; set; }

        [Description("The coefficient of variation of the distribution.")]
        [DisplayName("CV")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double? CvVariability { get; set; }
    }
}

