using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {

    /// <summary>
    /// Overall individual drilldown.
    /// </summary>
    public sealed class DetailedIndividualDayDrilldownRecord  {

        [Description("Food as eaten")]
        [DisplayName("Food as eaten")]
        public string FoodAsEaten { get; set; }

        [Description("Consumed amount of food as eaten.")]
        [DisplayName("Amount (ConsumptionUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Amount { get; set; }

        [Description("Consumed amount of modelled food (= Amount * Conversion factor / 100).")]
        [DisplayName("Modelled food")]
        public string ModelledFood { get; set; }

        [Description("Translation percentage from food as eaten to modelled food.")]
        [DisplayName("Conversion factor")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double ConversionFactor { get; set; }

        [Description("Consumed amount of modelled food (= Amount * Conversion factor / 100).")]
        [DisplayName("Portion amount (ConsumptionUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double PortionAmount { get; set; }

        [Description("Unit weight as specified in database.")]
        [DisplayName("Unit weight (ConsumptionUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double UnitWeight { get; set; }

        [Description("Units in composite sample as specified in database.")]
        [DisplayName("Units in composite sample")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double UnitsInCompositeSample { get; set; }

        [Description("Substance.")]
        [DisplayName("Substance")]
        public string Substance { get; set; }

        [Description("Monitoring residue (or drawn residue based on specified distribution).")]
        [DisplayName("Concentration in sample (ConcentrationUnit))")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double ConcentrationInSample { get; set; }

        [Description("Variability factor.")]
        [DisplayName("Variability factor")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double VariabilityFactor { get; set; }

        [Description("Stochastic variability factor (= Concentration in portion/Concentration in sample).")]
        [DisplayName("Stochastic vf")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double StochasticVf { get; set; }

        [Description("Drawn residue based on specified unit variability distribution and monitoring residue.")]
        [DisplayName("Concentration in portion (ConcentrationUnit))")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double ConcentrationInPortionUV { get; set; }

        [Description("Processing factor.")]
        [DisplayName("Processing factor")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double ProcessingFactor { get; set; }

        [Description("Processing correction factor.")]
        [DisplayName("Processing correction factor")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double ProcessingCorrectionFactor { get; set; }

        [Description("Processing type description.")]
        [DisplayName("Processing type description")]
        public string ProcessingTypeDescription { get; set; }

        [Description("Batch processing.")]
        [DisplayName("Batch processing")]
        public string BatchProcessing { get; set; }

        [Description("Exposure.")]
        [DisplayName("Exposure (IntakeUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Exposure { get; set; }

        [Description("RPF.")]
        [DisplayName("RPF")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Rpf { get; set; }

        [Description("Equivalent exposure.")]
        [DisplayName("Equivalent exposure (IntakeUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double EquivalentExposure { get; set; }

        [Description("Percentage of total.")]
        [DisplayName("Percentage of total (%)")]
        [DisplayFormat(DataFormatString = "{0:P2}")]
        public double Percentage { get; set; }
    }
}
