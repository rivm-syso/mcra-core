using MCRA.General;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Simulation.Action.UncertaintyFactorial {
    public sealed class UncertaintyFactorialSet {

        public UncertaintyFactorialSet(params UncertaintySource[] sources) {
            UncertaintySources = sources.ToHashSet();
        }

        /// <summary>
        /// The uncertainty/variability sources included in this set.
        /// </summary>
        public ICollection<UncertaintySource> UncertaintySources { get; set; }

        /// <summary>
        /// Gets/sets whether this is the full factorial set. I.e., including
        /// all uncertainty/variability sources.
        /// </summary>
        public bool IsFullSet { get; set; }

        /// <summary>
        /// Returns true if this factorial set includes the specified uncertainty/
        /// variability source.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public bool Contains(UncertaintySource source) {
            return UncertaintySources.Contains(source);
        }
    }
}
