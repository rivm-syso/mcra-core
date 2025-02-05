using System.Collections.Generic;
using MCRA.Utils.Charting.OxyPlot;
using MCRA.Utils.ExtensionMethods;
using OxyPlot;
using OxyPlot.Series;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class DietaryAcuteCompoundPieChartCreator : ReportPieChartCreatorBase {

        private readonly List<IndividualSubstanceDrillDownRecord> _records;
        private readonly int _ix;

        public DietaryAcuteCompoundPieChartCreator(List<IndividualSubstanceDrillDownRecord> record, int ix) {
            Width = 500;
            Height = 350;
            _records = record;
            _ix = ix;
        }
        public override string Title => "Total exposure per body weight/day for substances";

        public override string ChartId {
            get {
                var pictureId = "0bd4966f-a97c-4cc6-b755-54024a009188";
                return StringExtensions.CreateFingerprint(_ix + pictureId);
            }
        }

        public override PlotModel Create() {
            var records = _records.OrderByDescending(r => r.EquivalentExposure).ToList();
            var pieSlices = records.Select(c => new PieSlice(c.SubstanceName, c.EquivalentExposure)).ToList();
            return create(pieSlices);
        }

        /// <summary>
        /// To add a legenda, set plotmodel IsLegendVisible = true, and add an empty Title for the series, see custom model
        /// </summary>
        /// <param name="pieSlices"></param>
        /// <returns></returns>
        private PlotModel create(List<PieSlice> pieSlices) {
            var noSlices = getNumberOfSlices(pieSlices);
            var palette = CustomPalettes.BeachToneReverse(noSlices);
            var plotModel = base.create(pieSlices, noSlices, palette);
            return plotModel;
        }
    }
}
