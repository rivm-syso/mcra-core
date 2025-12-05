using MCRA.Utils.Statistics;
using MCRA.Utils.Statistics.Histograms;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Utils.Test.UnitTests.Statistics.Histograms {

    [TestClass]
    public class HistogramBinUtilitiesTests {

        private enum Gender { Male, Female }

        private class Person {
            public string Name { get; set; }
            public int Age { get; set; }
            public double Number { get; set; }
            public List<Gender> Children { get; set; }
            public int NumberOfChildren {
                get {
                    return Children.Count;
                }
            }
            public override string ToString() {
                return $"Name: {Name}, Age: {Age}";
            }
        }

        [TestMethod]
        public void MakeHistogramBinsTest0() {
            List<double> data = [];
            var bins = data.MakeHistogramBins();
            Assert.IsEmpty(bins);
        }

        [TestMethod]
        public void MakeHistogramBinsTest1() {
            List<double> data = [];
            var bins = data.MakeHistogramBins(4, 1, 4);
            Assert.HasCount(4, bins);
        }

        [TestMethod]
        public void MakeHistogramBinsTest2() {
            var random = new McraRandomGenerator();
            var persons = new List<Person>();
            for (int i = 0; i < 1000; i++) {
                persons.Add(new Person() {
                    Number = random.NextDouble()
                });
            }
            persons.Add(new Person() {
                Number = 1000,
            });
            var bins = persons.Select(v=>v.Number).MakeHistogramBins(10);
            Assert.AreEqual(1000, bins[0].Frequency);
            Assert.AreEqual(1, bins[9].Frequency);
        }

        [TestMethod]
        public void MakeHistogramBinsTest3() {
            var random = new McraRandomGenerator();
            var persons = new List<Person>();

            for (int i = 0; i < 1000; i++) {
                persons.Add(new Person() {
                    Number = random.NextDouble()
                });
            }

            persons.Add(new Person() {
                Number = 1000,
            });

            persons.Add(new Person() {
                Number = -1000,
            });

            var bins = persons.Select(v => v.Number).MakeHistogramBins(10, 0, 1);
            var n = bins.Sum(b => b.Frequency);
            Assert.AreEqual(1000, bins.Sum(b => b.Frequency));
        }

        [TestMethod]
        public void MakeHistogramBinsTest4() {
            var random = new McraRandomGenerator();
            var persons = new List<Person>();
            for (int i = 0; i < 1000; i++) {
                persons.Add(new Person() {
                    Number = random.NextDouble()
                });
            }

            persons.Add(new Person() {
                Number = 1000,
            });

            persons.Add(new Person() {
                Number = -1000,
            });

            var bins = persons.Select(v => v.Number).MakeHistogramBins(10, 0, 1,OutlierHandlingMethod.IncludeHigher);
            Assert.AreEqual(1001, bins.Sum(b => b.Frequency));
        }

        [TestMethod]
        public void MakeHistogramBinsTest5() {
            var random = new McraRandomGenerator();
            var persons = new List<Person>();
            for (int i = 0; i < 1000; i++) {
                persons.Add(new Person() {
                    Number = random.NextDouble()
                });
            }

            persons.Add(new Person() {
                Number = 1000,
            });

            persons.Add(new Person() {
                Number = -1000,
            });
            var bins = persons.Select(v => v.Number).MakeHistogramBins(10, 0, 1, OutlierHandlingMethod.IncludeLower);
            Assert.AreEqual(1001, bins.Sum(b => b.Frequency));
        }

        [TestMethod]
        public void MakeHistogramBinsTest6() {
            var random = new McraRandomGenerator();
            var persons = new List<Person>();
            for (int i = 0; i < 1000; i++) {
                persons.Add(new Person() {
                    Number = random.NextDouble()
                });
            }

            persons.Add(new Person() {
                Number = 1000,
            });

            persons.Add(new Person() {
                Number = -1000,
            });

            var bins = persons.Select(v => v.Number).MakeHistogramBins(10, 0, 1, OutlierHandlingMethod.IncludeBoth);
            Assert.AreEqual(1002, bins.Sum(b => b.Frequency));
        }

        [TestMethod]
        public void MakeHistogramBinsTest7() {
            var random = new McraRandomGenerator();
            var persons = new List<Person>();
            for (int i = 0; i < 1000; i++) {
                persons.Add(new Person() {
                    Number = random.NextDouble()
                });
            }
            var bins = persons.Select(v => v.Number).MakeHistogramBins(10, 0, 1, OutlierHandlingMethod.IncludeBoth);
            System.Diagnostics.Trace.WriteLine(bins.Average());
            System.Diagnostics.Trace.WriteLine(bins.Variance());
        }

        [TestMethod]
        public void MakeHistogramBinsTest8() {
            var random = new McraRandomGenerator();
            List<double> data = [0.1, 0.2];
            var bins = data.MakeHistogramBins(10);
            Assert.AreEqual(2, bins.GetTotalFrequency());
        }

        [TestMethod]
                public void MakeHistogramBinsTest9() {
            List<double> data = [1];
            var bins = data.MakeHistogramBins();
            Assert.AreEqual(1, bins.GetTotalFrequency());
            Assert.IsLessThan(1, bins.First().XMinValue);
            Assert.IsGreaterThan(1, bins.Last().XMaxValue);
        }

        [TestMethod]
                public void MakeHistogramBinsTest10() {
            List<double> data = [-1, -1, -1];
            var bins = data.MakeHistogramBins();
            Assert.IsLessThan(bins.First().XMaxValue, bins.First().XMinValue);
        }

        [TestMethod]
        public void MakeCategorizedHistogramBinsTest0() {
            var persons = new List<Person>();
            Func<Person, List<CategoryContribution<Gender>>> categoryExtractor = (x) => x.Children.GroupBy(c => c).Select(g => new CategoryContribution<Gender>(g.Key, g.Count())).ToList();
            Func<Person, double> valueExtractor = (x) => x.NumberOfChildren;
            var categorizedBins = persons.MakeCategorizedHistogramBins<Person, Gender>(categoryExtractor, valueExtractor);
            Assert.IsEmpty(categorizedBins);
        }

        [TestMethod]
        public void MakeCategorizedHistogramBinsTest1() {
            var persons = new List<Person>();
            Func<Person, List<CategoryContribution<Gender>>> categoryExtractor = (x) => x.Children.GroupBy(c => c).Select(g => new CategoryContribution<Gender>(g.Key, g.Count())).ToList();
            Func<Person, double> valueExtractor = (x) => x.NumberOfChildren;
            var categorizedBins = persons.MakeCategorizedHistogramBins<Person, Gender>(categoryExtractor, valueExtractor, null, 4, 1, 4);
            Assert.HasCount(4, categorizedBins);
        }

        [TestMethod]
        public void MakeCategorizedHistogramBinsTest2() {
            var persons = new List<Person> {
                new() {
                    Number = 0,
                    Children = [Gender.Male]
                },

                new() {
                    Number = 0,
                    Children = [Gender.Male, Gender.Female]
                },

                new() {
                    Number = 0,
                    Children = [Gender.Male, Gender.Female, Gender.Female]
                },

                new() {
                    Number = 0,
                    Children = [Gender.Male, Gender.Female, Gender.Female]
                },

                new() {
                    Number = 0,
                    Children = [Gender.Male, Gender.Male, Gender.Male, Gender.Female]
                },

                new() {
                    Number = 0,
                    Children = [Gender.Male, Gender.Female, Gender.Female, Gender.Female]
                }
            };

            var bins = persons.Select(v => (double)v.NumberOfChildren).MakeHistogramBins(4);
            Func<Person, List<CategoryContribution<Gender>>> categoryExtractor = (x) => x.Children.GroupBy(c => c).Select(g => new CategoryContribution<Gender>(g.Key, g.Count())).ToList();
            Func<Person, double> valueExtractor = (x) => x.NumberOfChildren;
            var categorizedBins = persons.MakeCategorizedHistogramBins<Person, Gender>(categoryExtractor, valueExtractor, null, 4, 0.5, 4.5);
            CollectionAssert.AreEqual(new List<double> { 1, 1, 2, 2 }, categorizedBins.Select(b => b.Frequency).ToList());
            CollectionAssert.AreEqual(new List<double> { 1D }, categorizedBins.ElementAt(0).ContributionFractions.Select(cf => cf.Contribution).OrderBy(cf => cf).ToList());
            CollectionAssert.AreEqual(new List<double> { 0.5, 0.5 }, categorizedBins.ElementAt(1).ContributionFractions.Select(cf => cf.Contribution).OrderBy(cf => cf).ToList());
            CollectionAssert.AreEqual(new List<double> { 1D/3, 2D/3 }, categorizedBins.ElementAt(2).ContributionFractions.Select(cf => cf.Contribution).OrderBy(cf => cf).ToList());
            CollectionAssert.AreEqual(new List<double> { 0.5, 0.5 }, categorizedBins.ElementAt(3).ContributionFractions.Select(cf => cf.Contribution).OrderBy(cf => cf).ToList());
        }

        [TestMethod]
        public void MakeCategorizedHistogramBinsTest3() {
            var persons = new List<Person> {
                new() {
                    Number = 0,
                    Children = [Gender.Male]
                }
            };
            Func<Person, List<CategoryContribution<Gender>>> categoryExtractor = (x) => x.Children.GroupBy(c => c).Select(g => new CategoryContribution<Gender>(g.Key, g.Count())).ToList();
            Func<Person, double> valueExtractor = (x) => x.NumberOfChildren;
            var categorizedBins = persons.MakeCategorizedHistogramBins<Person, Gender>(categoryExtractor, valueExtractor);
            Assert.AreEqual(1, categorizedBins.GetTotalFrequency());
        }

        [TestMethod]
        public void MakeCategorizedHistogramBinsTest4() {
            var values = new List<int>() { -1, -1, -1 };
            Func<int, List<CategoryContribution<int>>> categoryExtractor = (x) => [new CategoryContribution<int>(x, 1D)];
            Func<int, double> valueExtractor = (x) => x;
            var bins = values.MakeCategorizedHistogramBins<int, int>(categoryExtractor, valueExtractor);
            Assert.AreEqual(3, bins.GetTotalFrequency());
            Assert.IsLessThan(bins.First().XMaxValue, bins.First().XMinValue);
        }
    }
}
