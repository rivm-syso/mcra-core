namespace MCRA.General.Action.Settings {

    public class ScopeKeysFilter {

        public virtual ScopingType ScopingType { get; set; }

        public virtual HashSet<string> SelectedCodes { get; set; } = new(StringComparer.OrdinalIgnoreCase);

        public void SetCodesScope(IEnumerable<string> codes) {
            if (codes?.Any() ?? false) {
                SelectedCodes = codes.ToHashSet(StringComparer.OrdinalIgnoreCase);
            } else {
                SelectedCodes = null;
            }
        }
    }
}
