using MCRA.General;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualDayConcentrationCalculation {
    public sealed class HbmIndividualDayCollection {

        /// <summary>
        /// The target unit.
        /// </summary>
        public TargetUnit TargetUnit { get; set; }

        /// <summary>
        /// The exposure target of the collection.
        /// </summary>
        public ExposureTarget Target {
            get {
                return TargetUnit.Target;
            }
        }

        /// <summary>
        /// The HBM individual day concentrations for the target.
        /// </summary>
        public ICollection<HbmIndividualDayConcentration> HbmIndividualDayConcentrations { get; set; }

        public HbmIndividualDayCollection Clone() {
            return new HbmIndividualDayCollection() {
                TargetUnit = TargetUnit,
                HbmIndividualDayConcentrations = HbmIndividualDayConcentrations
                    .Select(r => r.Clone())
                    .ToList()
            };
        }

        public override string ToString() {
            return TargetUnit.ToString() + $" ({nameof(HbmIndividualDayConcentration)}(s): {HbmIndividualDayConcentrations.Count})";
        }

    }
}
