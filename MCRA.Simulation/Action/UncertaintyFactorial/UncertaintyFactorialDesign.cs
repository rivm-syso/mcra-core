using MCRA.General;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Simulation.Action.UncertaintyFactorial {

    public class UncertaintyFactorialDesign : IEnumerable<UncertaintyFactorialSet> {

        public List<List<UncertaintySource>> TruthTable;

        public double[,] DesignMatrix { get; set; }

        public List<string> UncertaintySources { get; set; }

        public int Count {
            get {
                return TruthTable.Count;
            }
        }

        public IEnumerator<UncertaintyFactorialSet> GetEnumerator() {
            foreach (var uncertaintySourcesCollection in TruthTable) {
                yield return new UncertaintyFactorialSet() {
                    UncertaintySources = uncertaintySourcesCollection.ToHashSet(),
                    IsFullSet = Count == Convert.ToInt32(Math.Pow(2, uncertaintySourcesCollection.Count))
                };
            }
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    }
}
