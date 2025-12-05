using System.Diagnostics;
using MCRA.Utils.Statistics;
using MCRA.Utils.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Utils.Test.UnitTests {

    [TestClass]
    public class UncertainDataPointCollectionTest {

        [TestMethod]
        public void UncertainDataPointCollection_TestPercentilesCollections() {
            var pc = new UncertainDataPointCollection<double>([0.1, 0.2, 0.3, 0.4, 0.5, 0.6, 0.7, 0.8, 0.9]);
            pc.ReferenceValues = [1, 2, 3, 4, 5, 6, 7, 8, 9];
            Assert.AreEqual(9, pc.ReferenceValues.Count());
            Assert.AreEqual(9, pc.XValues.Count());
            pc.AddUncertaintyValues([0, 1, 2, 3, 4, 5, 6, 7, 8]);
            pc.AddUncertaintyValues([2, 3, 4, 5, 6, 7, 8, 9, 10]);
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
            udpc.XValues = [1, 2, 3, 4];
            udpc.ReferenceValues = [10, 20, 30, 40];
            var xmlString = udpc.ToXml();
            udpc = XmlSerialization.FromXml<UncertainDataPointCollection<double>>(xmlString);
        }
    }
}
