using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Objects;
using MCRA.General;

namespace MCRA.Simulation.Test.UnitTests.Calculators.ActiveSubstanceAllocation {
    /// <summary>
    /// Aggregate membership model calculator tests.
    /// </summary>
    [TestClass]
    public abstract class ActiveSubstanceAllocationCalculatorTestsBase {

        protected static List<SampleCompoundCollection> fakeSampleCompoundCollection(
            Food food,
            params SampleCompoundRecord[] sampleSubstanceRecords
        ) {
            return [
                new SampleCompoundCollection(
                    food,
                    sampleSubstanceRecords.ToList()
                )
            ];
        }

        protected static SampleCompoundRecord fakeSampleSubstanceRecord(
            List<Compound> measuredSubstances,
            double[] concentrations
        ) {
            return new SampleCompoundRecord() {
                SampleCompounds = measuredSubstances
                    .Select((r, ix) => {
                        var missing = double.IsNaN(concentrations[ix]);
                        var resType = double.IsNaN(concentrations[ix]) ?
                            ResType.MV
                            : (concentrations[ix] >= 0) ? ResType.VAL : ResType.LOQ;
                        return new SampleCompound() {
                            MeasuredSubstance = r,
                            Residue = resType == ResType.VAL ? concentrations[ix] : double.NaN,
                            ResType = resType,
                            Loq = resType == ResType.LOQ ? -concentrations[ix] : double.NaN,
                        };
                    })
                    .ToDictionary(r => r.MeasuredSubstance)
            };
        }

        protected static SubstanceConversion createSubstanceConversion(
            Compound activeSubstance,
            Compound measuredSubstance,
            double conversionFactor = 1,
            bool isExclusive = true,
            double proportion = 1
        ) {
            return new SubstanceConversion() {
                ActiveSubstance = activeSubstance,
                MeasuredSubstance = measuredSubstance,
                ConversionFactor = conversionFactor,
                IsExclusive = isExclusive,
                Proportion = proportion
            };
        }
    }
}
