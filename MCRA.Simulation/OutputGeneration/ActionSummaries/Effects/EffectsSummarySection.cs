using MCRA.Utils.ExtensionMethods;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class EffectsSummarySection : SummarySection {

        public List<EffectsSummaryRecord> Records { get; set; }

        public void Summarize(ICollection<Effect> effects, string codeEffect) {
            Records = effects.Select(c => new EffectsSummaryRecord() {
                Code = c.Code,
                Name = c.Name,
                Description = c.Description,
                IsMainEffect = codeEffect?.Equals(c.Code, StringComparison.OrdinalIgnoreCase) ?? false,
                BiologicalOrganisation = c.BiologicalOrganisationType != BiologicalOrganisationType.Unspecified
                    ? c.BiologicalOrganisationType.GetDisplayName()
                    : null,
            })
            .OrderByDescending(r => r.IsMainEffect)
            .ToList();
        }
    }
}
