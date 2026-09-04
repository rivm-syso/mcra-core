using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class ConsumerProductExposureFractionRecord {
        [DisplayName("Product code")]
        public string ProductCode { get; set; }

        [DisplayName("Product name")]
        public string ProductName { get; set; }

        [DisplayName("Parent")]
        public string ParentName { get; set; }

        [DisplayName("Substance code")]
        public string SubstanceCode { get; set; }

        [DisplayName("Substance name")]
        public string SubstanceName { get; set; }

        public string Route { get; set; }

        [Description("The lower bound of the age group.")]
        [DisplayName("Age lower")]
        public double? AgeLower { get; set; }

        [Description("The sex group.")]
        [DisplayName("Sex")]
        public string Sex { get; set; }

        [Description("The exposure fraction.")]
        [DisplayName("Fraction")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Fraction { get; set; }
    }
}

