using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class AirIndoorFractionsDataRecord {
        [DisplayName("Subgroup id")]
        public string idSubgroup { get; set; }

        [Description("The lower bound of the age group.")]
        [DisplayName("Age lower")]
        public double? AgeLower { get; set; }

        [Description("The indoor fraction.")]
        [DisplayName("Fraction")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Fraction { get; set; }
    }
}

