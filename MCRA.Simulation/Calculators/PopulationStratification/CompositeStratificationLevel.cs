namespace MCRA.Simulation.Calculators.Stratification {
    public class CompositeStratificationLevel : IStratificationLevel, IEquatable<IStratificationLevel> {

        public List<IStratificationLevel> Parts { get; private set; }

        public string Code => string.Join("_", Parts.Select(c => c.Code));
        public string Name => string.Join(", ", Parts.Select(c => c.Name));

        public CompositeStratificationLevel(List<IStratificationLevel> parts) {
            Parts = parts;
        }

        public override bool Equals(object obj) {
            return Equals(obj as IStratificationLevel);
        }

        public bool Equals(IStratificationLevel other) {
            if (ReferenceEquals(this, other)) {
                return true;
            }

            if (other is null) {
                return false;
            }

            if (Parts == null && (other as CompositeStratificationLevel)?.Parts == null) {
                return true;
            }

            if (Parts == null || (other as CompositeStratificationLevel)?.Parts == null) {
                return false;
            }

            return Parts.Select(p => p.Code)
                .SequenceEqual((other as CompositeStratificationLevel).Parts.Select(p => p.Code));
        }

        public override int GetHashCode() {
            if (Parts == null) {
                return 0;
            }

            var hash = 17;
            foreach (var part in Parts) {
                hash = hash * 23 + (part.Code?.GetHashCode() ?? 0);
            }
            return hash;
        }
    }
}
