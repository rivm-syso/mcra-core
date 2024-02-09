using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {
    public class SingleValueNonDietaryExposureDeterminantValueRecord {

        [Display(Name = "Exposure determinants ID", Order = 1)]
        public string ExposureDeterminantCombinationId { get; set; }

        [Display(Name = "Exposure determinant", Order = 2)]
        public string ExposureDeterminant { get; set; }

        [Display(Name = "Exposure determinant description", Order = 3)]
        public string ExposureDeterminantDescription { get; set; }

        [Display(Name = "Value", Order = 4)]
        public string Value { get; set; }
    }
}

