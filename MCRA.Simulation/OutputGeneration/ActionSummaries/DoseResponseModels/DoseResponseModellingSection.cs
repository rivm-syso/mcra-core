using MCRA.Utils.ExtensionMethods;
using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class DoseResponseModellingSection : SummarySection {

        public List<DoseResponseModelSection> DoseResponseModels { get; set; }
        public List<EffectRepresentationRecord> EffectResponseCombinations { get; set; }

        public void Summarize(
            ICollection<DoseResponseExperiment> experiments,
            ICollection<DoseResponseModel> doseResponseModels,
            ICollection<EffectRepresentation> effectRepresentations,
            Compound referenceCompound
        ) {
            var responsesRepresented = effectRepresentations?.Select(r => r.Response).Distinct().ToList();
            var allResponsesEffect = doseResponseModels.Select(c => c.Response).Distinct().ToList();
            var responsesNotRepresented = allResponsesEffect.Where(c => !responsesRepresented?.Contains(c) ?? true).ToList();

            EffectResponseCombinations = effectRepresentations?
                .Join(responsesNotRepresented,
                    c => c.Response.Code,
                    c => c.Code,
                    (first, response) => new EffectRepresentationRecord() {
                        EffectName = first.Effect.Name,
                        EffectCode = first.Effect.Code,
                        ResponseName = response.Name,
                        ResponseCode = response.Code
                    })
                .ToList();

            var result = new List<DoseResponseModelSection>();
            var experimentsDictionary = new Dictionary<string, DoseResponseExperiment>();
            if (experiments == null) {
                experimentsDictionary = doseResponseModels
                    .ToDictionary(c => c.IdExperiment, c =>
                        new DoseResponseExperiment() {
                            Code = c.IdExperiment,
                            Responses = [new Response() { Code = c.Response.Code }],
                            Substances = c.Substances
                        }
                    );
            }

            var experimentsLookup = experiments?.ToDictionary(r => r.Code, StringComparer.InvariantCultureIgnoreCase) ?? experimentsDictionary;
            var effectRepresentationsLookup = effectRepresentations?.ToLookup(r => r.Response);
            foreach (var model in doseResponseModels) {
                var record = new DoseResponseModelSection();
                var experiment = experimentsLookup.ContainsKey(model.IdExperiment) ? experimentsLookup[model.IdExperiment] : null;
                var representations = (effectRepresentationsLookup?.Contains(model.Response) ?? false) ?
                    effectRepresentationsLookup[model.Response].ToList() : [];
                record.Summarize(model, experiment, model.Response, referenceCompound, representations);
                result.Add(record);
            }
            DoseResponseModels = result
                .OrderBy(r => r.ResponseCode, StringComparer.OrdinalIgnoreCase)
                .ThenBy(r => r.ExperimentCode, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
    }
}
