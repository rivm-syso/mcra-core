using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;
using MCRA.General;
using MCRA.Simulation.Calculators.CompoundResidueCollectionCalculation;
using MCRA.Simulation.Calculators.ConcentrationModelCalculation.ConcentrationModels;

namespace MCRA.Simulation.Test.Mock.MockDataGenerators {

    /// <summary>
    /// Class for generating mock sample substances collections.
    /// </summary>
    public static class MockSampleCompoundCollectionsGenerator {

        #region Helper classes

        internal class SimpleConcentrationModel {
            public double Mu { get; set; }
            public double Sigma { get; set; }
            public double FractionPositives { get; set; }
            public double Lor {
                get {
                    return !double.IsNaN(Loq) ? Loq : Lod;
                }
            }
            public double Lod { get; set; }
            public double Loq { get; set; }
        }

        #endregion

        /// <summary>
        /// Creates a list of sample substance collections
        /// </summary>
        /// <param name="foods"></param>
        /// <param name="substances"></param>
        /// <param name="numberOfSamples"></param>
        /// <param name="random"></param>
        /// <returns></returns>
        public static IDictionary<Food, SampleCompoundCollection> Create(
            ICollection<Food> foods,
            ICollection<Compound> substances,
            IRandom random,
            int[] numberOfSamples = null
        ) {
            var result = new List<SampleCompoundCollection>();
            for (int i = 0; i < foods.Count; i++) {
                var food = foods.ElementAt(i);
                var concentrationModels = substances
                    .ToDictionary(
                        r => r,
                        r => new SimpleConcentrationModel() {
                            Mu = NormalDistribution.Draw(random, 0, 1),
                            Sigma = LogNormalDistribution.Draw(random, 0, 1),
                            Loq = 0.05,
                            Lod = 0.05,
                            FractionPositives = random.NextDouble()
                        }
                    );
                var numSamples = numberOfSamples != null ? numberOfSamples[i] : 20;
                var sampleCompoundRecords = Enumerable
                    .Range(0, numSamples)
                    .Select(r => createSampleCompoundRecord(substances, concentrationModels, random))
                    .ToList();
                var record = new SampleCompoundCollection(
                    food,
                    sampleCompoundRecords
                );
                result.Add(record);
            }
            return result.ToDictionary(r => r.Food);
        }

        /// <summary>
        /// Creates a list of sample substance collections
        /// </summary>
        /// <param name="foods"></param>
        /// <param name="substances"></param>
        /// <param name="concentrationModels"></param>
        /// <returns></returns>
        public static IDictionary<Food, SampleCompoundCollection> Create(
            ICollection<Food> foods,
            ICollection<Compound> substances,
            IDictionary<(Food, Compound), ConcentrationModel> concentrationModels
        ) {
            var result = createSampleCompoundRecords(foods, substances, concentrationModels);
            return result.ToDictionary(r => r.Food);
        }

        /// <summary>
        /// Creates a sample compound record for the specified substances
        /// using the provided simple concentration models.
        /// </summary>
        /// <param name="substances"></param>
        /// <param name="concentrationModels"></param>
        /// <param name="random"></param>
        /// <returns></returns>
        private static SampleCompoundRecord createSampleCompoundRecord(
            IEnumerable<Compound> substances,
            IDictionary<Compound, SimpleConcentrationModel> concentrationModels,
            IRandom random
        ) {
            var result = new SampleCompoundRecord() {
                AuthorisedUse = true,
                FoodSample = new FoodSample(),
                SampleCompounds = substances
                    .ToDictionary(
                        r => r,
                        r => {
                            var model = concentrationModels[r];
                            var concentration = random.NextDouble() > model.FractionPositives
                                ? LogNormalDistribution.Draw(random, model.Mu, model.Sigma)
                                : 0;
                            var scr = new SampleCompound() {
                                ActiveSubstance = r,
                                MeasuredSubstance = r,
                                Loq = model.Loq,
                                Lod = model.Lod,
                                ResType = concentration < model.Lor ? ResType.LOQ : ResType.VAL,
                                Residue = concentration > model.Lor ? concentration : double.NaN
                            };
                            return scr;
                        }
                    ),
            };
            return result;
        }

        /// <summary>
        /// Creates a list of sample substance collections for the specified
        /// foods and substances using the provided concentration models to
        /// generate concentrations.
        /// </summary>
        /// <param name="foods"></param>
        /// <param name="substances"></param>
        /// <param name="concentrationModels"></param>
        /// <returns></returns>
        private static List<SampleCompoundCollection> createSampleCompoundRecords(
            ICollection<Food> foods,
            ICollection<Compound> substances,
            IDictionary<(Food, Compound), ConcentrationModel> concentrationModels
        ) {
            var sampleCompoundCollections = new List<SampleCompoundCollection>();
            var numberOfResidues = concentrationModels.First().Value.Residues.NumberOfResidues;
            var lor = concentrationModels.First().Value.Residues.CensoredValues.FirstOrDefault();

            foreach (var food in foods) {
                var sampleCompoundRecords = new List<SampleCompoundRecord>();
                for (int i = 0; i < numberOfResidues; i++) {
                    var sample = new SampleAnalysis() {
                        Concentrations = new Dictionary<Compound, ConcentrationPerSample>(),
                    };
                    var sampleCompoundRecord = new SampleCompoundRecord() {
                        FoodSample = new FoodSample() { 
                            Food = food,
                            SampleAnalyses = new List<SampleAnalysis> { sample } 
                        },
                        SampleCompounds = new Dictionary<Compound, SampleCompound>(),
                        AuthorisedUse = true
                    };
                    foreach (var substance in substances) {
                        concentrationModels.TryGetValue((food, substance), out ConcentrationModel model);
                        if (model != null) {
                            var allConcentrations = new List<double>();
                            allConcentrations.AddRange(model.Residues.Positives);
                            allConcentrations.AddRange(model.Residues.CensoredValues);
                            allConcentrations.AddRange(Enumerable.Repeat(0d, model.Residues.ZerosCount).ToList());
                            var concentrationValue = allConcentrations[i];
                            var concentrationPerSample = new ConcentrationPerSample() {
                                Compound = substance,
                                Sample = sample,
                                Concentration = concentrationValue,
                            };
                            concentrationPerSample.Sample.Concentrations[substance] = concentrationPerSample;
                            var resType = concentrationValue == 0 ? ResType.MV : (concentrationValue == lor ? ResType.LOQ : ResType.VAL);
                            var sampleCompound = new SampleCompound() {
                                Residue = concentrationValue,
                                ActiveSubstance = substance,
                                ResType = resType,
                                MeasuredSubstance = substance,
                                Loq = 0.05,
                                Lod = 0.05,
                                IsExtrapolated = false,
                            };

                            sampleCompoundRecord.SampleCompounds[substance] = sampleCompound;
                            sampleCompoundRecord.FoodSample.SampleAnalyses.First().Concentrations[substance] = concentrationPerSample;
                        } else {
                            var sampleCompound = new SampleCompound() {
                                ActiveSubstance = substance,
                                MeasuredSubstance = substance,
                                ResType = ResType.MV,
                            };
                            sampleCompoundRecord.SampleCompounds[substance] = sampleCompound;
                        }
                    }
                    sampleCompoundRecords.Add(sampleCompoundRecord);
                }
                sampleCompoundCollections.Add(new SampleCompoundCollection(food, sampleCompoundRecords) {
                    Food = food,
                    SampleCompoundRecords = sampleCompoundRecords,
                });
            }

            return sampleCompoundCollections;
        }

        /// <summary>
        /// Creates a list of sample substance concentrations
        /// </summary>
        /// <param name="mu"></param>
        /// <param name="sigma"></param>
        /// <param name="fractionZero"></param>
        /// <param name="n"></param>
        /// <param name="random"></param>
        /// <returns></returns>
        private static List<double> createConcentrations(double mu, double sigma, double fractionZero, int n, IRandom random) {
            var positives = (int)(n - Math.Round(fractionZero * n));
            var zeros = n - positives;
            var x = Enumerable
                .Range(0, n)
                .Select(r => r < positives ? LogNormalDistribution.InvCDF(mu, sigma, random.NextDouble(0, 1)) : 0D)
                .ToList();

            return x.Shuffle(random).ToList();
        }

        /// <summary>
        /// Creates a list of substance residue collections
        /// </summary>
        /// <param name="food"></param>
        /// <param name="compound"></param>
        /// <param name="concentrations"></param>
        /// <param name="lor"></param>
        /// <param name="random"></param>
        /// <returns></returns>
        private static CompoundResidueCollection createConcentrations(Food food, Compound compound, List<double> concentrations, double lor, IRandom random) {
            var positivesCount = concentrations.Where(r => r > 0).Count();
            var zerosCount = concentrations.Where(r => r == 0).Count();
            return new CompoundResidueCollection() {
                Food = food,
                Compound = compound,
                Positives = concentrations.Where(r => r >= lor).ToList(),
                CensoredValuesCollection = concentrations.Where(r => r < lor && r > 0).Select(r => new CensoredValueCollection() { LOD = lor, LOQ = lor }).ToList(),
                ZerosCount = zerosCount,
            };
        }
    }
}
