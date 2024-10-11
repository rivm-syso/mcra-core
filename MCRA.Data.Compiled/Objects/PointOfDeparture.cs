using MCRA.General;

namespace MCRA.Data.Compiled.Objects {
    public sealed class PointOfDeparture {
        public PointOfDeparture() {
            PointOfDepartureUncertains = new HashSet<PointOfDepartureUncertain>();
        }

        public Compound Compound { get; set; }
        public Effect Effect { get; set; }
        public ExpressionType ExpressionType { get; set; }
        public BiologicalMatrix BiologicalMatrix { get; set; }
        public TargetLevelType TargetLevel { get; set; }
        public string Code { get; set; }
        public string Species { get; set; }
        public string DoseResponseModelEquation { get; set; }
        public string DoseResponseModelParameterValues { get; set; }
        public double LimitDose { get; set; }
        public double CriticalEffectSize { get; set; }
        public bool IsCriticalEffect { get; set; }
        public string PublicationTitle { get; set; }
        public string PublicationAuthors { get; set; }
        public int? PublicationYear { get; set; }
        public string PublicationUri { get; set; }

        public ICollection<PointOfDepartureUncertain> PointOfDepartureUncertains { get; set; }

        public DoseUnit DoseUnit { get; set; } = DoseUnit.mgPerKgBWPerDay;

        public PointOfDepartureType PointOfDepartureType { get; set; }

        public ExposureRoute ExposureRoute { get; set; }

        public TargetUnit TargetUnit {
            get {
                if (TargetLevel == TargetLevelType.External) {
                    return TargetUnit.FromExternalDoseUnit(DoseUnit, ExposureRoute);
                } else {
                    return TargetUnit.FromInternalDoseUnit(DoseUnit, BiologicalMatrix, ExpressionType);
                }
            }
        }

        /// <summary>
        /// Creates a shallow copy of this object.
        /// </summary>
        public PointOfDeparture Clone() {
            return new PointOfDeparture() {
                Code = this.Code,
                Compound = this.Compound,
                Effect = this.Effect,
                Species = this.Species,
                ExposureRoute = this.ExposureRoute,
                PointOfDepartureType = this.PointOfDepartureType,
                LimitDose = this.LimitDose,
                PointOfDepartureUncertains = this.PointOfDepartureUncertains,
                DoseResponseModelEquation = this.DoseResponseModelEquation,
                DoseResponseModelParameterValues = this.DoseResponseModelParameterValues,
                DoseUnit = this.DoseUnit,
                CriticalEffectSize = this.CriticalEffectSize,
                IsCriticalEffect = this.IsCriticalEffect,
                PublicationTitle = this.PublicationTitle,
                PublicationAuthors = this.PublicationAuthors,
                PublicationYear = this.PublicationYear,
                PublicationUri = this.PublicationUri,
                TargetLevel = this.TargetLevel,
                BiologicalMatrix = this.BiologicalMatrix,
                ExpressionType = this.ExpressionType,
            };
        }
    }
}
