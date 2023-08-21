using MCRA.General;
using MCRA.Simulation.Calculators.RiskCalculation;

namespace MCRA.Simulation.OutputGeneration {
    public class AtRiskSectionBase : SummarySection {

        private const double _eps = 10E7D;
        public HealthEffectType HealthEffectType;
        public double Threshold;

        /// </summary>
        /// Calculate percentages at risk. For single foods no background is available, calculate only foreground at risk.
        /// Note, threshold is riskmetric dependent.
        /// The following is calculated for a Risk (MOE, HI) with threshold = 1. Suppose three days with MOE's are available.
        /// For MOE: Background Background+Food  Background Background+Food
        ///                                      MOE <= threshold = at risk
        ///                 1.2             0.9           0               1     At risk due to food (%)
        ///                 1.3             1.1           0               0     Not at risk (%)
        ///                 0.8             0.5           1               1     At risk with or without food (%)
        /// For HI:  Background Background+Food  Background Background+Food
        ///                                       HI >= threshold = at risk
        ///                 0.8             1.1           0               1     At risk due to food (%)
        ///                 0.8             0.9           0               0     Not at risk (%)
        ///                 1.3             2.0           1               1     At risk with or without food (%)
        /// </summary>
        /// <param name="individualEffects"></param>
        /// <param name="cumulativeIndividualRisks"></param>
        /// <param name="atRiskDueTo"></param>
        /// <param name="notAtRisk"></param>
        /// <param name="atRiskWithOrWithout"></param>
        /// <returns></returns>
        public (int atRiskDueTo, int notAtRisk, int atRiskWithOrWithout) CalculateHazardExposureRatioAtRisks(
            List<IndividualEffect> individualEffects,
            IDictionary<int, double> cumulativeIndividualRisks,
            int atRiskDueTo,
            int notAtRisk,
            int atRiskWithOrWithout
        ) {
            var maxRisk = CalculateHazardExposureRatio(double.MaxValue, 0);
            var risksDict = individualEffects.ToDictionary(v => v.SimulatedIndividualId, v => v.ExposureHazardRatio);

            foreach (var kvp in cumulativeIndividualRisks) {
                var cumulativeETR = kvp.Value;
                var cumulativeTER = 1 / kvp.Value;
                //not at risk:
                if (cumulativeTER > Threshold) {
                    notAtRisk++;
                }

                var background = 0D;
                if (risksDict.TryGetValue(kvp.Key, out var individualHi)) {
                    if (cumulativeETR - individualHi == 0) {
                        background = maxRisk;
                    } else {
                        background = 1 / (cumulativeETR - individualHi);
                    }
                } else {
                    background = cumulativeTER;
                }

                if (cumulativeTER <= Threshold) {
                    if (background > Threshold) {
                        atRiskDueTo++;
                    } else {
                        atRiskWithOrWithout++;
                    }
                }
            }
            return (atRiskDueTo, notAtRisk, atRiskWithOrWithout);
        }

        /// <summary>
        /// Calculate percentages at risk. For single foods no background is available, calculate only foreground at risk.
        /// Note, threshold is riskmetric dependent.
        /// The following is calculated for a MOE, HI with threshold = 1. Suppose three days with MOE's are available.
        /// For MOE: Background Background+Food  Background Background+Food
        ///                                      MOE <= threshold = at risk
        ///                 1.2             0.9           0               1     At risk due to food (%)
        ///                 1.3             1.1           0               0     Not at risk (%)
        ///                 0.8             0.5           1               1     At risk with or without food (%)
        /// For HI:  Background Background+Food  Background Background+Food
        ///                                       HI >= threshold = at risk
        ///                 0.8             1.1           0               1     At risk due to food (%)
        ///                 0.8             0.9           0               0     Not at risk (%)
        ///                 1.3             2.0           1               1     At risk with or without food (%)
        /// </summary>
        /// <param name="individualEffects"></param>
        /// <param name="cumulativeIndividualRisks"></param>
        /// <param name="atRiskDueTo"></param>
        /// <param name="notAtRisk"></param>
        /// <param name="atRiskWithOrWithout"></param>
        /// <returns></returns>
        public (int atRiskDueTo, int notAtRisk, int atRiskWithOrWithout) CalculateExposureHazardRatioAtRisks(
            List<IndividualEffect> individualEffects,
            IDictionary<int, double> cumulativeIndividualRisks,
            int atRiskDueTo,
            int notAtRisk,
            int atRiskWithOrWithout
        ) {
            var riskDict = individualEffects.ToDictionary(v => v.SimulatedIndividualId, v => v.ExposureHazardRatio);

            foreach (var kvp in cumulativeIndividualRisks) {
                var cumulativeRisk = kvp.Value;
                //not at risk:
                if (cumulativeRisk < Threshold) {
                    notAtRisk++;
                }

                var background = 0D;
                if (riskDict.TryGetValue(kvp.Key, out var individualHi)) {
                    background = cumulativeRisk - individualHi;
                } else {
                    background = cumulativeRisk;
                }

                if (cumulativeRisk >= Threshold) {
                    if (background >= Threshold) {
                        atRiskWithOrWithout++;
                    } else {
                        atRiskDueTo++;
                    }
                }
            }
            return (atRiskDueTo, notAtRisk, atRiskWithOrWithout);
        }

        /// <summary>
        /// Risk or benefit.
        /// </summary>
        /// <param name="iced"></param>
        /// <param name="iexp"></param>
        /// <returns></returns>
        public double CalculateHazardExposureRatio(double iced, double iexp) {
            if (HealthEffectType == HealthEffectType.Benefit) {
                return iced > iexp / _eps ? iexp / iced : _eps;
            } else {
                return iexp > iced / _eps ? iced / iexp : _eps;
            }
        }
    }
}
