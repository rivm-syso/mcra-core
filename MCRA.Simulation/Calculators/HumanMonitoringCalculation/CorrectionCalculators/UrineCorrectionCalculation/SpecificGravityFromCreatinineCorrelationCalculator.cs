using MCRA.Data.Compiled.Wrappers;
using MCRA.General;
using MCRA.Simulation.Calculators.HumanMonitoringSampleCompoundCollections;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation.CorrectionCalculators.UrineCorrectionCalculation {
    public class SpecificGravityFromCreatinineCorrelationCalculator : CorrectionCalculator {

        private readonly double _specificGravityConversionFactor = double.NaN;

        public SpecificGravityFromCreatinineCorrelationCalculator(
            List<string> substancesExcludedFromStandardisation, 
            double? specificGravityConversionFactor
        ) : base(substancesExcludedFromStandardisation) {
            _specificGravityConversionFactor = specificGravityConversionFactor.Value;
        }

        public override List<HumanMonitoringSampleSubstanceCollection> ComputeResidueCorrection(
               ICollection<HumanMonitoringSampleSubstanceCollection> hbmSampleSubstanceCollections
           ) {
            var result = new List<HumanMonitoringSampleSubstanceCollection>();
            foreach (var sampleCollection in hbmSampleSubstanceCollections) {
                if (sampleCollection.SamplingMethod.IsUrine) {

                    var unitAlignmentFactor = getUnitAlignment(
                        sampleCollection,
                        out ConcentrationUnit correctedConcentrationUnit,
                        out ExpressionType correctedExpressionType);

                    var correctedSampleSubstanceRecords = sampleCollection.HumanMonitoringSampleSubstanceRecords
                        .Select(sample => {
                            var sampleCompounds = sample.HumanMonitoringSampleSubstances.Values
                                .Select(r => getSampleSubstance(
                                    r,
                                    sample,
                                    unitAlignmentFactor
                                 ))
                                .ToDictionary(c => c.MeasuredSubstance);
                            return new HumanMonitoringSampleSubstanceRecord() {
                                HumanMonitoringSampleSubstances = sampleCompounds,
                                HumanMonitoringSample = sample.HumanMonitoringSample
                            };
                        })
                        .ToList();
                    result.Add(new HumanMonitoringSampleSubstanceCollection(
                        sampleCollection.SamplingMethod,
                        correctedSampleSubstanceRecords,
                        correctedConcentrationUnit,
                        correctedExpressionType,
                        sampleCollection.TriglycConcentrationUnit,
                        sampleCollection.CholestConcentrationUnit,
                        sampleCollection.LipidConcentrationUnit,
                        sampleCollection.CreatConcentrationUnit
                    )
                    );
                } else {
                    result.Add(sampleCollection);
                }
            }
            return result;
        }

        protected override double getUnitAlignment(
            HumanMonitoringSampleSubstanceCollection sampleCollection,
            out ConcentrationUnit targetConcentrationUnit,
            out ExpressionType targetExpressionType
        ) {
            // For specific gravity, derived from creatinine standardised values, the unit remains the original units (gram/L)
            targetConcentrationUnit = sampleCollection.ConcentrationUnit;
            targetExpressionType = sampleCollection.ExpressionType;

            return getAlignmentFactor(
                        sampleCollection.CreatConcentrationUnit,
                        sampleCollection.ConcentrationUnit.GetConcentrationMassUnit()
                    );
        }

        protected override SampleCompound getSampleSubstance(
          SampleCompound sampleSubstance,
          HumanMonitoringSampleSubstanceRecord sampleSubstanceRecord,
          double unitAlignmentFactor = 1
       ) {
            if (sampleSubstance.IsMissingValue) {
                return sampleSubstance;
            }

            if (SubstancesExcludedFromStandardisation.Contains(sampleSubstance.MeasuredSubstance.Code)) {
                return sampleSubstance;
            }

            double? correctionFactor = sampleSubstanceRecord?.HumanMonitoringSample.Creatinine / (unitAlignmentFactor * _specificGravityConversionFactor);

            var clone = sampleSubstance.Clone();
            if (correctionFactor.HasValue) {
                clone.Residue = sampleSubstance.Residue / correctionFactor.Value;
            } else {
                clone.Residue = double.NaN;
                clone.ResType = ResType.MV;
            }

            return clone;
        }
    }
}