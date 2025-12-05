using MCRA.Utils.Collections;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Utils.Test.UnitTests {
    [TestClass]
    public class BitPattern32Tests {
        [TestMethod]
        public void BitPattern32TestSubset() {
            var bpBas = new BitPattern32("00000110101010101001101011101111000000001111");
            var bpSub = new BitPattern32("00000110101010101001101011101111000000001111");
            Assert.IsTrue(bpSub.IsSubSetOf(bpBas));

            bpBas = new BitPattern32("00000110101010101001101011101111000000001111");
            bpSub = new BitPattern32("00000110101010101001101011100000000000001111");
            Assert.IsTrue(bpSub.IsSubSetOf(bpBas));

            bpBas = new BitPattern32("00000110101010101001101011101111000000001111");
            bpSub = new BitPattern32("0");
            Assert.IsFalse(bpSub.IsSubSetOf(bpBas));

            bpBas = new BitPattern32("00000110101010101001101011101111000000001111");
            bpSub = new BitPattern32("1");
            Assert.IsFalse(bpSub.IsSubSetOf(bpBas));

            bpBas = new BitPattern32("");
            bpSub = new BitPattern32("");
            Assert.IsFalse(bpSub.IsSubSetOf(bpBas));

            bpBas = new BitPattern32("1");
            bpSub = new BitPattern32("1");
            Assert.IsTrue(bpSub.IsSubSetOf(bpBas));

            bpBas = new BitPattern32("00000110101010101001101011101111000000001111");
            bpSub = new BitPattern32("00000000000000000000000000000000000000000001");
            Assert.IsTrue(bpSub.IsSubSetOf(bpBas));

            bpBas = new BitPattern32("00000110101010101001101011101111000000001111");
            bpSub = new BitPattern32("10000000000000000000000000000000000000000000");
            Assert.IsFalse(bpSub.IsSubSetOf(bpBas));

            bpBas = new BitPattern32("0000011010101010100110101110111100000000111");
            bpSub = new BitPattern32("0000011010100000000000000000000000000000111000000000000000000000");
            Assert.IsFalse(bpSub.IsSubSetOf(bpBas));

            bpBas = new BitPattern32("0000011010101010100110101110111100000000111");
            bpSub = new BitPattern32("000001101010000000000000000000000000000011100000000000000000000000000000010000");
            Assert.IsFalse(bpSub.IsSubSetOf(bpBas));
        }

        [TestMethod]
        public void BitPattern32GroupingTest() {
            var list = new string[] {
                "00000000001000001000000000000000001000000000000000001100000000000000001000000000000100000000000000",//group1
                "00000000001000001000000000000000001000000000000000001100000000000000001000000000000100000000000000",
                "00000000001000001000000000000000001000000000000000001100000000000000001000000000000100000000000000",
                "00000000001000001000000000000000001000000000000000001100000000000000000100000000000000000000010000",//group2
                "00000000001000001000000000000000001000000000000000001100000000000000000100000000000000000000010000",
                "00000000001000001000000000000000001000000000000000001100000000000011100000000000000000000000010000",//group3
                "00000000001000001000000000000000001000000000000000001100000000000011100000000000000000000000010000",
                "00000000001000001000000000000000001000000000000000001100000000000010000001000000000000000000010000",//group4
                "00000000001000001000000000000000001000000000000000001100000000000010000001000000000000000000010000",
                "00000000001000001000000000000000001000000000000000001100000000000010000001000000000000000000010000",
                "10000000001000001000000000000000001000000000000000001100000000000010000000000000000000000000010000",//group5
                "00000000001000001000000000000000001000000000000000001100000000000000000001000000001100000000000000",//group6
                "00000000001000001000000000000000001000000000000000001100000000000000000001000000001100000000000000",
                "00000000001000001000000000000000001000000000000000001100000000000000000001000000001100000000000000",
                "10000000001000001000000000000000001000000000000000001100000000000000000001000000001100110000000000",//group7
            };
            var groups = list.Select(s => new BitPattern32(s))
                .GroupBy(p => p).ToList();

            Assert.HasCount(7, groups);
        }

        [TestMethod]
        public void BitPattern32Test() {
            var strIn = "";
            var bp = new BitPattern32(strIn);
            var strOut = bp.ToString();
            Assert.AreEqual(strIn, strOut);
            Assert.AreEqual(0, bp.NumberOfSetBits);
            Assert.AreEqual(0, bp.IndicesOfSetBits.Count());

            strIn = "0";
            bp = new BitPattern32(strIn);
            strOut = bp.ToString();
            Assert.AreEqual(strIn, strOut);
            Assert.AreEqual(0, bp.NumberOfSetBits);
            Assert.AreEqual(0, bp.IndicesOfSetBits.Count());

            strIn = "1";
            bp = new BitPattern32(strIn);
            strOut = bp.ToString();
            Assert.AreEqual(strIn, strOut);
            Assert.AreEqual(1, bp.NumberOfSetBits);
            Assert.AreEqual(0, bp.IndicesOfSetBits.Single());

            strIn = "000001101010101010101010101010101000000000000000000000100000000000000000001010101011101010110101101011101111000000001111";
            bp = new BitPattern32(strIn);
            strOut = bp.ToString();
            Assert.AreEqual(strIn, strOut);
            var indices = string.Join(",", bp.IndicesOfSetBits);
            Assert.AreEqual("5,6,8,10,12,14,16,18,20,22,24,26,28,30,32,54,74,76,78,80,82,83,84,86,88,90,91,93,95,96,98,100,101,102,104,105,106,107,116,117,118,119", indices);
            Assert.AreEqual(42, bp.NumberOfSetBits);
            var chars = strIn.ToCharArray();
            for (int i = 0; i < chars.Length; i++) {
                Assert.AreEqual(chars[i] == '1', bp.Get(i));
            }

            strIn = "0000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000001";
            bp = new BitPattern32(strIn);
            strOut = bp.ToString();
            Assert.AreEqual(strIn, strOut);
            Assert.AreEqual(1, bp.NumberOfSetBits);
            Assert.AreEqual(180, bp.IndicesOfSetBits.Single());
        }
    }
}
