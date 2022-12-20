using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {
    public class AvailableHazardCharacterisationsSummaryRecord : HazardCharacterisationSummaryRecord {

        [Description("Hazard characterisation expressed for the original system from which it was derived (e.g., animals).")]
        [Display(Name = "Hazard characterisation test-system", Order = 100)]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double SystemHazardCharacterisation { get; set; }

        [Description("Unit of the test-system hazard characterisation.")]
        [Display(Name = "Unit test-system", Order = 100)]
        public string SystemDoseUnit { get; set; }

        [Description("The species of the test-system hazard characterisation.")]
        [Display(Name = "Species test system", Order = 100)]
        public string Species { get; set; }

        [Description("The organ of the test-system hazard characterisation.")]
        [Display(Name = "Organ test system", Order = 100)]
        public string Organ { get; set; }

        [Description("The exposure route of the test-system hazard characterisation.")]
        [Display(Name = "Exposure route test system", Order = 100)]
        public string ExposureRoute { get; set; }

        [Description("Conversion factor to align the dose unit of test-system hazard characterisation with the target dose unit.")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        [Display(Name = "Unit conversion factor", Order = 100)]
        public double UnitConversionFactor { get; set; }

        [Description("Conversion factor used for extrapolation between different expression types (e.g., NOAEL to BMD).")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        [Display(Name = "Expression type conversion factor", Order = 100)]
        public double ExpressionTypeConversionFactor { get; set; }

        [Description("Nominal inter-species conversion factor to translate the test system hazard characterisation to a human hazard characterisation (1/GMinter).")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        [Display(Name = "Inter-species conversion factor", Order = 100)]
        public double NominalInterSpeciesConversionFactor { get; set; }

        [Description("Nominal intra-species conversion factor to translate the test system hazard characterisation to a human hazard characterisation (1/EFintra).")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        [Display(Name = "Intra-species conversion factor", Order = 100)]
        public double NominalIntraSpeciesConversionFactor { get; set; }

        [Description("Additional conversion factor to translate the test system hazard characterisation to a human hazard characterisation (1/AF).")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        [Display(Name = "Additional conversion factor", Order = 100)]
        public double AdditionalConversionFactor { get; set; }

        [Description("Nominal conversion factor modelling the effect of kinetic-processes when translating the system hazard characterisation to the hazard characterisation.")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        [Display(Name = "Kinetic conversion factor", Order = 100)]
        public double NominalKineticConversionFactor { get; set; }

        [Description("Dose response model/equation.")]
        [Display(Name = "Model equation", Order = 100)]
        public string ModelEquation { get; set; }

        [Description("Parameter estimates of the dose-response curve.")]
        [Display(Name = "Parameter values", Order = 100)]
        public string ModelParameterValues { get; set; }

        [Description("Benchmark response from which this benchmark dose is derived.")]
        [DisplayFormat(DataFormatString = "{0:G4}")]
        [Display(Name = "Critical effect size", Order = 100)]
        public double CriticalEffectSize { get; set; } = double.NaN;

    }
}
