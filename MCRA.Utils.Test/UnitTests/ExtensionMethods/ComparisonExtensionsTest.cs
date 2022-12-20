using System;
using System.Xml.Linq;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Test.Mocks;
using MCRA.Utils.TestReporting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Utils.Test.UnitTests {

    [TestClass()]
    public class ComparisonExtensionMethodsTests {

        [TestMethod]
        public void ComparePublicProperties_Tests1() {
            var name = TestUtils.GetRandomString(8);
            var emp1 = new Employee() {
                Name = name,
                Age = 30,
            };
            var emp2 = new Employee() {
                Name = name,
                Age = 30,
            };
            Assert.IsTrue(emp1.ComparePublicProperties(emp2));
        }

        [TestMethod]
        public void ComparePublicProperties_Tests2() {
            var name = TestUtils.GetRandomString(8);
            var emp1 = new Employee() {
                Name = name,
                Age = 30,
            };
            var emp2 = new Employee() {
                Name = name,
                Age = 29,
            };
            Assert.IsFalse(emp1.ComparePublicProperties(emp2));
        }

        [TestMethod]
        public void ComparePublicProperties_Tests3() {
            var name = TestUtils.GetRandomString(8);
            var emp1 = new Employee() {
                Name = name,
                Age = 30,
            };
            var emp2 = new Employee() {
                Name = name,
            };
            Assert.IsFalse(emp1.ComparePublicProperties(emp2));
        }

        [TestMethod]
        public void ComparePublicProperties_Tests4() {
            var name = TestUtils.GetRandomString(8);
            var emp1 = new Employee() {
                Name = name,
                Age = 30,
            };
            Assert.IsTrue(emp1.ComparePublicProperties(emp1));
        }

        [TestMethod]
        public void ComparePublicProperties_Tests5() {
            var name = TestUtils.GetRandomString(8);
            var emp1 = new Employee() {
                Name = name,
                Age = 30,
            };
            Assert.IsFalse(emp1.ComparePublicProperties(null));
        }
    }
}
