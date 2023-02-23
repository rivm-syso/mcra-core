using MCRA.General;

namespace MCRA.Data.Compiled.Objects {
    public sealed class PointOfDeparture {
        public PointOfDeparture() {
            PointOfDepartureUncertains = new HashSet<PointOfDepartureUncertain>();
        }

        public Compound Compound { get; set; }
        public Effect Effect { get; set; }

        public string Code { get; set; }
        public string Species { get; set; }
        public string DoseResponseModelEquation { get; set; }
        public string DoseResponseModelParameterValues { get; set; }
        public double LimitDose { get; set; }
        public string DoseUnitString { get; set; }
        public string PointOfDepartureTypeString { get; set; }
        public double CriticalEffectSize { get; set; }
        public string ExposureRouteTypeString { get; set; }
        public bool IsCriticalEffect { get; set; }

        public ICollection<PointOfDepartureUncertain> PointOfDepartureUncertains { get; set; }

        public DoseUnit DoseUnit {
            get {
                return DoseUnitConverter.FromString(this.DoseUnitString, DoseUnit.mgPerKgBWPerDay);
            }
        }

        public PointOfDepartureType PointOfDepartureType {
            get {
                return PointOfDepartureTypeConverter.FromString(PointOfDepartureTypeString, PointOfDepartureType.Bmd);
            }
        }

        public ExposureRouteType ExposureRoute {
            get {
                var exposureRoute = ExposureRouteTypeConverter.FromString(ExposureRouteTypeString);
                if (exposureRoute == ExposureRouteType.Undefined) {
                    return ExposureRouteType.Dietary;
                }
                return exposureRoute;
            }
        }

        /// <summary>
        /// Creates a shallow copy of this object.
        /// </summary>
        /// <returns></returns>
        public PointOfDeparture Clone() {
            return new PointOfDeparture() {
                Code = this.Code,
                Compound = this.Compound,
                Effect = this.Effect,
                Species = this.Species,
                ExposureRouteTypeString = this.ExposureRouteTypeString,
                PointOfDepartureTypeString = this.PointOfDepartureTypeString,
                LimitDose = this.LimitDose,
                PointOfDepartureUncertains = this.PointOfDepartureUncertains,
                DoseResponseModelEquation = this.DoseResponseModelEquation,
                DoseResponseModelParameterValues = this.DoseResponseModelParameterValues,
                DoseUnitString = this.DoseUnitString,
                CriticalEffectSize = this.CriticalEffectSize,
                IsCriticalEffect = this.IsCriticalEffect
            };
        }
    }
}
