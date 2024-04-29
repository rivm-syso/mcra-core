using MCRA.General;

namespace MCRA.Data.Compiled.Objects {
    [Serializable]
    public sealed class KineticModelInstance {
        private string _name;

        private List<string> _compartmentCodes;

        private int _numberOfDosesPerDay = 1;
        public string IdModelInstance { get; set; }
        public string IdModelDefinition { get; set; }
        public string IdTestSystem { get; set; }
        public string Reference { get; set; }
        public string Name {
            get {
                if (string.IsNullOrEmpty(_name)) {
                    return IdModelInstance;
                }
                return _name;
            }
            set {
                _name = value;
            }
        }

        public string Description { get; set; }

        public List<Compound> Substances {
            get {
                return KineticModelSubstances?
                    .Select(r => r.Substance)
                    .ToList();
            }
        }

        public List<KineticModelSubstance> KineticModelSubstances { get; set; }

        // TODO kinetic modelling: should be a list when we want to support
        // multiple kinetic models
        public Compound InputSubstance {
            get {
                return KineticModelSubstances?
                    .Where(r => r.SubstanceDefinition?.IsInput ?? true)
                    .Select(r => r.Substance)
                    .Single() ?? Substances.Single();
            }
        }

        public bool UseParameterVariability { get; set; }
        public List<string> CompartmentCodes {
            get { return _compartmentCodes; }
            set {
                _compartmentCodes = value;
                BiologicalMatrices = value.Select(c => BiologicalMatrixConverter.FromString(c)).ToList();
            }
        }

        public List<BiologicalMatrix> BiologicalMatrices { get; set; }

        public int NumberOfDays { get; set; } = 50;

        public int NumberOfDosesPerDay {
            get {
                if (SpecifyEvents) {
                    return SelectedEvents.Length;
                }
                return _numberOfDosesPerDay;
            }
            set {
                _numberOfDosesPerDay = value;
            }
        }

        public int NumberOfDosesPerDayNonDietaryOral { get; set; } = 1;
        public int NumberOfDosesPerDayNonDietaryDermal { get; set; } = 1;
        public int NumberOfDosesPerDayNonDietaryInhalation { get; set; } = 1;
        public int NonStationaryPeriod { get; set; } = 10;
        public bool SpecifyEvents { get; set; }
        public int[] SelectedEvents { get; set; }

        public IDictionary<string, KineticModelInstanceParameter> KineticModelInstanceParameters { get; set; }

        public KineticModelDefinition KineticModelDefinition { get; set; }

        public TimeUnit ResolutionType {
            get {
                return TimeUnitConverter.FromString(KineticModelDefinition?.Resolution ?? "NotSpecified");
            }
        }

        public bool IsHumanModel {
            get {
                return IdTestSystem != null && IdTestSystem.Equals("Human", StringComparison.InvariantCultureIgnoreCase);
            }
        }

        public KineticModelInstance Clone() {
            var instance = new KineticModelInstance() {
                IdModelDefinition = this.IdModelDefinition,
                IdModelInstance = this.IdModelInstance,
                IdTestSystem = this.IdTestSystem,
                KineticModelDefinition = this.KineticModelDefinition,
                Reference = this.Reference,
                KineticModelInstanceParameters = this.KineticModelInstanceParameters,
                UseParameterVariability = this.UseParameterVariability,
                CompartmentCodes = this.CompartmentCodes,
                NumberOfDays = this.NumberOfDays,
                NumberOfDosesPerDay = this.NumberOfDosesPerDay,
                NumberOfDosesPerDayNonDietaryDermal = this.NumberOfDosesPerDayNonDietaryDermal,
                NumberOfDosesPerDayNonDietaryInhalation = this.NumberOfDosesPerDayNonDietaryInhalation,
                NumberOfDosesPerDayNonDietaryOral = this.NumberOfDosesPerDayNonDietaryOral,
                NonStationaryPeriod = this.NonStationaryPeriod,
                SpecifyEvents = this.SpecifyEvents,
                SelectedEvents = this.SelectedEvents,
                KineticModelSubstances = this.KineticModelSubstances,
            };
            return instance;
        }

        public KineticModelType KineticModelType {
            get {
                return MCRAKineticModelDefinitions.FromString(this.IdModelDefinition);
            }
        }

        public bool HasMetabolites() {
            return KineticModelSubstances
                .Any(r => r.SubstanceDefinition != null && !r.SubstanceDefinition.IsInput);
        }
    }
}
