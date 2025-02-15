namespace MCRA.Utils.Sbml.Objects {
    public class SbmlUnitDefinition : SbmlModelElement {
        public List<SbmlUnit> Units { get; set; }

        public string GetUnitString() {
            var result = string.Empty;
            for (int i = 0; i < Units.Count; i++) {
                var unitPartString = Units[i].GetUnitString();
                if (!string.IsNullOrEmpty(unitPartString)) {
                    result += unitPartString;
                }
            }
            if (string.IsNullOrEmpty(result)) {
                result = "dimensionless";
            } else if (result.StartsWith('.')) {
                result = result[1..];
            }
            return result;
        }
    }
}
