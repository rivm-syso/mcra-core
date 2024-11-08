using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Utils.ExtensionMethods;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Data.Compiled.Test {
    [TestClass]
    public class HumanMonitoringSamplingMethodTests {

        /// <summary>
        /// In some cases, the sampling type is part of the biological matrix name. When combining
        /// the names of biological matrix and sampling type, we should avoid duplication of sampling
        /// type (so avoid things like 'Blood serum (-) Serum')
        /// </summary>
        [TestMethod]
        public void Name_SamplingTypeNameIsPartOfBiologicalMatrix_ShouldOmitSamplingTypeName() {

            var samplingTypePartOfBiologicalMatrix = new HumanMonitoringSamplingMethod {
                BiologicalMatrix = BiologicalMatrix.BloodSerum,    // Contains the sampling type name 'Serum'
                SampleTypeCode = "Serum"
            };
            var samplingTypeNotPartOfBiologicalMatrix = new HumanMonitoringSamplingMethod {
                BiologicalMatrix = BiologicalMatrix.Urine,
                SampleTypeCode = "Spot"
            };

            var nameSamplingTypePartOf = samplingTypePartOfBiologicalMatrix.Name;
            var nameSamplingTypeNotPartOf = samplingTypeNotPartOfBiologicalMatrix.Name;

            Assert.AreEqual(BiologicalMatrix.BloodSerum.GetDisplayName(), nameSamplingTypePartOf);
            Assert.AreEqual($"{BiologicalMatrix.Urine.GetDisplayName()} spot", nameSamplingTypeNotPartOf);
        }
    }
}
