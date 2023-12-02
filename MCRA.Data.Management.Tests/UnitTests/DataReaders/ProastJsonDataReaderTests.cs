using MCRA.Data.Raw.Copying.BulkCopiers.DoseResponseModels;
using MCRA.Data.Raw.Objects.RawTableGroups;
using MCRA.General;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Data.Management.Test.DataReaders {

    /// <summary>
    /// Tests the custom data reader to read PROAST ans.all json files.
    /// </summary>
    [TestClass]
    public class ProastJsonDataReaderTests {

        private double _eps = 1e-4;
        private static string _pathProastFiles = @"Resources/Proast";

        private static string[] _proastFiles = new string[] {
            "2016-E90-HepaRG-AdipoRed-72h",
            "160425-B-CAR",
            "cellsALN",
            "ERODExp_erod",
            "ERODExp_erod_SINGMOD",
            "foetalExp_Weight",
            "idQuantal_number",
            "idQuantal_v2",
            "Mixture_IFNgml",
            "Mixture_IL10ml",
            "fitAdditive",
            "proast-analysis-1",
            "proast-analysis-2"
        };

        [TestMethod]
        public void ProastJsonDataReader() {
            var x = Math.Pow(-25, 1 / 3D);
            foreach (var file in _proastFiles) {
                RawDoseResponseModelData output = readProastJson(file);
                Assert.IsNotNull(output);
            }
        }

        [TestMethod]
        public void ProastJsonDataReader_Test1() {
            var data = readProastJson("foetalExp_Weight");
            assertModel(data, DoseResponseModelType.Expm3, 1, null, null, 199.1718, 18.184, 517.51, double.NaN, double.NaN, double.NaN, null, null);
            assertModel(data, DoseResponseModelType.Hillm3, 1, null, null, 320.3045, 85.891, 434.39, double.NaN, double.NaN, double.NaN, null, null);
        }

        [TestMethod]
        public void ProastJsonDataReader_Test2() {
            var data = readProastJson("ERODExp_erod");
            assertModel(data, DoseResponseModelType.Hillm5, 2, "f", null, 31.4057, 1.4943, 106.19, double.NaN, double.NaN, double.NaN, null, null);
            assertModel(data, DoseResponseModelType.Hillm5, 2, "m", null, 6.9551, 0.3129, 37.277, double.NaN, double.NaN, double.NaN, null, null);
            assertModel(data, DoseResponseModelType.Expm5, 2, "f", null, 39.6414, 0.2053, 77.038, double.NaN, double.NaN, double.NaN, null, null);
            assertModel(data, DoseResponseModelType.Expm5, 2, "m", null, 39.6414, 0.2053, 77.038, double.NaN, double.NaN, double.NaN, null, null);
        }

        [TestMethod]
        public void ProastJsonDataReader_Test3() {
            var data = readProastJson("idQuantal_number");
            assertModel(data, DoseResponseModelType.TwoStage, 1, null, null, 3.0567, double.NaN, double.NaN, double.NaN, double.NaN, double.NaN, null, null);
            assertModel(data, DoseResponseModelType.LogLogist, 1, null, null, 0.6446, 0.0839, 3.5134, double.NaN, double.NaN, double.NaN, null, null);
            assertModel(data, DoseResponseModelType.Weibull, 1, null, null, 0.587, 0.063, 3.5939, double.NaN, double.NaN, double.NaN, null, null);
            assertModel(data, DoseResponseModelType.LogProb, 1, null, null, 0.7363, 0.1234, 3.3028, double.NaN, double.NaN, double.NaN, null, null);
            assertModel(data, DoseResponseModelType.Gamma, 1, null, null, 0.5296, 0.0431, 3.4549, double.NaN, double.NaN, double.NaN, null, null);
            assertModel(data, DoseResponseModelType.Logistic, 1, null, null, 8.0649, double.NaN, double.NaN, double.NaN, double.NaN, double.NaN, null, null);
            assertModel(data, DoseResponseModelType.LVM_Exp_M3, 1, null, null, 0.4494, 0.0917, 3.723, double.NaN, double.NaN, double.NaN, null, null);
            assertModel(data, DoseResponseModelType.LVM_Hill_M3, 1, null, null, 0.4974, 0.0661, 3.822, double.NaN, double.NaN, double.NaN, null, null);
        }

        [TestMethod]
        public void ProastJsonDataReader_Test4() {
            var data = readProastJson("idQuantal_v2");
            assertModel(data, DoseResponseModelType.TwoStage, 1, null, null, 4.7804, double.NaN, double.NaN, double.NaN, double.NaN, double.NaN, null, null);
            assertModel(data, DoseResponseModelType.LogLogist, 1, null, null, 0.7476, 0.1931, 0.3783, double.NaN, double.NaN, double.NaN, null, null);
            assertModel(data, DoseResponseModelType.Weibull, 1, null, null, 0.711, 0.1938, 0.3787, double.NaN, double.NaN, double.NaN, null, null);
            assertModel(data, DoseResponseModelType.LogProb, 1, null, null, 0.7599, 0.1929, 0.3773, double.NaN, double.NaN, double.NaN, null, null);
            assertModel(data, DoseResponseModelType.Gamma, 1, null, null, 0.6591, 0.1944, 0.3788, double.NaN, double.NaN, double.NaN, null, null);
            assertModel(data, DoseResponseModelType.Logistic, 1, null, null, 5.9477, double.NaN, double.NaN, double.NaN, double.NaN, double.NaN, null, null);
            assertModel(data, DoseResponseModelType.LVM_Exp_M3, 1, null, null, 0.5182, 0.022, 4.9049, double.NaN, double.NaN, double.NaN, null, null);
            assertModel(data, DoseResponseModelType.LVM_Hill_M3, 1, null, null, 0.5731, 0.0297, 5.0677, double.NaN, double.NaN, double.NaN, null, null);
        }

        [TestMethod]
        public void ProastJsonDataReader_Test5() {
            var data = readProastJson("cellsALN");
            assertModel(data, DoseResponseModelType.Expm5, 2, null, "iedose", 0.012, 0.001, 0.0298, double.NaN, double.NaN, double.NaN, null, null);
            assertModel(data, DoseResponseModelType.Expm5, 2, null, "cadose", double.NaN, double.NaN, double.NaN, 0.1872, 0.099, 0.3218, null, null);
            assertModel(data, DoseResponseModelType.Hillm5, 2, null, "iedose", 0.0127, 0.0006, 0.0355, double.NaN, double.NaN, double.NaN, null, null);
            assertModel(data, DoseResponseModelType.Hillm5, 2, null, "cadose", double.NaN, double.NaN, double.NaN, 0.1858, 0.0975, 0.3366, null, null);
        }

        [TestMethod]
        public void ProastJsonDataReader_Test6() {
            var data = readProastJson("Mixture_IFNgml");
            assertModel(data, DoseResponseModelType.Expm3, 2, null, "iedose", 7.9066e-006, 0, 0.0001, double.NaN, double.NaN, double.NaN, null, null);
            assertModel(data, DoseResponseModelType.Expm3, 2, null, "cadose", double.NaN, double.NaN, double.NaN, 0.7436, 0.5333, 1.0431, null, null);
            assertModel(data, DoseResponseModelType.Hillm5, 2, null, "iedose", 0.0004, 0.0001, 0.001, double.NaN, double.NaN, double.NaN, null, null);
            assertModel(data, DoseResponseModelType.Hillm5, 2, null, "cadose", double.NaN, double.NaN, double.NaN, 0.7755, 0.5408, 1.1199, null, null);
        }

        [TestMethod]
        public void ProastJsonDataReader_Test7() {
            var data = readProastJson("Mixture_IL10ml");
            assertModel(data, DoseResponseModelType.Expm5, 2, null, "iedose", 0.0023, 0.0004, 0.007, double.NaN, double.NaN, double.NaN, null, null);
            assertModel(data, DoseResponseModelType.Expm5, 2, null, "cadose", double.NaN, double.NaN, double.NaN, 0.2929, 0.1989, 0.4226, null, null);
            assertModel(data, DoseResponseModelType.Hillm5, 2, null, "iedose", 0.0023, 0.0004, 0.0077, double.NaN, double.NaN, double.NaN, null, null);
            assertModel(data, DoseResponseModelType.Hillm5, 2, null, "cadose", double.NaN, double.NaN, double.NaN, 0.2941, 0.1993, 0.4251, null, null);
        }

        [TestMethod]
        public void ProastJsonDataReader_Test8() {
            var data = readProastJson("fitAdditive");
            assertModel(data, DoseResponseModelType.Expm5, 3, null, "RF-0101-001-PPP", 470, 363, 580, double.NaN, double.NaN, double.NaN, null, null);
            assertModel(data, DoseResponseModelType.Expm5, 3, null, "RF-0133-001-PPP", double.NaN, double.NaN, double.NaN, 130, 108, 167, null, null);
            assertModel(data, DoseResponseModelType.Expm5, 3, null, "RF-0218-001-PPP", double.NaN, double.NaN, double.NaN, 40, 31.8, 50.8, null, null);
        }

        [TestMethod]
        public void ProastJsonDataReader_Test9() {
            var data = readProastJson("ERODExp_erod_SINGMOD");
            assertModel(data, DoseResponseModelType.Expm5, 2, "f", null, 2.9, 0.3654, 18.68, double.NaN, double.NaN, double.NaN, null, null);
            assertModel(data, DoseResponseModelType.Expm5, 2, "m", null, 2.9, 0.3654, 18.68, double.NaN, double.NaN, double.NaN, null, null);
        }

        [TestMethod]
        public void ProastJsonDataReader_Test10() {
            var data = readProastJson("foetalExp_Weight_SINGMOD");
            assertModel(data, DoseResponseModelType.Expm3, 1, null, null, 200, 4.318, 485.3, double.NaN, double.NaN, double.NaN, null, null);
            Assert.AreEqual(100, data.BenchmarkDosesUncertain.GroupBy(r => r.idUncertaintySet).Count());
        }

        [TestMethod]
        public void ProastJsonDataReader_Test11() {
            var data = readProastJson("Adipored72h_SINGMOD_DA");
            assertModel(data, DoseResponseModelType.Expm5, 5, null, "compoundA", 170, 98.35, 290.8, double.NaN, double.NaN, double.NaN, null, null);
            assertModel(data, DoseResponseModelType.Expm5, 5, null, "compoundB", double.NaN, double.NaN, double.NaN, 42, 30.58, 61.66, null, null);
            assertModel(data, DoseResponseModelType.Expm5, 5, null, "compoundC", double.NaN, double.NaN, double.NaN, 130, 98.63, 186.3, null, null);
            assertModel(data, DoseResponseModelType.Expm5, 5, null, "compoundD", double.NaN, double.NaN, double.NaN, 4.3, 3.293, 6.359, null, null);
            assertModel(data, DoseResponseModelType.Expm5, 5, null, "compoundE", double.NaN, double.NaN, double.NaN, 31, 21.96, 44.55, null, null);
            var x = data.BenchmarkDosesUncertain.GroupBy(r => r.idUncertaintySet);
            Assert.AreEqual(100, data.BenchmarkDosesUncertain.GroupBy(r => r.idUncertaintySet).Count());
        }

        private void assertModel(
            RawDoseResponseModelData data,
            DoseResponseModelType type,
            int bmdRecordCount,
            string covariate,
            string substance,
            double bmd,
            double bmdL,
            double bmdU,
            double rpf,
            double rpfL,
            double rpfU,
            string[] parameterNames,
            double[] parameterValues
        ) {
            var modelRecord = data.DoseResponseModels.FirstOrDefault(r => r.DoseResponseModelType == type);
            var bmdRecords = data.BenchmarkDoses.Where(r => r.idDoseResponseModel == modelRecord.idDoseResponseModel).ToList();
            Assert.AreEqual(bmdRecordCount, bmdRecords.Count);

            var bmdRecord = bmdRecords.FirstOrDefault(r => (string.IsNullOrEmpty(covariate) || r.Covariates == covariate)
                && (string.IsNullOrEmpty(substance) || r.idSubstance == substance));
            Assert.IsNotNull(modelRecord);
            Assert.IsNotNull(bmdRecord);
            if (!double.IsNaN(bmd)) {
                Assert.AreEqual(bmd, bmdRecord.BenchmarkDose, _eps);
            }
            if (!double.IsNaN(bmdL)) {
                Assert.AreEqual(bmdL, bmdRecord.BenchmarkDoseLower.Value, _eps);
            }
            if (!double.IsNaN(bmdU)) {
                Assert.AreEqual(bmdU, bmdRecord.BenchmarkDoseUpper.Value, _eps);
            }
            if (!double.IsNaN(rpf)) {
                Assert.AreEqual(rpf, bmdRecord.Rpf.Value, _eps);
            }
            if (!double.IsNaN(rpfL)) {
                Assert.AreEqual(rpfL, bmdRecord.RpfLower.Value, _eps);
            }
            if (!double.IsNaN(rpfU)) {
                Assert.AreEqual(rpfU, bmdRecord.RpfUpper.Value, _eps);
            }
            if (parameterNames != null) {
                var parameters = bmdRecord.GetParameterValuesDict();
                for (int i = 0; i < parameterNames.Length; i++) {
                    Assert.IsTrue(parameters.ContainsKey(parameterNames[i]));
                    Assert.AreEqual(parameters[parameterNames[i]], parameterValues[i], _eps);
                }
            }
        }

        private static RawDoseResponseModelData readProastJson(string file) {
            var path = Path.Combine(_pathProastFiles, $"{file}.json");
            var text = File.ReadAllText(path);
            var reader = new ProastJsonDataReader();
            var output = reader.Read(text, file, "experiment1");
            return output;
        }
    }
}
