using System.Globalization;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ComponentCalculation.DriverSubstanceCalculation;
using MCRA.Simulation.Calculators.ComponentCalculation.ExposureMatrixCalculation;
using MCRA.Simulation.Calculators.ComponentCalculation.NmfCalculation;
using MCRA.Simulation.Calculators.MixtureCalculation.ExposureMatrixCalculation;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.MockCalculatorSettings;
using MCRA.Utils;
using MCRA.Utils.Charting.OxyPlot;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;
using MCRA.Utils.Test;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Calculators.NonNegativeMatrixFactorization {

    /// <summary>
    /// NonNegativeMatrixFactorization calculator
    /// </summary>
    [TestClass]
    public class NmfCalculators {

        #region Helper classes

        internal class ExposurePerIndividualCompoundRecord {
            public string IndividualCode { get; set; }
            public int NumberOfSurveyDays { get; set; }
            public double SamplingWeight { get; set; }
            public double BodyWeight { get; set; }
            public Compound Compound { get; set; }
            public double Intake { get; set; }
        }

        #endregion

        /// <summary>
        /// Non Negative Matrix Factorization, simulate data
        /// </summary>
        [TestMethod]
        public void NmfCalculator_NMFTest1() {
            var sigma = 0.0D;
            var normalize = false;
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var exposures = initialize(sigma, normalize, random, out var substancesWithExposure);
            var individuals = Enumerable.Range(1, exposures.ColumnDimension).Select(c => new Individual(c)).ToList();

            var rowRecords = substancesWithExposure
                .Select((x, ix) => (ix, rowRecord: new ExposureMatrixRowRecord() {
                    Substance = substancesWithExposure[ix],
                    TargetUnit = new TargetUnit() { Target = ExposureTarget.DefaultInternalExposureTarget },
                    Stdev = 1d
                }))
                .ToDictionary(c => c.ix, c => c.rowRecord);

            var exposure = new ExposureMatrix() {
                Exposures = exposures,
                Individuals = individuals,
                RowRecords = rowRecords,
            };

            var emb = new ExposureMatrixBuilder();
            var (exposureMatrix, totalExposureCutOffPercentage) = emb.Compute(exposure);
            var nmfCalculatorSettings = new MockNmfCalculatorSettings() {
                NumberOfIterations = 500,
                NumberOfComponents = 6,
                Sparseness = 0.08,
                Epsilon = 0.001D
            };
            var nmfCalculator = new NmfCalculator(nmfCalculatorSettings);
            var (componentRecords, sweepWMatrix, componentMatrix, rmse) = nmfCalculator.Compute(exposureMatrix.Exposures, random, new ProgressState());
            var individualComponentMatrix = new IndividualMatrix() {
                ClusterResult = null,
                VMatrix = componentMatrix,
                Individuals = individuals
            };

            var componentExposureSection = new ComponentSelectionOverviewSection();
            componentExposureSection.Summarize(
                substances: exposureMatrix.RowRecords.Values.Select(c => c.Substance).ToList(),
                componentRecords: componentRecords,
                rmse: rmse,
                uMatrix: sweepWMatrix,
                substanceSamplingMethods: null,
                exposureApproachType: ExposureApproachType.RiskBased,
                internalConcentrationType: InternalConcentrationType.MonitoringConcentration,
                exposureType: ExposureType.Chronic,
                totalExposureCutOffPercentile: totalExposureCutOffPercentage,
                sparseness: 0.08,
                ratioCutoff: 0,
                totalExposureCutoff: 0,
                numberOfDays: exposure.Exposures.ColumnDimension,
                numberOfSelectedDays: exposureMatrix.Individuals.Count,
                numberOfIterations: 500,
                removeZeros: true,
                header: new SectionHeader()
            );
            var component1 = componentExposureSection.SubstanceComponentRecords[0].Count;
            var component2 = componentExposureSection.SubstanceComponentRecords[1].Count;
            var component3 = componentExposureSection.SubstanceComponentRecords[2].Count;
            var component4 = componentExposureSection.SubstanceComponentRecords[3].Count;
            var component5 = componentExposureSection.SubstanceComponentRecords[4].Count;
            var component6 = componentExposureSection.SubstanceComponentRecords[5].Count;
            Assert.AreEqual(7, component1);
            Assert.AreEqual(4, component2);
            Assert.AreEqual(2, component3);
            Assert.AreEqual(1, component4);
            Assert.AreEqual(6, component5);
            Assert.AreEqual(3, component6);

            plotHeatMap(500, seed, 0.08, 6, componentExposureSection, sigma);
        }

        /// <summary>
        /// Non Negative Matrix Factorization, using French test data
        /// </summary>
        [TestMethod]
        public void NmfCalculator_NMFTest2() {
            var results = readExposurePerIndividualCompoundRecords();
            var sum = results.Sum(r => r.Intake);
            Assert.AreEqual(677117.077562527, sum, 1e-1);

            //Note that sw ~ 0: not sparse; sw ~ 1: sparse
            var nmfCalculatorSettings = new MockNmfCalculatorSettings() {
                NumberOfIterations = 1000,
                NumberOfComponents = 5,
                Sparseness = 0.00004,
                Epsilon = 0.001
            };
            var nmfCalculator = new NmfCalculator(nmfCalculatorSettings);
            var driverSubstanceSettings = new MockDriverSubstanceCalculatorSettings() {
                TotalExposureCutOff = 0,
                RatioCutOff = 4
            };
            var maximumCumulativeRatioSection = new MaximumCumulativeRatioSection();
            var substancesWithExposure = new List<Compound>();
            var exposures = initialize(results, out substancesWithExposure);
            var individuals = Enumerable.Range(1, exposures.ColumnDimension).Select(c => new Individual(c)).ToList();
            var rowRecords = substancesWithExposure.Select((x, ix) => (ix, rowRecord: new ExposureMatrixRowRecord() {
                Substance = substancesWithExposure[ix],
                TargetUnit = new TargetUnit() { Target = ExposureTarget.DefaultInternalExposureTarget },
                Stdev = 1d
            }))
                .ToDictionary(c => c.ix, c => c.rowRecord);
            var exposure = new ExposureMatrix() {
                Exposures = exposures,
                Individuals = individuals,
                RowRecords = rowRecords
            };

            var emb = new ExposureMatrixBuilder();
            var (exposureMatrix, totalExposureCutOffPercentage) = emb.Compute(exposure);
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var (componentRecords, sweepWMatrix, componentMatrix, rmse) = nmfCalculator.Compute(exposureMatrix.Exposures, random, new ProgressState());
            var individualComponentMatrix = new IndividualMatrix() {
                ClusterResult = null,
                VMatrix = componentMatrix,
                Individuals = individuals
            };
            var componentExposureSection = new ComponentSelectionOverviewSection();
            componentExposureSection.Summarize(
                substances: exposureMatrix.RowRecords.Values.Select(c => c.Substance).ToList(),
                componentRecords: componentRecords,
                rmse: rmse,
                uMatrix: sweepWMatrix,
                substanceSamplingMethods: null,
                exposureApproachType: ExposureApproachType.RiskBased,
                internalConcentrationType: InternalConcentrationType.MonitoringConcentration,
                exposureType: ExposureType.Chronic,
                totalExposureCutOffPercentile: totalExposureCutOffPercentage,
                sparseness: 0.00004,
                ratioCutoff: 4,
                totalExposureCutoff: 0,
                numberOfDays: exposure.Exposures.ColumnDimension,
                numberOfSelectedDays: exposureMatrix.Individuals.Count,
                numberOfIterations: 1000,
                removeZeros: true,
                header: new SectionHeader()
            );
            var nonZeroComponents = componentExposureSection.SubstanceComponentRecords;
            var component1 = nonZeroComponents[0].Count;
            var component2 = nonZeroComponents[1].Count;
            var component3 = nonZeroComponents[2].Count;
            var component4 = nonZeroComponents[3].Count;
            var component5 = nonZeroComponents[4].Count;
            Assert.AreEqual(32, component1);
            Assert.AreEqual(35, component2);
            Assert.AreEqual(33, component3);
            Assert.AreEqual(26, component4);
            Assert.AreEqual(14, component5);

            plotHeatMap(1000, seed, 0.00004, 4, componentExposureSection);
            var rpfs = substancesWithExposure.ToDictionary(r => r, r => 1d);
            var memberships = substancesWithExposure.ToDictionary(r => r, r => 1d);
            maximumCumulativeRatioSection.Summarize(
                DriverSubstanceCalculator.CalculateExposureDrivers(exposure),
                TargetUnit.FromExternalExposureUnit(ExternalExposureUnit.ugPerKgBWPerDay),
                ExposureApproachType.RiskBased,
                4,
                new double[] { 5, 50, 95 },
                0,
                0
            );
            OxyPlotChartCreator chart = new DriverSubstancesEllipsChartCreator(maximumCumulativeRatioSection);
            chart.CreateToSvg(TestUtilities.ConcatWithOutputPath("mcrDriversEllipsTest2.svg"));
            chart = new DriverSubstancesChartCreator(maximumCumulativeRatioSection);
            chart.CreateToSvg(TestUtilities.ConcatWithOutputPath("mcrDriversTest2.svg"));
        }

        private static List<ExposurePerIndividualCompoundRecord> readExposurePerIndividualCompoundRecords() {
            var results = new List<ExposurePerIndividualCompoundRecord>();
            var fileName = Path.Combine("Resources", "IndividualCompoundIntakeSectionTable.csv");
            var substances = new Dictionary<string, Compound>();
            if (File.Exists(fileName)) {
                var lines = File.ReadAllLines(fileName);
                for (int i = 1; i < lines.Length; i++) {
                    var check = lines[i].Split('\"');
                    var parts = new List<string> {
                        check[1]
                    };
                    var result = check[2].Split(',').ToList();
                    parts.Add(result[1]);
                    parts.Add(result[2]);
                    parts.Add(result[3]);
                    parts.Add(check[3]);
                    parts.Add(check[4].Trim(','));
                    var compoundName = parts[4].Replace("\"", "").Trim();
                    if (!substances.TryGetValue(compoundName, out var substance)) {
                        substance = new Compound() {
                            Name = compoundName,
                            Code = compoundName,
                        };
                        substances[substance.Code] = substance;
                    }
                    var data = new ExposurePerIndividualCompoundRecord() {
                        SamplingWeight = 1D,
                        Compound = substance,
                        Intake = double.Parse(parts[5], CultureInfo.InvariantCulture) / double.Parse(parts[2], CultureInfo.InvariantCulture),
                    };
                    results.Add(data);
                }
            }
            return results;
        }

        /// <summary>
        /// Paul Price MCR, simulate data
        /// </summary>
        [TestMethod]
        public void NmfCalculator_NMFTest3() {
            //Note that sw ~ 0: not sparse; sw ~ 1: sparse
            var nmfCalculatorSettings = new MockNmfCalculatorSettings() {
                NumberOfIterations = 500,
                NumberOfComponents = 4,
                Sparseness = 0.8,
                Epsilon = 0.001
            };
            var nmfCalculator = new NmfCalculator(nmfCalculatorSettings);
            var driverSubstanceSettings = new MockDriverSubstanceCalculatorSettings() {
                TotalExposureCutOff = 0,
                RatioCutOff = 0
            };
            var substancesWithExposure = new List<Compound>();

            var sigma = 0.2D;
            var normalize = false;
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var exposures = initialize(sigma, normalize, random, out substancesWithExposure);
            var individuals = Enumerable.Range(1, exposures.ColumnDimension).Select(c => new Individual(c)).ToList();
            var rowRecords = substancesWithExposure
                .Select((x, ix) => (ix, rowRecord: new ExposureMatrixRowRecord() {
                    Substance = substancesWithExposure[ix],
                    TargetUnit = new TargetUnit() { Target = ExposureTarget.DietaryExposureTarget },
                    Stdev = 1d
                }))
                .ToDictionary(c => c.ix, c => c.rowRecord);
            var exposure = new ExposureMatrix() {
                Exposures = exposures,
                Individuals = individuals,
                RowRecords = rowRecords
            };

            var emb = new ExposureMatrixBuilder();
            var (exposureMatrix, totalExposureCutOffPercentage) = emb.Compute(exposure);
            var (componentRecords, sweepWMatrix, componentMatrix, rmse) = nmfCalculator.Compute(exposureMatrix.Exposures, random, new ProgressState());

            var componentExposureSection = new ComponentSelectionOverviewSection();
            var individualComponentMatrix = new IndividualMatrix() {
                ClusterResult = null,
                VMatrix = componentMatrix,
                Individuals = individuals
            };
            componentExposureSection.Summarize(
                substances: exposureMatrix.RowRecords.Values.Select(c => c.Substance).ToList(),
                componentRecords: componentRecords, rmse: rmse,
                uMatrix: sweepWMatrix,
                substanceSamplingMethods: null,
                exposureApproachType: ExposureApproachType.RiskBased,
                internalConcentrationType: InternalConcentrationType.MonitoringConcentration,
                exposureType: ExposureType.Chronic,
                totalExposureCutOffPercentile: totalExposureCutOffPercentage,
                sparseness: 0.8,
                ratioCutoff: 0,
                totalExposureCutoff: 0,
                numberOfDays: exposure.Exposures.ColumnDimension,
                numberOfSelectedDays: exposureMatrix.Individuals.Count,
                numberOfIterations: 500,
                removeZeros: true,
                header: new SectionHeader()
            );
            var nonZeroComponents = componentExposureSection.SubstanceComponentRecords;
            var component1 = nonZeroComponents[0].Count;
            var component2 = nonZeroComponents[1].Count;
            Assert.AreEqual(1, component1);
            Assert.AreEqual(4, component2);

            plotHeatMap(500, seed, 0.8, 4, componentExposureSection);

            var rpfs = substancesWithExposure.ToDictionary(r => r, r => 1d);
            var memberships = substancesWithExposure.ToDictionary(r => r, r => 1d);
            var maximumCumulativeRatioSection = new MaximumCumulativeRatioSection();
            maximumCumulativeRatioSection.Summarize(
                DriverSubstanceCalculator.CalculateExposureDrivers(exposure),
                TargetUnit.FromExternalExposureUnit(ExternalExposureUnit.ugPerKgBWPerDay),
                ExposureApproachType.RiskBased,
                0,
                new double[] { },
                0,
                0
            );
            OxyPlotChartCreator chart = new DriverSubstancesEllipsChartCreator(maximumCumulativeRatioSection);
            chart.CreateToSvg(TestUtilities.ConcatWithOutputPath("mcrDriversEllipsTest3.svg"));
            chart = new DriverSubstancesChartCreator(maximumCumulativeRatioSection);
            chart.CreateToSvg(TestUtilities.ConcatWithOutputPath("mcrDriversTest3.svg"));
        }

        private static void plotHeatMap(int numberOfIterations, int randomSeed, double sw, int k, ComponentSelectionOverviewSection componentExposureSection, double sigma) {
            var plotName = $"iter{numberOfIterations}_seed{randomSeed}_sW{sw}_K{k}_sigma{sigma}.svg";
            var chart = new NMFHeatMapChartCreator(componentExposureSection);
            chart.CreateToSvg(TestUtilities.ConcatWithOutputPath(plotName));
        }

        private static void plotHeatMap(int numberOfIterations, int randomSeed, double sw, int k, ComponentSelectionOverviewSection componentExposureSection) {
            if (componentExposureSection.Records.Count > 1) {
                var plotName = $"iter{numberOfIterations}_seed{randomSeed}_sW{sw}_K{k}.svg";
                var chart = new NMFHeatMapChartCreator(componentExposureSection);
                chart.CreateToSvg(TestUtilities.ConcatWithOutputPath(plotName));
            }
        }

        /// <summary>
        /// Initialize matrix
        /// </summary>
        /// <param name="sigma"></param>
        /// <param name="normalize"></param>
        /// <param name="random"></param>
        /// <param name="substancesWithExposure"></param>
        /// <returns></returns>
        public GeneralMatrix initialize(double sigma, bool normalize, IRandom random, out List<Compound> substancesWithExposure) {

            var numberOfComponents = 4;
            var maxNumberOfSubstancesPerComponent = 8;
            var minNumberOfSubstancesPerComponent = 1;
            var n = 400;

            var numberOfSubstancesPerComponent = new List<int>(numberOfComponents);
            for (int i = 0; i < numberOfComponents; i++) {
                numberOfSubstancesPerComponent.Add(random.Next(minNumberOfSubstancesPerComponent, maxNumberOfSubstancesPerComponent + 1));

            }
            var numberOfSubstances = numberOfSubstancesPerComponent.Sum();
            var nCols = new List<double>();
            for (int i = 0; i < numberOfComponents; i++) {
                nCols.Add(random.NextDouble());
            }
            var sum = nCols.Sum();
            for (int i = 0; i < numberOfComponents; i++) {
                nCols[i] = BMath.Floor(nCols[i] / sum * n);
                if (nCols[i] == 0) {
                    nCols[i] = 1;
                }
            }
            var nCol = (int)nCols.Sum();

            substancesWithExposure = new List<Compound>();
            for (int i = 0; i < numberOfComponents; i++) {
                var ii = i + 1;
                for (int j = 0; j < numberOfSubstancesPerComponent[i]; j++) {
                    var jj = j + 1;
                    substancesWithExposure.Add(new Compound() {
                        Code = "S" + ii + "_" + jj,
                        Name = "S" + ii + "_" + jj,
                    });
                }
            }

            var ones = Enumerable.Repeat(1.0, numberOfComponents).ToArray();
            var diag = GeneralMatrix.CreateDiagonal(ones);

            var u = new List<double>();
            for (int i = 0; i < numberOfComponents; i++) {
                for (int j = 0; j < numberOfComponents; j++) {
                    for (int b = 0; b < numberOfSubstancesPerComponent[j]; b++) {
                        if (diag.Array[i][j] == 1) {
                            u.Add(Math.Abs(NormalDistribution.InvCDF(0, 1, random.NextDouble(0, 1)) + 1));

                        } else {
                            u.Add(0);
                        }
                    }
                }
            }

            var _U = new GeneralMatrix(u.ToArray(), numberOfSubstances);

            var v = new List<double>();
            for (int i = 0; i < numberOfComponents; i++) {
                for (int j = 0; j < numberOfComponents; j++) {
                    for (int b = 0; b < nCols[j]; b++) {
                        if (diag.Array[i][j] == 1) {
                            v.Add(1);
                        } else {
                            v.Add(0);
                        }
                    }
                }
            }
            //array is read in per column
            var V = new GeneralMatrix(v.ToArray(), nCol);
            V = V.Transpose();
            var exposure = _U.Multiply(V);
            var noise = new List<double>();
            for (int i = 0; i < numberOfSubstances * nCol; i++) {
                noise.Add(NormalDistribution.InvCDF(0, 1, random.NextDouble(0, 1)) * sigma);
            }
            var _noise = new GeneralMatrix(noise.ToArray(), numberOfSubstances);
            exposure = exposure.Add(_noise);
            exposure = exposure.ReplaceNegativeAssign();

            if (normalize) {
                var sd = exposure.Array.Select(c => Math.Sqrt(c.Variance())).ToArray();
                //Read se matrix
                var se = GeneralMatrix.CreateDiagonal(sd);
                var seInverse = se.Inverse();
                //Normalize exposure matrix
                return seInverse.Multiply(exposure);
            } else {
                return exposure;
            }
        }

        private GeneralMatrix initialize(List<ExposurePerIndividualCompoundRecord> data, out List<Compound> substancesWithExposure) {
            var intakesPerSubstance = data.GroupBy(gr => gr.Compound)
                .Select(c => {
                    var intakes = c.Select(i => i.Intake).ToList();
                    return new {
                        compound = c.Key,
                        intake = intakes,
                        sum = intakes.Sum(),
                        sd = Math.Sqrt(intakes.Variance())
                    };
                })
                .ToList();

            //Remove empty substances
            var itemsToRemove = intakesPerSubstance.Where(r => r.sum == 0).ToList();
            foreach (var item in itemsToRemove) {
                intakesPerSubstance.Remove(item);
            }

            substancesWithExposure = intakesPerSubstance.Select(c => c.compound).ToList();
            //Read exposure matrix
            Func<int, int, double> exposureDelegate = (i, j) => intakesPerSubstance[i].intake[j];
            return new GeneralMatrix(intakesPerSubstance.Count, intakesPerSubstance.First().intake.Count, exposureDelegate);
        }
    }
}
