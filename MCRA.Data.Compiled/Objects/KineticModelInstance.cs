using MCRA.General;
using System;
using System.Collections.Generic;

namespace MCRA.Data.Compiled.Objects {
    [Serializable]
    public sealed class KineticModelInstance {
        private string _name;
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

        public Compound Substance { get; set; }

        public bool UseParameterVariability { get; set; }
        public string CodeCompartment { get; set; }
        public int NumberOfDays { get; set; } = 50;
        public int NumberOfDosesPerDay { get; set; } = 1;
        public int NumberOfDosesPerDayNonDietaryOral { get; set; } = 1;
        public int NumberOfDosesPerDayNonDietaryDermal { get; set; } = 1;
        public int NumberOfDosesPerDayNonDietaryInhalation { get; set; } = 1;
        public int NonStationaryPeriod { get; set; } = 10;

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
                Substance = this.Substance,
                KineticModelInstanceParameters = this.KineticModelInstanceParameters,
                UseParameterVariability = this.UseParameterVariability,
                CodeCompartment = this.CodeCompartment,
                NumberOfDays = this.NumberOfDays,
                NumberOfDosesPerDay = this.NumberOfDosesPerDay,
                NumberOfDosesPerDayNonDietaryDermal = this.NumberOfDosesPerDayNonDietaryDermal,
                NumberOfDosesPerDayNonDietaryInhalation = this.NumberOfDosesPerDayNonDietaryInhalation,
                NumberOfDosesPerDayNonDietaryOral = this.NumberOfDosesPerDayNonDietaryOral,
                NonStationaryPeriod = this.NonStationaryPeriod,
            };
            return instance;
        }

        public KineticModelType KineticModelType {
            get {
                return MCRAKineticModelDefinitions.FromString(this.IdModelDefinition);
            }
        }
    }
}
