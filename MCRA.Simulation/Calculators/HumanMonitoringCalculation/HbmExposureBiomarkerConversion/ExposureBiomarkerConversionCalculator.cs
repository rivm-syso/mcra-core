using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmExposureBiomarkerConversion.ExposureBiomarkerConversionModels;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualDayConcentrationCalculation;
using MCRA.Utils.Statistics;
using MCRA.Utils.Statistics.RandomGenerators;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmExposureBiomarkerConversion {
    public sealed class ExposureBiomarkerConversionCalculator {

        private readonly ILookup<ExposureTarget, IExposureBiomarkerConversionModel> _conversionsByTarget;

        private readonly ILookup<(Compound substance, ExposureTarget target), IExposureBiomarkerConversionModel> _conversionsLookup;

        public ExposureBiomarkerConversionCalculator(ICollection<IExposureBiomarkerConversionModel> conversions) {
            _conversionsLookup = conversions.ToLookup(c => (c.ConversionRule.SubstanceFrom, new ExposureTarget(c.ConversionRule.BiologicalMatrix, c.ConversionRule.ExpressionTypeFrom)));
            _conversionsByTarget = conversions.ToLookup(r => new ExposureTarget(r.ConversionRule.BiologicalMatrix, r.ConversionRule.ExpressionTypeFrom));
        }

        /// <summary>
        /// Convert to a new substance (target exposure, i.c. biomarker)
        /// </summary>
        /// <param name="hbmIndividualDayCollections"></param>
        /// <param name="seed"></param>
        /// <returns></returns>
        public List<HbmIndividualDayCollection> Convert(
            ICollection<HbmIndividualDayCollection> hbmIndividualDayCollections,
            int seed
        ) {
            var results = hbmIndividualDayCollections.Select(c => c.Clone()).ToList();
            var collectionsLookup = results.ToDictionary(r => r.Target);
            foreach (var collection in results) {
                // Loop over the HBM individual day collections
                if (!_conversionsByTarget.Contains(collection.Target)) {
                    // No conversions for the target of this collection
                    continue;
                }

                // Get the conversions applicable for this target
                var conversionGroups = _conversionsByTarget[collection.Target]
                    .GroupBy(r => r.ConversionRule.SubstanceTo);

                var collectionSeed = RandomUtils.CreateSeed(seed, collection.Target.BiologicalMatrix.GetHashCode());

                // Loop overthe conversions for this target
                foreach (var conversionGroup in conversionGroups) {

                    if (collection.HbmIndividualDayConcentrations.Any(r => r.ConcentrationsBySubstance.ContainsKey(conversionGroup.Key))) {
                        // Target collection already contains the to-substance; don't apply conversion
                        continue;
                    }

                    foreach (var model in conversionGroup) {
                        var conversion = model.ConversionRule;
                        if (collection.HbmIndividualDayConcentrations.Any(r => r.ConcentrationsBySubstance.ContainsKey(conversion.SubstanceFrom))) {

                            // From-substance concentrations found in the collection
                            if (conversion.ExpressionTypeFrom == conversion.ExpressionTypeTo) {
                                // Conversion within the same target (i.e., only substance conversion)

                                // Get random seed
                                var random = new McraRandomGenerator(
                                    RandomUtils.CreateSeed(collectionSeed, conversion.SubstanceFrom.GetHashCode(), conversion.ExpressionTypeFrom.GetHashCode())
                                );

                                // Get unit alignment factor
                                var targetUnitFrom = new TargetUnit(
                                    new ExposureTarget(conversion.BiologicalMatrix, conversion.ExpressionTypeFrom),
                                    conversion.UnitFrom
                                );
                                var alignmentFactorFrom = collection.TargetUnit
                                    .GetAlignmentFactor(targetUnitFrom, conversion.SubstanceFrom.MolecularMass, double.NaN);
                                var targetUnitTo = new TargetUnit(
                                    new ExposureTarget(conversion.BiologicalMatrix, conversion.ExpressionTypeTo),
                                    conversion.UnitTo
                                );
                                var alignmentFactorTo = targetUnitTo
                                    .GetAlignmentFactor(collection.TargetUnit, conversion.SubstanceTo.MolecularMass, double.NaN);
                                var overallAlignmentFactor = alignmentFactorFrom * alignmentFactorTo;

                                // Iterate over HBM individual day concentrations
                                foreach (var item in collection.HbmIndividualDayConcentrations) {
                                    if (item.ConcentrationsBySubstance.TryGetValue(conversion.SubstanceFrom, out var hbmTargetExposureFrom)) {

                                        var hasProperties = item.Individual?.IndividualPropertyValues?.Any() ?? false;
                                        var age = hasProperties ? (item.Individual.IndividualPropertyValues
                                            .FirstOrDefault(c => c.IndividualProperty.Name == "Age")?.DoubleValue) : null;
                                        var gender = hasProperties ? (item.Individual.IndividualPropertyValues
                                            .FirstOrDefault(c => c.IndividualProperty.Name == "Gender")?.TextValue) : null;
                                        var genderType = gender != null ? GenderTypeConverter.FromString(gender) : GenderType.Undefined;

                                        if (!item.ConcentrationsBySubstance.TryGetValue(conversion.SubstanceTo, out var targetExposure)) {
                                            // Substance to record does not yet exist, so create it
                                            item.ConcentrationsBySubstance[conversion.SubstanceTo] = convertTargetExposure(
                                                hbmTargetExposureFrom,
                                                conversion.SubstanceTo,
                                                model.Draw(random, age, genderType),
                                                overallAlignmentFactor
                                            );
                                        } else {
                                            // Substance to already exists (from another conversion)
                                            var record = convertTargetExposure(
                                                hbmTargetExposureFrom,
                                                conversion.SubstanceTo,
                                                model.Draw(random, age, genderType),
                                                overallAlignmentFactor
                                            );
                                            targetExposure.SourceSamplingMethods = targetExposure.SourceSamplingMethods
                                                .Union(record.SourceSamplingMethods)
                                                .ToList();
                                            targetExposure.Exposure += record.Exposure;
                                            targetExposure.IsAggregateOfMultipleSamplingMethods = targetExposure.SourceSamplingMethods.Count > 1;
                                        }
                                    }
                                }
                                collection.Target.ExpressionType = conversion.ExpressionTypeTo;
                            } else {
                                // Convert to other target (translated values need to be added to another collection)
                                throw new NotImplementedException();
                            }
                        }
                    }
                }
            }
            return results;
        }

        /// <summary>
        /// Clones and converts the substance target exposure record by a new substance using a conversion factor.
        /// Aligns the exposures using the collection and the From and To dose units information.
        /// </summary>
        /// <param name="hbmSubstanceTargetExposure"></param>
        /// <param name="substanceTo"></param>
        /// <param name="draw"></param>
        /// <param name="alignmentFactor"></param>
        /// <returns></returns>
        private HbmSubstanceTargetExposure convertTargetExposure(
            HbmSubstanceTargetExposure hbmSubstanceTargetExposure,
            Compound substanceTo,
            double draw,
            double alignmentFactor
        ) {
            return new HbmSubstanceTargetExposure() {
                Substance = substanceTo,
                SourceSamplingMethods = hbmSubstanceTargetExposure.SourceSamplingMethods,
                IsAggregateOfMultipleSamplingMethods = hbmSubstanceTargetExposure.IsAggregateOfMultipleSamplingMethods,
                Exposure = hbmSubstanceTargetExposure.Exposure * draw * alignmentFactor
            };
        }
    }
}
