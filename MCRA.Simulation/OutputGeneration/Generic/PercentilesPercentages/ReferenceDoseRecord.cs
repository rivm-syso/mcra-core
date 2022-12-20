using MCRA.Simulation.Calculators.HazardCharacterisationCalculation;

namespace MCRA.Simulation.OutputGeneration {

    /// <summary>
    /// Reference dose summary record.
    /// </summary>
    public sealed class ReferenceDoseRecord {

        public string Name { get; set; }
        public string Code { get; set; }
        public string Type { get; set; }
        public double Dose { get; set; } = double.NaN;

        /// <summary>
        /// Obsolete?
        /// </summary>
        public double SafetyFactor { get; set; } = double.NaN;

        /// <summary>
        /// Obsolete?
        /// </summary>
        public bool IsAcute { get; set; }

        /// <summary>
        /// Obsolete? Dit gaat niet gebruikt worden vermoed ik
        /// </summary>
        public double ReferenceDose {
            get {
                var result = Dose * SafetyFactor;
                return (result > 0) ? result : double.NaN;
            }
        }

        public static ReferenceDoseRecord FromHazardCharacterisation(IHazardCharacterisationModel reference) {
            if (reference == null) {
                return null;
            }
            return new ReferenceDoseRecord() {
                Name = reference.Substance.Name,
                Code = reference.Substance.Code,
                Dose = reference.Value,
                Type = reference.HazardCharacterisationType.ToString()
            };
        }
    }
}
