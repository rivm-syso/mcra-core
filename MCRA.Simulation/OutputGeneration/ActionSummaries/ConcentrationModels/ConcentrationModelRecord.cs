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
    public sealed class ConcentrationModelRecord {

        private Guid _id = Guid.NewGuid();

        public ConcentrationModelRecord() {
        }

        [Display(AutoGenerateField = false)]
        public string Id {
            get { return _id.ToString(); }
            set { _id = new Guid(value); }
        }

        [Display(AutoGenerateField = false)]
        public string IdCompound { get; set; }

        [DisplayName("Substance name")]
        public string CompoundName { get; set; }

        [DisplayName("Substance code")]
        public string CompoundCode { get; set; }

        [Display(AutoGenerateField = false)]
        public string IdFood { get; set; }

        [DisplayName("Food name")]
        public string FoodName { get; set; }

        [DisplayName("Food code")]
        public string FoodCode { get; set; }

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

        [Description("The derived percentage of food samples potentially containing residues of this substance, corrected for actual the percentage of positives encountered in the data.")]
        [DisplayName("Corrected potential presence (%)")]
        [DisplayFormat(DataFormatString = "{0:P2}")]
        public double? CorrectedAgriculturalUseFraction { get; set; }

        [Description("The derived percentage of food samples potentially containing residues of this substance.")]
        [DisplayName("Potential presence (%)")]
        [DisplayFormat(DataFormatString = "{0:P2}")]
        public double? AgriculturalUseFraction { get; set; }

        [Description("The Maximum Residue Limit (MRL) for this food/compound combination.")]
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
                if (FractionPositives > (AgriculturalUseFraction + 1e-5) && !CompoundName.StartsWith("_")) {
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

        public void Summarize(
            Food food,
            Compound compound,
            ConcentrationModel concentrationModel,
            bool isCumulativeConcentrationModel,
            bool createHistogramBins = true
        ) {
            IdCompound = compound.Code;
            CompoundCode = compound.Code;
            CompoundName = compound.Name;
            IdFood = food.Code;
            FoodCode = food.Code;
            FoodName = food.Name;
            var modelTypeFitted = concentrationModel.ModelType;
            var modelTypeDesired = concentrationModel.DesiredModelType;
            if (isCumulativeConcentrationModel) {
                if (modelTypeDesired == ConcentrationModelType.NonDetectSpikeLogNormal) {
                    modelTypeDesired = ConcentrationModelType.LogNormal;
                }
                if (modelTypeFitted == ConcentrationModelType.NonDetectSpikeLogNormal) {
                    modelTypeFitted = ConcentrationModelType.LogNormal;
                }
            }
            Model = modelTypeFitted;
            DesiredModel = modelTypeDesired;
            Unit = concentrationModel.ConcentrationUnit;
            FractionTrueZeros = 1D - concentrationModel.CorrectedWeightedAgriculturalUseFraction;
            FractionCensored = concentrationModel.FractionCensored;
            FractionNonQuantifications = concentrationModel.FractionNonQuantifications;
            FractionNonDetects = concentrationModel.FractionNonDetects;
            FractionPositives = concentrationModel.FractionPositives;
            AgriculturalUseFraction = concentrationModel.WeightedAgriculturalUseFraction;
            CorrectedAgriculturalUseFraction = concentrationModel.CorrectedWeightedAgriculturalUseFraction;
            FractionOfMrl = concentrationModel.ModelType == ConcentrationModelType.MaximumResidueLimit ? concentrationModel.FractionOfMrl : null;

            switch (concentrationModel.ModelType) {
                case ConcentrationModelType.Empirical:
                    Mu = null;
                    Sigma = null;
                    break;
                case ConcentrationModelType.MaximumResidueLimit:
                    Mu = null;
                    Sigma = null;
                    MaximumResidueLimit = ((CMMaximumResidueLimit)concentrationModel).MaximumResidueLimit;
                    break;
                case ConcentrationModelType.CensoredLogNormal:
                    Mu = ((CMCensoredLogNormal)concentrationModel).Mu;
                    Sigma = ((CMCensoredLogNormal)concentrationModel).Sigma;
                    break;
                case ConcentrationModelType.ZeroSpikeCensoredLogNormal:
                    Mu = ((CMZeroSpikeCensoredLogNormal)concentrationModel).Mu;
                    Sigma = ((CMZeroSpikeCensoredLogNormal)concentrationModel).Sigma;
                    break;
                case ConcentrationModelType.NonDetectSpikeLogNormal:
                    Mu = ((CMNonDetectSpikeLogNormal)concentrationModel).Mu;
                    Sigma = ((CMNonDetectSpikeLogNormal)concentrationModel).Sigma;
                    break;
                case ConcentrationModelType.NonDetectSpikeTruncatedLogNormal:
                    Mu = ((CMNonDetectSpikeTruncatedLogNormal)concentrationModel).Mu;
                    Sigma = ((CMNonDetectSpikeTruncatedLogNormal)concentrationModel).Sigma;
                    break;
                case ConcentrationModelType.SummaryStatistics:
                    Mu = ((CMSummaryStatistics)concentrationModel).Mu;
                    Sigma = ((CMSummaryStatistics)concentrationModel).Sigma;
                    break;
                default:
                    break;
            }

            CensoredValuesCount = concentrationModel.Residues.CensoredValues.Count;
            TotalMeasurementsCount = concentrationModel.Residues.NumberOfResidues;
            LORs = concentrationModel.Residues.CensoredValues;
            MeanConcentration = concentrationModel.GetDistributionMean();

            if (createHistogramBins && concentrationModel.Residues.Positives.Any()) {
                // Generate the bins of the positive measurements
                var positiveResidueMeasurements = concentrationModel.Residues.Positives;

                var logPositiveResidueMeasurements = positiveResidueMeasurements
                    .Where(prm => prm > 0)
                    .Select(prm => Math.Log(prm))
                    .ToList();

                var min = BMath.Floor(logPositiveResidueMeasurements.Min(), 1);
                var max = BMath.Ceiling(logPositiveResidueMeasurements.Max(), 1);

                // Correct the minimum bound of the bins to coincide with a LOR value (that is smaller
                // than the minimum of the measured values)
                var lORsSmallerThanMin = LORs.Where(lor => lor > 0 && Math.Log(lor) <= logPositiveResidueMeasurements.Min());
                min = (lORsSmallerThanMin.Any()) ? Math.Log(lORsSmallerThanMin.Max()) : min;

                if (min == max) {
                    min = Math.Round(logPositiveResidueMeasurements.Min() - 1);
                    max = Math.Round(logPositiveResidueMeasurements.Max());
                    LogPositiveResiduesBins = logPositiveResidueMeasurements.MakeHistogramBins(1, min, max);
                } else if (Math.Sqrt(logPositiveResidueMeasurements.Count) < 100) {
                    var numberOfBins = BMath.Ceiling(Math.Sqrt(logPositiveResidueMeasurements.Count));
                    LogPositiveResiduesBins = logPositiveResidueMeasurements.MakeHistogramBins(numberOfBins, min, max);
                } else {
                    LogPositiveResiduesBins = logPositiveResidueMeasurements.MakeHistogramBins(100, min, max);
                }
            }
        }
    }
}

