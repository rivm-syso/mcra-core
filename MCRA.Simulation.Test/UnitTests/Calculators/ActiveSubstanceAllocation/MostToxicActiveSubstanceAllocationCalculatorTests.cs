﻿using System.Collections.Generic;
using System.Linq;
using DocumentFormat.OpenXml.Bibliography;
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;
using MCRA.General;
using MCRA.Simulation.Calculators.ActiveSubstanceAllocation;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using MCRA.Utils.Statistics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Calculators.ActiveSubstanceAllocation {
    /// <summary>
    /// Aggregate membership model calculator tests.
    /// </summary>
    [TestClass]
    public class MostToxicActiveSubstanceAllocationCalculatorTests : ActiveSubstanceAllocationCalculatorTestsBase {

        [TestMethod]
        public void MostToxicActiveSubstanceAllocationCalculator_TestAuthorised() {
            var foods = MockFoodsGenerator.Create(1);

            // Substances and substance conversions
            var activeSubstances = MockSubstancesGenerator.Create(new[] { "AS1", "AS2" });
            var measuredSubstances = MockSubstancesGenerator.Create(new[] { "MS1" });

            //Note: for exclusive conversions the proportion parameter is NOT used.
            var substanceConversions = new List<SubstanceConversion>() {
                createSubstanceConversion(activeSubstances[0], measuredSubstances[0], 0.5, true, 100),
                createSubstanceConversion(activeSubstances[1], measuredSubstances[0], 0.5, true, 200)
            };

            // Create authorisations
            var autorisations = MockSubstanceAuthorisationsGenerator.Create(
                (foods[0], activeSubstances[1]), (foods[0], activeSubstances[0])
            );

            // Create a sample substance collection (one sample and one measurement)
            var sampleSubstanceCollection = fakeSampleCompoundCollection(
                foods[0],
                fakeSampleSubstanceRecord(measuredSubstances, new[] { 0.1, double.NaN })
            );
            //AS1 substance most toxic
            var rpfs = activeSubstances.ToDictionary(c => c, c => 1d);
            rpfs[activeSubstances[0]] = 2;

            // Create allocator and run
            var allocator = new MostToxicActiveSubstanceAllocationCalculator(
                substanceConversions,
                autorisations,
                true, false, rpfs, false
            );
            var result = allocator.Allocate(
                sampleSubstanceCollection,
                activeSubstances.ToHashSet(),
                new McraRandomGenerator(1)
            );
            var sampleCompound = result.Single().SampleCompoundRecords.First();

            // Assert: concentration should be allocated to the authorised active substance, this is the first
            Assert.IsTrue(sampleCompound.AuthorisedUse);
            Assert.IsTrue(sampleCompound.SampleCompounds[activeSubstances[1]].IsZeroConcentration);
            Assert.AreEqual(0.5 * 0.1, sampleCompound.SampleCompounds[activeSubstances[0]].Residue);
        }

        [TestMethod]
        [DataRow(false, false)]
        [DataRow(false, true)]
        [DataRow(true, false)]
        [DataRow(true, true)]
        public void MostToxicActiveSubstanceAllocationCalculator_TestAuthorisedRawFood(
            params bool[] authorised
        ) {
            // Create a raw food and a processed food of the raw food
            var rawFoods = MockFoodsGenerator.Create(1);
            var processingTypes = MockProcessingTypesGenerator.Create(1);
            var processedFoods = MockFoodsGenerator.CreateProcessedFoods(rawFoods, processingTypes);

            // Substances and substance conversions
            var measuredSubstances = MockSubstancesGenerator.Create(new[] { "MS1" });
            var activeSubstances = MockSubstancesGenerator.Create(new[] { "AS1", "AS2" });
            var substanceConversions = new List<SubstanceConversion>() {
                createSubstanceConversion(activeSubstances[0], measuredSubstances[0], 0.5, true, 0.5),
                createSubstanceConversion(activeSubstances[1], measuredSubstances[0], 0.5, true, 0.5)
            };

            // Authorisations at level of raw foods
            var tuples = authorised
                .Select((r, ix) => (r, ix))
                .Where(r => r.r)
                .Select(r => (rawFoods[0], activeSubstances[r.ix]))
                .ToArray();
            var autorisations = MockSubstanceAuthorisationsGenerator.Create(
                tuples
            );

            // Sample concentrations at level of processed foods
            var sampleSubstanceCollection = fakeSampleCompoundCollection(
                processedFoods[0],
                fakeSampleSubstanceRecord(measuredSubstances, new[] { 0.1, double.NaN })
            );
            //AS1 substance most toxic
            var rpfs = activeSubstances.ToDictionary(c => c, c => 1d);
            rpfs[activeSubstances[0]] = 2;

            // Create allocator and run
            var allocator = new MostToxicActiveSubstanceAllocationCalculator(
                substanceConversions,
                autorisations,
                true, false, rpfs, false
            );

            var result = allocator.Allocate(
                sampleSubstanceCollection,
                activeSubstances.ToHashSet(),
                new McraRandomGenerator(1)
            );
            var sampleCompound = result.Single().SampleCompoundRecords.First();

            // One of the two substances should be allocated, the other not
            var allocatedSubstance = activeSubstances.Single(r => sampleCompound.SampleCompounds[r].IsPositiveResidue);
            var notAllocatedSubsance = activeSubstances.Single(r => sampleCompound.SampleCompounds[r].IsZeroConcentration);

            // If one of the two active substances is authorised, the resulting sample substance
            // should also be authorised (and allocated to that substance)
            Assert.AreEqual(authorised[0] || authorised[1], sampleCompound.AuthorisedUse);

            // Residue should be allocated to the second active substance, which is authorised
            Assert.AreEqual(0.5 * 0.1, sampleCompound.SampleCompounds[allocatedSubstance].Residue);

            if (authorised[0] && !authorised[1]) {
                // The allocated active substance should be the authorised one
                Assert.AreEqual(activeSubstances[0], allocatedSubstance);
            } else if (!authorised[0] && authorised[1]) {
                // The allocated active substance should be the authorised one
                Assert.AreEqual(activeSubstances[1], allocatedSubstance);
            }
        }
    }
}