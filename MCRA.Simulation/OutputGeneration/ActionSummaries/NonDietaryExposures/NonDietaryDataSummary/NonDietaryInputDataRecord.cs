using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class NonDietaryInputDataRecord {

        [DisplayName("Non-dietary survey")]
        public string NonDietarySurvey { get; set; }

        [DisplayName("Substance name")]
        public string CompoundName { get; set; }

        [DisplayName("Substance code")]
        public string CompoundCode { get; set; }

        [DisplayName("Total individuals")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int TotalIndividuals { get; set; }

        [DisplayName("Mean dermal exposure (NonDietaryExposureUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double MeanDermal { get; set; }

        [DisplayName("Mean oral non-dietary exposure (NonDietaryExposureUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double MeanOral { get; set; }

        [DisplayName("Mean inhalation exposure (NonDietaryExposureUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double MeanInhalation { get; set; }

        #region Obsolete

        [Display(AutoGenerateField = false)]
        public string Matched { get; set; }

        [Display(AutoGenerateField = false)]
        public double DermalAbsorptionFactor { get; set; }

        [Display(AutoGenerateField = false)]
        public double OralAbsorptionFactor { get; set; }

        [Display(AutoGenerateField = false)]
        public double InhalationAbsorptionFactor { get; set; }

        #endregion
    }
}
