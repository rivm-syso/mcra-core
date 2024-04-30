using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.General.ModuleDefinitions.Settings;

namespace MCRA.Simulation.Action {

    public class ModuleOutputSectionsManager<TEnum>
        where TEnum : struct, IConvertible, IComparable, IFormattable {

        private readonly ActionType _actionType;

        private readonly bool _isHeaderSelectionOptOut;

        private readonly HashSet<string> _sectionsSelectionList = null;

        public ModuleOutputSectionsManager(ActionModuleConfig outputSettings, ActionType actionType) {
            _actionType = actionType;
            _sectionsSelectionList = outputSettings.OutputSections?.ToHashSet(StringComparer.OrdinalIgnoreCase);
            _isHeaderSelectionOptOut = outputSettings.OutputSectionSelectionMethod == OutputSectionSelectionMethod.OptOut;
            if ((_sectionsSelectionList?.Any() ?? false) && !_isHeaderSelectionOptOut) {
                var actionList = _sectionsSelectionList
                    .Where(c => c.Contains(':'))
                    .Select(c => c.Split(':')[0])
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();
                _sectionsSelectionList.UnionWith(actionList);
            }
        }

        public bool ShouldSummarizeModuleOutput() {
            return shouldProcessOutputSection(_actionType.ToString());
        }

        public bool ShouldSummarize(TEnum section) {
            return shouldProcessOutputSection($"{_actionType}:{section}")
                || (!_isHeaderSelectionOptOut && shouldProcessOutputSection($"{_actionType}:*"));
        }

        private bool shouldProcessOutputSection(string sectionName) {
            if (!_sectionsSelectionList?.Any() ?? true) {
                //No sections mentioned: always process
                return true;
            }
            var hasSection = _sectionsSelectionList.Contains(sectionName);
            return _isHeaderSelectionOptOut ? !hasSection : hasSection;
        }
    }
}
