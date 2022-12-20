using System.Collections.Generic;
using System.Xml.Serialization;

namespace MCRA.General.Action.Settings.Dto {

    public enum OutputSectionSelectionMethod {
        OptIn,
        OptOut
    }

    public class OutputDetailSettingsDto {

        public bool IsDetailedOutput { get; set; }

        public bool SummarizeSimulatedData { get; set; }

        public bool StoreIndividualDayIntakes { get; set; }

        public double[] SelectedPercentiles { get; set; } = new double[] { 50, 90, 95, 99, 99.9, 99.99 };

        public double PercentageForDrilldown { get; set; } = 97.5D;

        public double PercentageForUpperTail { get; set; } = 97.5D;

        public ExposureMethod ExposureMethod { get; set; } = ExposureMethod.Automatic;

        public double[] ExposureLevels { get; set; } = new double[] { 1, 10, 50, 100, 200, 500 };

        // Obsolete
        //public ExposureInterpretation ExposureInterpretation { get; set; }

        public double Intervals { get; set; } = 20D;

        public double[] ExtraPredictionLevels { get; set; } = new double[] { };

        public double LowerPercentage { get; set; } = 25D;

        public double UpperPercentage { get; set; } = 75D;

        [XmlArrayItem("Label")]
        public List<string> OutputSections { get; set; }

        public OutputSectionSelectionMethod OutputSectionSelectionMethod { get; set; }

        public double MaximumCumulativeRatioCutOff { get; set; } = 2D;

        public double[] MaximumCumulativeRatioPercentiles { get; set; } = new double[] { 95, 99 };

        public double MaximumCumulativeRatioMinimumPercentage { get; set; } = 5;

    }
}