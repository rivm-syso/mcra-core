using MCRA.General;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualDayConcentrationCalculation {
    public sealed class HbmIndividualDayCollection {

        public ExposureTarget Target { get; set; }

        public TargetUnit TargetUnit { get; set; }

        public ICollection<HbmIndividualDayConcentration> HbmIndividualDayConcentrations { get; set; }

        public HbmIndividualDayCollection Clone() {
            return new HbmIndividualDayCollection() {
                Target = Target,
                TargetUnit = TargetUnit,
                HbmIndividualDayConcentrations = HbmIndividualDayConcentrations
                    .Select(r => r.Clone())
                    .ToList()
            };
        }

    }
}
