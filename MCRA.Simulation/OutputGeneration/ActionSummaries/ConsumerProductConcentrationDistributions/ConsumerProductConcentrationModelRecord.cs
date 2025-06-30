using MCRA.Utils;
using MCRA.Utils.Statistics;
using MCRA.Utils.Statistics.Histograms;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ConcentrationModelCalculation.ConcentrationModels;
using System.Text.Json.Serialization;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace MCRA.Simulation.OutputGeneration {

    /// <summary>
    /// Contains summary information of a concentration model.
    /// </summary>
    public sealed class ConsumerProductConcentrationModelRecord {

        private Guid _id = Guid.NewGuid();

        public ConsumerProductConcentrationModelRecord() {
        }

        [Display(AutoGenerateField = false)]
        public string Id {
            get { return _id.ToString(); }
            set { _id = new Guid(value); }
        }

        [Display(AutoGenerateField = false)]
        public string IdSubstance { get; set; }

        [DisplayName("Substance name")]
        public string SubstanceName { get; set; }

        [DisplayName("Substance code")]
        public string SubstanceCode { get; set; }

        [Display(AutoGenerateField = false)]
        public string IdConsumerProduct { get; set; }

        [DisplayName("Consumer product name")]
        public string ConsumerProductName { get; set; }

        [DisplayName("Food code")]
        public string ConsumerProductCode { get; set; }

        [Description("The concentration model type specified in the concentration model section. If model fitting fails, a simpler (fallback) model is taken that overrules the model specified in the concentration model section (see, fitted model).")]
        [DisplayName("Desired Model")]
        public ConcentrationModelType DesiredModel { get; set; }

        [Description("The concentration model type used within the simulation.")]
        [DisplayName("Fitted Model")]
        public ConcentrationModelType Model { get; set; }

        [Display(AutoGenerateField = false)]
        public ConcentrationUnit Unit { get; set; }

        [Description("Estimated mu of the specified distribution (e.g. the lognormal distribution).")]
        [DisplayName("Mu")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double? Mu { get; set; }

        [Description("Estimated standard deviation of the specified distribution (e.g. the lognormal distribution).")]
        [DisplayName("Sigma")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double? Sigma { get; set; }

        [Description("MRL multiplication factor.")]
        [DisplayName("MRL multiplication factor")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double? FractionOfMrl { get; set; }

        [Description("Total number of analysed samples on which this model is based.")]
        [DisplayName("Total samples analysed (n)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public int TotalMeasurementsCount { get; set; }

        [Description("The percentage of the residues that are zero.")]
        [DisplayName("Zeros (%)")]
        [DisplayFormat(DataFormatString = "{0:P2}")]
        public double FractionTrueZeros { get; set; }

        [Description("The percentage of values considered as censored values (value < LOD or value < LOQ).")]
        [DisplayName("Censored (%)")]
        [DisplayFormat(DataFormatString = "{0:P2}")]
        public double FractionCensored { get; set; }

        [Description("The percentage of values considered as non-detects (value < LOD).")]
        [DisplayName("Non-detects (%)")]
        [DisplayFormat(DataFormatString = "{0:P2}")]
        public double FractionNonDetects { get; set; }

        [Description("The percentage of values considered as non-quantifications (value < LOQ).")]
        [DisplayName("Non-quantifications (%)")]
        [DisplayFormat(DataFormatString = "{0:P2}")]
        public double FractionNonQuantifications { get; set; }

        [Description("The percentage of detects.")]
        [DisplayName("Positives (%)")]
        [DisplayFormat(DataFormatString = "{0:P2}")]
        public double FractionPositives { get; set; }

        [Description("The derived percentage of consumer product samples potentially containing residues of this substance, corrected for actual the percentage of positives encountered in the data.")]
        [DisplayName("Corrected potential presence (%)")]
        [DisplayFormat(DataFormatString = "{0:P2}")]
        public double? CorrectedAgriculturalUseFraction { get; set; }

        [Description("The derived percentage of consumer product samples potentially containing residues of this substance.")]
        [DisplayName("Potential presence (%)")]
        [DisplayFormat(DataFormatString = "{0:P2}")]
        public double? AgriculturalUseFraction { get; set; }

        [Description("The Maximum Residue Limit (MRL) for this consumer product/substance combination.")]
        [DisplayName("MRL (ConcentrationUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double? MaximumResidueLimit { get; set; }

        [Description("Mean concentration according to this model (also includes values according to censored handling method).")]
        [DisplayName("Mean concentration (ConcentrationUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double? MeanConcentration { get; set; }

        [Display(AutoGenerateField = false)]
        public List<double> MeanConcentrationUncertaintyValues { get; set; }

        [Description("Mean concentration lower bound (p2.5).")]
        [DisplayName("Mean concentration lower bound (p2.5)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double MeanConcentrationLowerBoundPercentile {
            get {
                if ((MeanConcentrationUncertaintyValues?.Count ?? 0) > 1) {
                    return MeanConcentrationUncertaintyValues.Percentile(2.5);
                }
                return double.NaN;
            }
        }

        [Description("Mean concentration upper bound (p97.5).")]
        [DisplayName("Mean concentration upper bound (p97.5)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double MeanConcentrationUpperBoundPercentile {
            get {
                if ((MeanConcentrationUncertaintyValues?.Count ?? 0) > 1) {
                    return MeanConcentrationUncertaintyValues.Percentile(97.5);
                }
                return double.NaN;
            }
        }

        [Display(AutoGenerateField = false)]
        public List<HistogramBin> LogPositiveResiduesBins { get; set; }

        [Display(AutoGenerateField = false)]
        public int CensoredValuesCount { get; set; }

        [Display(AutoGenerateField = false)]
        public List<double> LORs { get; set; }

        [Description("Provides details about possible model warnings.")]
        [DisplayName("Warning")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public string Warning {
            get {
                if (FractionPositives > (AgriculturalUseFraction + 1e-5) && !SubstanceName.StartsWith("_")) {
                    return "More positives than expected based on specified potential presence.";
                }
                return null;
            }
        }

        [Display(AutoGenerateField = false)]
        [XmlIgnore, JsonIgnore]
        public bool HasMeasurements {
            get {
                return Model != ConcentrationModelType.Empirical
                    || FractionPositives > 0D
                    || CensoredValuesCount > 0;
            }
        }

        [Display(AutoGenerateField = false)]
        [XmlIgnore, JsonIgnore]
        public bool IsEmpiricalWithNoPositives100PctCensored {
            get {
                return Model == ConcentrationModelType.Empirical
                    && FractionPositives == 0D
                    && FractionCensored == 1D
                    && FractionTrueZeros == 0D;
            }
        }
    }
}

