using MCRA.Utils.Charting.OxyPlot;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OxyPlot;
using OxyPlot.Series;

namespace MCRA.Utils.Test.UnitTests.Charting.Oxyplot {

    public class MockPieChartCreator : OxyPlotPieChartCreator {

        private readonly int _maxSlices;
        private readonly Func<int, OxyPalette> _paletteCreator;
        private readonly List<PieSlice> _slices;
        private readonly string _title;

        public string Title { get { return _title; } }

        public MockPieChartCreator(
            List<PieSlice> slices,
            Func<int, OxyPalette> paletteCreator,
            string title,
            int maxSlices = 13
        ) {
            Width = 500;
            Height = 300;
            _slices = slices;
            _paletteCreator = paletteCreator;
            _maxSlices = maxSlices;
            _title = title;
        }

        public override PlotModel Create() {
            var noSlices = getNumberOfSlices(_slices, _maxSlices);
            var palette = _paletteCreator(noSlices);
            var plotModel = create(_slices, noSlices, palette);
            plotModel.Title = _title;
            return plotModel;
        }
    }

    [TestClass]
    public class PieChartCreatorTests : ChartCreatorTestsBase {

        private static readonly List<PieSlice> _fakeData = [
            new PieSlice("Apple", 12),
            new PieSlice("Orange", 20),
            new PieSlice("Banana", 10),
            new PieSlice("Carrot", 12),
            new PieSlice("Cabagge", 23),
            new PieSlice("PineApple", 6),
            new PieSlice("Tomatoes", 11),
            new PieSlice("Leek", 12),
            new PieSlice("Papaja", 5),
            new PieSlice("Plums", 2),
            new PieSlice("Celeriac", 23),
            new PieSlice("Tablegrapes", 12),
            new PieSlice("Cherry", 11),
            new PieSlice("Pear", 12),
            new PieSlice("Rosebud", 13),
        ];

        /// <summary>
        /// General test procedure to test all custom palletes.
        /// </summary>
        [TestMethod]
        public void PieChartCreator_TestCustomPalettes() {
            var creator = new MockPieChartCreator(
                slices: _fakeData,
                paletteCreator: (noSlices) => CustomPalettes.BlueTone(noSlices),
                title: "BlueTone"
            );
            WritePng(creator, creator.Title);

            creator = new MockPieChartCreator(
                slices: _fakeData,
                paletteCreator: (noSlices) => CustomPalettes.ArtDecoTone(noSlices),
                title: "ArtDecoTone"
            );
            WritePng(creator, creator.Title);

            creator = new MockPieChartCreator(
                slices: _fakeData,
                paletteCreator: (noSlices) => CustomPalettes.BeachTone(noSlices),
                title: "BeachTone"
            );
            WritePng(creator, creator.Title);

            creator = new MockPieChartCreator(
                slices: _fakeData,
                paletteCreator: (noSlices) => CustomPalettes.CoolTone(noSlices),
                title: "CoolTone"
            );
            WritePng(creator, creator.Title);

            creator = new MockPieChartCreator(
                slices: _fakeData,
                paletteCreator: (noSlices) => CustomPalettes.DistinctTone(noSlices),
                title: "DistinctTone"
            );
            WritePng(creator, creator.Title);

            creator = new MockPieChartCreator(
                slices: _fakeData,
                paletteCreator: (noSlices) => CustomPalettes.EarthTone(noSlices),
                title: "EarthTone"
            );
            WritePng(creator, creator.Title);

            creator = new MockPieChartCreator(
                slices: _fakeData,
                paletteCreator: (noSlices) => CustomPalettes.ElegantTone(noSlices),
                title: "ElegantTone"
            );
            WritePng(creator, creator.Title);

            creator = new MockPieChartCreator(
                slices: _fakeData,
                paletteCreator: (noSlices) => CustomPalettes.GorgeousTone(noSlices),
                title: "GorgeousTone"
            );
            WritePng(creator, creator.Title);

            creator = new MockPieChartCreator(
                slices: _fakeData,
                paletteCreator: (noSlices) => CustomPalettes.GreenTone(noSlices),
                title: "GreenTone"
            );
            WritePng(creator, creator.Title);

            creator = new MockPieChartCreator(
                slices: _fakeData,
                paletteCreator: (noSlices) => CustomPalettes.PurpleTone(noSlices),
                title: "PurpleTone"
            );
            WritePng(creator, creator.Title);

            creator = new MockPieChartCreator(
                slices: _fakeData,
                paletteCreator: (noSlices) => CustomPalettes.WarmTone(noSlices),
                title: "WarmTone"
            );
            WritePng(creator, creator.Title);
        }

        /// <summary>
        /// General test procedure to test all oxyplot palletes.
        /// </summary>
        [TestMethod]
        public void PieChartCreator_TestOxyPalettes() {

            var creator = new MockPieChartCreator(
                slices: _fakeData,
                paletteCreator: (noSlices) => OxyPalettes.BlackWhiteRed(noSlices),
                title: "BlackWhiteRed"
            );
            WritePng(creator, creator.Title);

            creator = new MockPieChartCreator(
                slices: _fakeData,
                paletteCreator: (noSlices) => OxyPalettes.BlueWhiteRed(noSlices),
                title: "BlueWhiteRed"
            );
            WritePng(creator, creator.Title);

            creator = new MockPieChartCreator(
                slices: _fakeData,
                paletteCreator: (noSlices) => OxyPalettes.Cool(noSlices),
                title: "Cool"
            );
            WritePng(creator, creator.Title);

            creator = new MockPieChartCreator(
                slices: _fakeData,
                paletteCreator: (noSlices) => OxyPalettes.Hot(noSlices),
                title: "Hot"
            );
            WritePng(creator, creator.Title);

            creator = new MockPieChartCreator(
                slices: _fakeData,
                paletteCreator: (noSlices) => OxyPalettes.Hue(noSlices),
                title: "Hue"
            );
            WritePng(creator, creator.Title);

            creator = new MockPieChartCreator(
                slices: _fakeData,
                paletteCreator: (noSlices) => OxyPalettes.Jet(noSlices),
                title: "Jet"
            );
            WritePng(creator, creator.Title);

            creator = new MockPieChartCreator(
                slices: _fakeData,
                paletteCreator: (noSlices) => OxyPalettes.Rainbow(noSlices),
                title: "Rainbow"
            );
            WritePng(creator, creator.Title);

            creator = new MockPieChartCreator(
                slices: _fakeData,
                paletteCreator: (noSlices) => OxyPalettes.Gray(noSlices),
                title: "Gray"
            );
            WritePng(creator, creator.Title);
        }

        /// <summary>
        /// General test procedure to test all monochrome palletes.
        /// </summary>
        [TestMethod]
        public void PieChartCreator_TestMonochromePalettes() {
            var creator = new MockPieChartCreator(
                slices: _fakeData,
                paletteCreator: (noSlices) => CustomPalettes.Monochrome(noSlices, 0.5883, .2, .2, 1, 1),
                title: "Monochrome_1_blue"
            );
            WritePng(creator, creator.Title);

            creator = new MockPieChartCreator(
                slices: _fakeData,
                paletteCreator: (noSlices) => CustomPalettes.Monochrome(noSlices, 0.4138, .2, .2, 1, 1),
                title: "Monochrome_1_green"
            );
            WritePng(creator, creator.Title);

            creator = new MockPieChartCreator(
                slices: _fakeData,
                paletteCreator: (noSlices) => CustomPalettes.Monochrome(noSlices, 0.5883, .2, .2, .9, .9),
                title: "Monochrome_2_blue"
            );
            WritePng(creator, creator.Title);

            creator = new MockPieChartCreator(
                slices: _fakeData,
                paletteCreator: (noSlices) => CustomPalettes.Monochrome(noSlices, 0.4138, .2, .2, .9, .9),
                title: "Monochrome_2_green"
            );
            WritePng(creator, creator.Title);

            creator = new MockPieChartCreator(
                slices: _fakeData,
                paletteCreator: (noSlices) => CustomPalettes.Monochrome(noSlices, 0.5883, .3, .3, .9, .9),
                title: "Monochrome_3"
            );
            WritePng(creator, creator.Title);

            creator = new MockPieChartCreator(
                slices: _fakeData,
                paletteCreator: (noSlices) => CustomPalettes.Monochrome(noSlices, 0.5883, .3, .3, 1, 1),
                title: "Monochrome_4"
            );
            WritePng(creator, creator.Title);
        }

        /// <summary>
        /// General test procedure to test all triadic palletes.
        /// </summary>
        [TestMethod]
        public void PieChartCreator_TestTriadicPalettes() {
            var creator = new MockPieChartCreator(
                slices: _fakeData,
                paletteCreator: (noSlices) => CustomPalettes.Triadic(noSlices, 0.5883, .2, .2, .9, .9),
                title: "Triadic_1"
            );
            WritePng(creator, creator.Title);

            creator = new MockPieChartCreator(
                slices: _fakeData,
                paletteCreator: (noSlices) => CustomPalettes.Triadic(noSlices, 0.5883, .3, .3, .9, .9),
                title: "Triadic_2"
            );
            WritePng(creator, creator.Title);
        }

        /// <summary>
        /// General test procedure to test all analogous palletes.
        /// </summary>
        [TestMethod]
        public void PieChartCreator_TestAnalogousPalettes() {
            var creator = new MockPieChartCreator(
                slices: _fakeData,
                paletteCreator: (noSlices) => CustomPalettes.Analogous(noSlices, 0.5883, .2, .2, 1, 1),
                title: "Analogous_1"
            );
            WritePng(creator, creator.Title);

            creator = new MockPieChartCreator(
                slices: _fakeData,
                paletteCreator: (noSlices) => CustomPalettes.Analogous(noSlices, 0.5883, .2, .2, .9, .9),
                title: "Analogous_2"
            );
            WritePng(creator, creator.Title);

            creator = new MockPieChartCreator(
                slices: _fakeData,
                paletteCreator: (noSlices) => CustomPalettes.Analogous(noSlices, 0.5883, .3, .3, .9, .9),
                title: "Analogous_3"
            );
            WritePng(creator, creator.Title);
        }

        /// <summary>
        /// General test procedure to test all multiColor palletes.
        /// </summary>
        [TestMethod]
        public void PieChartCreator_TestMultiColorPalettes() {
            var creator = new MockPieChartCreator(
                slices: _fakeData,
                paletteCreator: (noSlices) => CustomPalettes.MultiColor(noSlices, [0.5883, 0.4138], .2, .2, .9, .9),
                title: "BiColor_1"
            );
            WritePng(creator, creator.Title);

            creator = new MockPieChartCreator(
                slices: _fakeData,
                paletteCreator: (noSlices) => CustomPalettes.MultiColor(noSlices, [0.5883, 0.4138], .3, .3, .9, .9),
                title: "BiColor_2"
            );
            WritePng(creator, creator.Title);
        }

        /// <summary>
        /// General test procedure to test all split-complementary palletes.
        /// </summary>
        [TestMethod]
        public void PieChartCreator_TestSplitComplementaryPalettes() {
            var creator = new MockPieChartCreator(
                slices: _fakeData,
                paletteCreator: (noSlices) => CustomPalettes.SplitComplementary(noSlices, 0.5883, .2, .2, .9, .9),
                title: "SplitComplementary_1"
            );
            WritePng(creator, creator.Title);

            creator = new MockPieChartCreator(
                slices: _fakeData,
                paletteCreator: (noSlices) => CustomPalettes.SplitComplementary(noSlices, 0.5883, .3, .3, .9, .9),
                title: "SplitComplementary_2"
            );
            WritePng(creator, creator.Title);
        }
    }
}
