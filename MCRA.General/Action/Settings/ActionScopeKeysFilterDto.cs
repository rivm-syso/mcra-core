using System;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.General.Action.Settings.Dto {

    public class ActionScopeKeysFilterDto {

        public virtual ScopingType ScopingType { get; set; }

        public virtual HashSet<string> SelectedCodes { get; set; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public void SetCodesScope(IEnumerable<string> codes) {
            if (codes?.Any() ?? false) {
                SelectedCodes = codes.ToHashSet(StringComparer.OrdinalIgnoreCase);
            } else {
                SelectedCodes = null;
            }
        }
    }
}
