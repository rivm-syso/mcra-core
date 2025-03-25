using MCRA.General;

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

        public List<Compound> Substances => KineticModelSubstances?
                    .Select(r => r.Substance)
                    .ToList();

        public List<KineticModelSubstance> KineticModelSubstances { get; set; }

        public Compound InputSubstance => KineticModelSubstances?
                    .Where(r => r.SubstanceDefinition?.IsInput ?? true)
                    .Select(r => r.Substance)
                    .Single() ?? Substances.Single();

        public IDictionary<string, KineticModelInstanceParameter> KineticModelInstanceParameters { get; set; }

        public PbkModelDefinition PbkModelDefinition { get; set; }

        public KineticModelDefinition KineticModelDefinition { get; set; }

        public bool IsHumanModel => IdTestSystem != null && IdTestSystem.Equals("Human", StringComparison.InvariantCultureIgnoreCase);

        public KineticModelInstance Clone() {
            var instance = new KineticModelInstance() {
                IdModelDefinition = this.IdModelDefinition,
                IdModelInstance = this.IdModelInstance,
                IdTestSystem = this.IdTestSystem,
                KineticModelDefinition = this.KineticModelDefinition,
                Reference = this.Reference,
                KineticModelInstanceParameters = this.KineticModelInstanceParameters,
                KineticModelSubstances = this.KineticModelSubstances,
                PbkModelDefinition = this.PbkModelDefinition,
            };
            return instance;
        }

        public KineticModelType KineticModelType => KineticModelDefinition.Format;

        public bool HasMetabolites() {
            return KineticModelSubstances
                .Any(r => r.SubstanceDefinition != null && !r.SubstanceDefinition.IsInput);
        }
    }
}
