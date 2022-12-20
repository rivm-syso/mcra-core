using System.Diagnostics;
using System.Linq;
using MCRA.Utils.Statistics;
using MCRA.Utils.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Utils.Test.UnitTests {

    [TestClass]
    public class UncertainDataPointCollectionTest {

        [TestMethod]
        public void UncertainDataPointCollection_TestPercentilesCollections() {
            var pc = new UncertainDataPointCollection<double>(new double[] { 0.1, 0.2, 0.3, 0.4, 0.5, 0.6, 0.7, 0.8, 0.9 });
            pc.ReferenceValues = new double[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            Assert.IsTrue(pc.ReferenceValues.Count() == 9);
            Assert.IsTrue(pc.XValues.Count() == 9);
            pc.AddUncertaintyValues(new double[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 });
            pc.AddUncertaintyValues(new double[] { 2, 3, 4, 5, 6, 7, 8, 9, 10 });
            foreach (var p in pc) {
                Trace.WriteLine($"Percentile: {p.XValue}, Reference: {p.ReferenceValue}, Min: {p.Percentile(2.5)}, Max: {p.Percentile(97.5)}");
            }
        }

        /// <summary>
        /// XML serializer
        /// </summary>
        [TestMethod]
        public void UncertainDataPointCollection_TestSerializer() {
            var udpc = new UncertainDataPointCollection<double>();
            udpc.XValues = new double[] { 1, 2, 3, 4 };
            udpc.ReferenceValues = new double[] { 10, 20, 30, 40 };
            var xmlString = udpc.ToXml();
            udpc = XmlSerialization.FromXml<UncertainDataPointCollection<double>>(xmlString);
        }
    }
}
