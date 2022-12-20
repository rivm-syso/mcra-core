using System.Collections.Generic;
using System.Data;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.R.REngines;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Utils.Test.UnitTests.R.REngines.RDotNet {
    [TestClass]
    public class RDotNetEngineTests {

        private static DataTable MockDataTable() {
            var table = new DataTable();
            table.Columns.Add("Drug", typeof(string));
            table.Columns.Add("Patient", typeof(string));
            table.Columns.Add("Dosage", typeof(int));
            table.Columns.Add("Effect", typeof(double));
            table.Rows.Add("A", "PA", 10, 0.4);
            table.Rows.Add("A", "PA", 10, 0.1);
            table.Rows.Add("A", "PA", 10, 0.7);
            table.Rows.Add("A", "PA", 10, 0.3);
            table.Rows.Add("A", "PA", 10, 0.4);
            table.Rows.Add("B", "PB", 10, 1.4);
            table.Rows.Add("B", "PB", 10, 1.6);
            table.Rows.Add("B", "PB", 10, 1.1);
            table.Rows.Add("B", "PB", 10, 1.2);
            return table;
        }

        private static void printDataTable(DataTable table) {
            foreach (var item in table.Columns) {
                System.Diagnostics.Trace.Write(item);
                System.Diagnostics.Trace.Write(",");
            }
            System.Diagnostics.Trace.WriteLine("");
            foreach (DataRow dataRow in table.Rows) {
                foreach (var item in dataRow.ItemArray) {
                    System.Diagnostics.Trace.Write(item);
                    System.Diagnostics.Trace.Write(",");
                }
                System.Diagnostics.Trace.WriteLine("");
            }
        }

        [TestMethod]
        public void RDotNetEngine_GetInstance() {
            using (var instance = new RDotNetEngine()) {
                Assert.IsTrue(instance.IsRunning);
            }
            using (var instance = new RDotNetEngine()) {
                Assert.IsTrue(instance.IsRunning);
            }
        }

        [TestMethod]
        public void RDotNetEngine_BasicAssignment() {
            using (var instance = new RDotNetEngine()) {
                instance.EvaluateNoReturn("bubba <- c(3,5,7,9)");
                var result = instance.EvaluateNumericVector("bubba");
                Assert.AreEqual("3,5,7,9", string.Join(",", result));
            }
        }

        [TestMethod]
        public void RDotNetEngine_AssignInteger() {
            using (var instance = new RDotNetEngine()) {
                var name = "integer";
                var input = 41;
                instance.SetSymbol(name, input);
                var output = instance.EvaluateInteger(name);
                Assert.AreEqual(input, output);
            }
        }

        [TestMethod]
        public void RDotNetEngine_AssignDouble() {
            using (var instance = new RDotNetEngine()) {
                var name = "double";
                var input = 3.234;
                instance.SetSymbol(name, input);
                var output = instance.EvaluateDouble(name);
                Assert.AreEqual(input, output);
            }
        }

        [TestMethod]
        public void RDotNetEngine_AssignIntegerVector() {
            using (var instance = new RDotNetEngine()) {
                var name = "integerVector";
                var inputList = new List<int> { 3, 5, 7, 9 };
                instance.SetSymbol(name, inputList);
                var outputList = instance.EvaluateIntegerVector(name);
                CollectionAssert.AreEqual(inputList, outputList);
            }
        }

        [TestMethod]
        public void RDotNetEngine_AssignIntegerMatrix() {
            using (var instance = new RDotNetEngine()) {
                var name = "mat";
                var matrix = new int[,] {
                    { 1, 2 },
                    { 3, 4 },
                    { 5, 6 },
                    { 7, 8 }
                };
                instance.SetSymbol(name, matrix);
                var output = instance.EvaluateIntegerMatrix(name);
                Assert.IsTrue(matrix.MatrixEquals(output));
            }
        }

        [TestMethod]
        public void RDotNetEngine_AssignNumericVector() {
            using (var instance = new RDotNetEngine()) {
                var name = "numericVector";
                var input = new List<double> { 3.1, 5.1, 7.1, 9.1 };
                instance.SetSymbol(name, input);
                var output = instance.EvaluateNumericVector(name);
                CollectionAssert.AreEqual(input, output);
            }
        }

        [TestMethod]
        public void RDotNetEngine_AssignNumericMatrix() {
            using (var instance = new RDotNetEngine()) {
                var name = "mat";
                var matrix = new double[,] {
                    { 1.1, 2.2 },
                    { 3.3, 4.4 },
                    { 5.5, 6.6 },
                    { 7.7, 8.8 }
                };
                instance.SetSymbol(name, matrix);
                var output = instance.EvaluateMatrix(name);
                Assert.IsTrue(matrix.MatrixEquals(output));
            }
        }

        [TestMethod]
        public void RDotNetEngine_AssignCharacterVector() {
            using (var instance = new RDotNetEngine()) {
                var name = "characterVector";
                var inputList = new List<string> { "test", "test whitespace" };
                instance.SetSymbol(name, inputList);
                var outputList = instance.EvaluateCharacterVector(name);
                CollectionAssert.AreEqual(inputList, outputList);
            }
        }

        [TestMethod]
        public void RDotNetEngine_AssignDataFrame() {
            using (var instance = new RDotNetEngine()) {
                var name = "dataFrame";
                var table = MockDataTable();
                instance.SetSymbol(name, table);
                printDataTable(table);
            }
        }

        [TestMethod]
        public void RDotNetEngine_GetError() {
            using (var instance = new RDotNetEngine()) {
                try {
                    instance.EvaluateNoReturn("xsd");
                } catch {
                    var error = instance.GetErrorMessage();
                    Assert.AreEqual("Error: object 'xsd' not found\n", error);
                }
            }
        }

        [TestMethod]
        public void RDotNetEngine_LoadLibrary() {
            using (var instance = new RDotNetEngine()) {
                instance.LoadLibrary("lme4");
            }
        }

        [TestMethod]
        public void RDotNetEngine_ExecuteTTest() {
            using (var instance = new RDotNetEngine()) {

                // .NET Framework array to R vector.
                var group1 = new List<double>() { 30.02, 29.99, 30.11, 29.97, 30.01, 29.99 };
                instance.SetSymbol("group1", group1);

                // Direct parsing from R script.
                instance.EvaluateNoReturn("group2 <- c(29.89, 29.93, 29.72, 29.98, 30.02, 29.98)");
                var group2 = instance.EvaluateNumericVector("group2");

                // Test difference of mean and get the P-value.
                instance.EvaluateNoReturn("result <- t.test(group1, group2)");
                var p = instance.EvaluateDouble("result$p.value");

                System.Diagnostics.Trace.WriteLine($"Group1: [{string.Join(", ", group1)}]");
                System.Diagnostics.Trace.WriteLine($"Group2: [{string.Join(", ", group2)}]");
                System.Diagnostics.Trace.WriteLine($"P-value = {p:0.000}");
            }
        }

        [TestMethod]
        public void RDotNetEngine_FitLinearModel() {
            var table = MockDataTable();
            using (var instance = new RDotNetEngine()) {
                instance.SetSymbol("data", table);
                instance.EvaluateNoReturn("library(lme4)");
                instance.EvaluateNoReturn("model <- lm(Effect ~ 1 + Drug, data = data)");
                var residuals = instance.EvaluateNumericVector("residuals(model)");
            }
        }

        [TestMethod]
        public void RDotNetEngine_FitLinearMixedEffectModel() {
            var table = MockDataTable();
            using (var instance = new RDotNetEngine()) {
                instance.SetSymbol("data", table);
                instance.EvaluateNoReturn("library(lme4)");
                instance.EvaluateNoReturn("model <- lmer(Effect ~ 1 + Drug + (1|Patient), data)");
                var residuals = instance.EvaluateNumericVector("residuals(model)");
            }
        }
    }
}
