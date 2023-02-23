using MCRA.Utils.ExtensionMethods;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class HazardExposure_TraditionalChartCreator : HazardExposureHeatMapCreatorBase {

        public HazardExposure_TraditionalChartCreator(
            HazardExposureSection section,
            string intakeUnit
        ) : base(section, intakeUnit) {
        }

        public override string ChartId {
            get {
                var pictureId = "bcc4b001-03c9-4061-b9d3-d67fcdd989ec";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }

        public override PlotModel Create() {
            return create(_section);
        }

        private PlotModel create(HazardExposureSection section) {
            var plotModel = base.createPlotModel(_section, _intakeUnit);
            var decades = Math.Ceiling(Math.Log10(_yHigh)) - Math.Floor(Math.Log10(_yLow));
            var positionBottomLabel = Math.Pow(10, .2 * decades) * _yLow;
            var ticks = GetTicks(_xLow, _xHigh, _hazardExposureRecords.Take(section.NumberOfLabels).ToList());
            var counter = 0;
            var numberOfSubstances = section.NumberOfSubstances;
            if (section.NumberOfSubstances <= section.NumberOfLabels) {
                numberOfSubstances = section.NumberOfLabels;
            }
            //Keep this line because probably this wil be reconsidered, now the option "Number of substances in hazard  and exposure plot" is useless;
            //_hazardExposureRecords = _hazardExposureRecords.Take(numberOfSubstances).ToList();
            foreach (var item in _hazardExposureRecords) {
                var color = OxyColors.Black;
                var label = string.Empty;
                if (_hazardExposureRecords.Count > 1) {
                    label = item.SubstanceName;
                }
                var fontSize = 10;

                if (item.IsCumulativeRecord) {
                    color = OxyColors.Red;
                    label = "CUMULATIVE";
                    fontSize = 13;
                }
                var scatterSeriesExposure = createScatterSeries(item.UpperExposure, item.NominalHazardCharacterisation, OxyColors.Black);
                plotModel.Series.Add(scatterSeriesExposure);
                var coordUpper = new List<double> { item.UpperExposure, item.NominalHazardCharacterisation };
                if (label != string.Empty && counter < section.NumberOfLabels) {
                    var textAnnotation = createAnnotation(positionBottomLabel, ticks, counter, label, fontSize);
                    plotModel.Annotations.Add(textAnnotation);
                    var labelConnection = createLabelConnection(positionBottomLabel, ticks, counter, coordUpper);
                    plotModel.Series.Add(labelConnection);
                }

                counter++;
            }
            return plotModel;
        }
    }
}
