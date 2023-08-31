using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ComponentCalculation.DriverSubstanceCalculation;
using MCRA.Simulation.Calculators.ComponentCalculation.ExposureMatrixCalculation;

namespace MCRA.Simulation.OutputGeneration {
    public class MaximumCumulativeRatioSection : SummarySection {
        public List<DriverCompoundRecord> DriverCompounds { get; set; }
        public List<DriverCompoundStatisticsRecord> DriverCompoundStatisticsRecords { get; set; }
        public double RatioCutOff { get; set; }
        public double CumulativeExposureCutOffPercentage { get; set; }
        public TargetUnit TargetUnit { get; set; }
        public bool RiskBased { get; set; }

        public double[] Percentiles { get; set; }
        public double MinimumPercentage { get; set; }
        public bool IsNewImplementation { get; set; }

        public List<MCRDrilldownRecord> MCRDrilldownRecords { get; set; }

        public void Summarize(
            List<DriverSubstance> driverSubstances,
            TargetUnit targetUnit,
            ExposureApproachType exposureApproachType,
            double ratioCutOff,
            double[] percentiles,
            double totalExposureCutOffPercentage,
            double minimumPercentage
        ) {
            IsNewImplementation = true;
            if (exposureApproachType == ExposureApproachType.RiskBased) {
                RiskBased = true;
            }
            TargetUnit = targetUnit;
            Percentiles = percentiles;
            RatioCutOff = ratioCutOff;
            CumulativeExposureCutOffPercentage = totalExposureCutOffPercentage;
            MinimumPercentage = minimumPercentage;
            DriverCompounds = new List<DriverCompoundRecord>();
            foreach (var item in driverSubstances) {
                DriverCompounds.Add(new DriverCompoundRecord() {
                    CompoundCode = item.Compound.Code,
                    CompoundName = item.Compound.Name,
                    Ratio = item.MaximumCumulativeRatio,
                    CumulativeExposure = item.CumulativeExposure,
                });
            }

            DriverCompoundStatisticsRecords = driverSubstances.GroupBy(gr => gr.Compound)
                .Select(g => {
                    var logTotalExposure = g.Select(c => Math.Log(c.CumulativeExposure)).ToList();
                    var logRatio = g.Select(c => Math.Log(c.MaximumCumulativeRatio)).ToList();
                    var bivariate = getBivariateParameters(logTotalExposure, logRatio);
                    return new DriverCompoundStatisticsRecord {
                        CompoundName = g.Key.Name,
                        CompoundCode = g.Key.Code,
                        CumulativeExposureMedian = Math.Exp(bivariate[0]),
                        CVCumulativeExposure = bivariate[2],
                        RatioMedian = Math.Exp(bivariate[1]),
                        CVRatio = bivariate[3],
                        R = bivariate[4],
                        Number = logTotalExposure.Count,
                    };
                })
                .OrderBy(c => c.CumulativeExposureMedian)
                .ToList();
        }

        public void Summarize(
                ExposureMatrix exposureMatrix,
                double[] percentiles,
                double minimumPercentage
            ) {
            var cancelToken = ProgressState?.CancellationToken ?? new System.Threading.CancellationToken();
            var coExposures = new List<List<double>>();
            var exposureTranspose = exposureMatrix.Exposures.Transpose();
            foreach (var item in exposureTranspose.Array) {
                coExposures.Add(item.ToList());
            }

            var substances = exposureMatrix.Substances;
            coExposures = coExposures.Where(c => c.Sum() > 0).OrderByDescending(c => c.Sum()).ToList();
            MCRDrilldownRecords = new List<MCRDrilldownRecord>();
            percentiles = percentiles.OrderBy(c => c).ToArray();
            foreach (var percentage in percentiles) {
                var take = Convert.ToInt32((100 - percentage) * coExposures.Count / 100);
                if (take > 0) {
                    var selectedCoExposures = coExposures.Take(take).ToList();
                    var exposureDictionary = new Dictionary<Compound, double>();
                    var ix = 0;
                    foreach (var substance in substances) {
                        exposureDictionary[substance] = 0;
                        foreach (var item in selectedCoExposures) {
                            exposureDictionary[substance] += item[ix];
                        }
                        ix++;
                    }
                    var totalExposure = exposureDictionary.Values.Sum();

                    var selectedSubstances = substances
                        .Where(substance => exposureDictionary[substance] / totalExposure * 100 > minimumPercentage)
                        .ToList();

                    var resultMCR = selectedCoExposures
                        .AsParallel()
                        .WithCancellation(cancelToken)
                        .Select(c => new MCRRecord {
                            Ratio = c.Sum() / c.Max(),
                            SubstanceNames = getCompoundNames(c, substances, selectedSubstances)
                        })
                        .Where(c => c.Ratio > 0)
                        .ToList();

                    var mcr1 = resultMCR.Where(c => c.Ratio == 1).ToList();
                    var mcr2 = resultMCR.Where(c => c.Ratio > 1 && c.Ratio <= 2).ToList();
                    var mcr3 = resultMCR.Where(c => c.Ratio > 2).ToList();
                    var mcrDrilldownRecord = new MCRDrilldownRecord();
                    mcrDrilldownRecord.Tail = percentage;
                    mcrDrilldownRecord.Percentage1 = mcr1.Count * 100d / resultMCR.Count;
                    mcrDrilldownRecord.Percentage2 = mcr2.Count * 100d / resultMCR.Count;
                    mcrDrilldownRecord.Percentage3 = mcr3.Count * 100d / resultMCR.Count;
                    mcrDrilldownRecord.Substances1 = string.Join(", ", mcr1.SelectMany(c => c.SubstanceNames).Distinct().OrderBy(c => c, StringComparer.OrdinalIgnoreCase));
                    mcrDrilldownRecord.Substances2 = string.Join(", ", mcr2.SelectMany(c => c.SubstanceNames).Distinct().OrderBy(c => c, StringComparer.OrdinalIgnoreCase));
                    mcrDrilldownRecord.Substances3 = string.Join(", ", mcr3.SelectMany(c => c.SubstanceNames).Distinct().OrderBy(c => c, StringComparer.OrdinalIgnoreCase));
                    mcrDrilldownRecord.Number = resultMCR.Count;
                    MCRDrilldownRecords.Add(mcrDrilldownRecord);
                }
            }
        }

        private static List<string> getCompoundNames(List<double> exposures, ICollection<Compound> activeSubstances, ICollection<Compound> selectedSubstances) {
            var names = new List<string>();
            for (int i = 0; i < exposures.Count; i++) {
                if (exposures[i] > 0) {
                    var substance = activeSubstances.ElementAt(i);
                    if (selectedSubstances.Contains(substance)) {
                        names.Add(substance.Name);
                    }
                }
            }
            return names;
        }

        /// <summary>
        /// x, y is normal after log transformation (logscale)
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private List<double> getBivariateParameters(List<double> x, List<double> y) {
            var x_muX = new List<double>();
            var y_muY = new List<double>();

            var bivariate = new List<double>(5);
            var muX = x.Average();
            var muY = y.Average(); ;
            var sdX = Math.Sqrt(x.Variance());
            var sdY = Math.Sqrt(y.Variance());

            for (int i = 0; i < x.Count; i++) {
                x_muX.Add(x[i] - muX);
                y_muY.Add(y[i] - muY);
            }

            var sumXY = 0d;
            var sumX = 0d;
            var sumY = 0d;
            for (int i = 0; i < x.Count; i++) {
                sumXY += x_muX[i] * y_muY[i];
                sumX += x_muX[i] * x_muX[i];
                sumY += y_muY[i] * y_muY[i];
            }
            var correlation = sumXY / Math.Sqrt(sumX * sumY);
            bivariate.Add(muX);
            bivariate.Add(muY);
            bivariate.Add(sdX);
            bivariate.Add(sdY);
            bivariate.Add(correlation);
            return bivariate;
        }
    }
}
