using System.Collections.Concurrent;
using MCRA.Utils.Collections;
using MCRA.Utils.Test.Mocks;
using MCRA.Utils.TestReporting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Utils.Test.UnitTests {

    [TestClass]
    public class UtilsCollectionsTests {

        [TestMethod]
        public void TwoKeyConcurrentDictionaryTest1() {
            var name = TestUtils.GetRandomString(8);
            var tkcd = new ConcurrentDictionary<(string, int), Employee>();
            Assert.IsTrue(tkcd.TryAdd((name, 40), new Employee() { Name = name, Age = 40 }));
            Assert.IsFalse(tkcd.TryAdd((name, 40), new Employee() { Name = name, Age = 40 }));
            Assert.IsTrue(tkcd.TryAdd((TestUtils.GetRandomString(8), 40), new Employee() { Name = TestUtils.GetRandomString(8), Age = 40 }));
            Assert.IsTrue(tkcd.TryAdd((TestUtils.GetRandomString(8), 30), new Employee() { Name = TestUtils.GetRandomString(8), Age = 40 }));

            Assert.IsTrue(tkcd.ContainsKey((name, 40)));
            Assert.IsFalse(tkcd.ContainsKey((TestUtils.GetRandomString(8), 30)));

            Assert.IsTrue(tkcd.TryRemove((name, 40), out var fons));
            Assert.IsFalse(tkcd.ContainsKey((name, 30)));
        }

        [TestMethod]
        public void ThreeKeyConcurrentDictionaryTest1() {
            var name = TestUtils.GetRandomString(8);
            var tkcd = new ConcurrentDictionary<(string, int, DateTime), Employee>();
            Assert.IsTrue(tkcd.TryAdd((name, 40, DateTime.Today), new Employee() { Name = name, Age = 40 }));
            Assert.IsFalse(tkcd.TryAdd((name, 40, DateTime.Today), new Employee() { Name = name, Age = 40 }));
            Assert.IsTrue(tkcd.TryAdd((TestUtils.GetRandomString(8), 40, DateTime.Today), new Employee() { Name = TestUtils.GetRandomString(8), Age = 40 }));
            Assert.IsTrue(tkcd.TryAdd((TestUtils.GetRandomString(8), 30, DateTime.Today), new Employee() { Name = TestUtils.GetRandomString(8), Age = 40 }));

            Assert.IsTrue(tkcd.ContainsKey((name, 40, DateTime.Today)));
            Assert.IsFalse(tkcd.ContainsKey((TestUtils.GetRandomString(8), 30, DateTime.Today)));

            Assert.IsTrue(tkcd.TryRemove((name, 40, DateTime.Today), out _));
            Assert.IsFalse(tkcd.ContainsKey((name, 30, DateTime.Today)));
        }

        [TestMethod]
        public void SerializableDictionaryTest1() {
            var nameA = TestUtils.GetRandomString(8);
            var nameB = TestUtils.GetRandomString(8);
            var nameC = TestUtils.GetRandomString(8);
            var nameD = TestUtils.GetRandomString(8);
            var serializableDictionary = new SerializableDictionary<string, Employee> {
                { nameA, new Employee() { Name = nameA, Age = 40 } },
                { nameB, new Employee() { Name = nameB, Age = 40 } },
                { nameC, new Employee() { Name = nameC, Age = 40 } },
                { nameD, new Employee() { Name = nameD, Age = 40 } }
            };

            var dictionary = serializableDictionary as Dictionary<string, Employee>;
            Assert.IsTrue(dictionary.ContainsKey(nameA));
            Assert.IsTrue(dictionary.ContainsKey(nameB));
            Assert.IsTrue(dictionary.ContainsKey(nameC));
            Assert.IsTrue(dictionary.ContainsKey(nameD));
        }

        [TestMethod]
        public void SerializableDictionaryTest2() {
            var nameA = TestUtils.GetRandomString(8);
            var nameB = TestUtils.GetRandomString(8);
            var nameC = TestUtils.GetRandomString(8);
            var nameD = TestUtils.GetRandomString(8);
            var dictionary = new Dictionary<string, Employee> {
                { nameA, new Employee() { Name = nameA, Age = 40 } },
                { nameB, new Employee() { Name = nameB, Age = 40 } },
                { nameC, new Employee() { Name = nameC, Age = 40 } },
                { nameD, new Employee() { Name = nameD, Age = 40 } }
            };

            var serializableDictionary = new SerializableDictionary<string, Employee>(dictionary);
            Assert.IsTrue(serializableDictionary.ContainsKey(nameA));
            Assert.IsTrue(serializableDictionary.ContainsKey(nameB));
            Assert.IsTrue(serializableDictionary.ContainsKey(nameC));
            Assert.IsTrue(serializableDictionary.ContainsKey(nameD));
        }
    }
}
