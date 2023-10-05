using MCRA.Utils.Statistics;
using System.Data;
using System.Diagnostics.Contracts;

namespace MCRA.Utils.ExtensionMethods {

    public static class IEnumerableExtensions {

        /// <summary>
        /// Creates an IEnumerable from a single element.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <returns></returns>
        public static IEnumerable<T> ToSingleElementSequence<T>(this T item) {
            yield return item;
        }

        /// <summary>
        /// Generates a cumulated sums list of the values extracted from the source list by the provided value extractor.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="valueExtractor"></param>
        /// <returns></returns>
        public static IEnumerable<double> CumulativeWeights<T>(this IEnumerable<T> source, Func<T, double> valueExtractor) {
            var currentTotal = 0D;
            var cumulativeSums = new List<double>();
            foreach (var weights in source) {
                currentTotal += valueExtractor(weights);
                cumulativeSums.Add(currentTotal);
            }
            return cumulativeSums;
        }

        /// <summary>
        /// Returns a resampled subset based on the target IEnumerable using a random generator with the specified seed.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="original">The set to resample.</param>
        /// <param name="random">The random number generator.</param>
        /// <param name="count">The number of resample draws.</param>
        /// <returns>Resampled set</returns>
        public static IEnumerable<T> Resample<T>(this IEnumerable<T> original, IRandom random, int count) {
            var newSet = new List<T>(count);
            var originalSet = original.ToList();
            for (int i = 0; i < count; i++) {
                newSet.Add(originalSet[random.Next(originalSet.Count)]);
            }
            return newSet;
        }

        /// <summary>
        /// Returns a resampled subset on the target IEnumerable using a random seeded random generator.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="original"></param>
        /// <param name="random"></param>
        /// <returns></returns>
        public static IEnumerable<T> Resample<T>(this IEnumerable<T> original, IRandom random) {
            return original.Resample(random, original.Count());
        }

        /// <summary>
        /// Returns a resampled subset based on the target IEnumerable using a random seeded random generator.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="original">The set to resample.</param>
        /// <returns>Resampled set</returns>
        public static IEnumerable<T> Resample<T>(this IEnumerable<T> original) {
            return original.Resample(new McraRandomGenerator());
        }

        /// <summary>
        /// Partitions the target IEnumerable into chunks.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="size">Chuck size</param>
        /// <returns></returns>
        public static IEnumerable<List<T>> Partition<T>(this IList<T> source, int size) {
            for (int i = 0; i < BMath.Ceiling(source.Count / (double)size); i++) {
                yield return new List<T>(source.Skip(size * i).Take(size));
            }
        }

        /// <summary>
        /// Calculates the intersection of all child IEnumerables.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IEnumerable<T> FullSelfIntersect<T>(this IEnumerable<IEnumerable<T>> source) {
            var s = source.ToList();
            IEnumerable<T> intersection = s[0];
            for (int i = 1; i < source.Count(); i++) {
                intersection = intersection.Intersect(s[i]);
            }
            return intersection;
        }

        /// <summary>
        /// Returns the distinct values in the set based on a key property.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="keyExtractor">key selector</param>
        /// <returns></returns>
        public static IEnumerable<T> Distinct<T>(this IEnumerable<T> source, Func<T, object> keyExtractor) {
            return source.Distinct<T>(new KeyEqualityComparer<T>(keyExtractor));
        }

        /// <summary>
        /// Returns the average of over the elements in the list, or zero if the list is empty.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static double AverageOrZero(this IEnumerable<double> source) {
            if (!source.Any()) {
                return 0;
            } else {
                return source.Average();
            }
        }

        /// <summary>
        /// Returns the average of over the elements in the list, or zero if the list is empty.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="keySelector"></param>
        /// <returns></returns>
        public static double AverageOrZero<T>(this IEnumerable<T> source, Func<T, double> keySelector) {
            if (!source.Any()) {
                return 0;
            } else {
                return source.Average(keySelector);
            }
        }

        /// <summary>
        /// Draws an object from the sourceObjects collection based on the probability-weight
        /// factor extracted by WeightExtractor.
        /// </summary>
        /// <param name="probabilityExtractor">function delegate to calculate the probability a certain object is drawn</param>
        public static T DrawRandom<T>(this IEnumerable<T> source, Func<T, double> probabilityExtractor) {
            return source.DrawRandomProbExtractor(new McraRandomGenerator(), probabilityExtractor);
        }

        /// <summary>
        /// Draws a random object from the sourceObjects collection.
        /// </summary>
        public static T DrawRandom<T>(this IEnumerable<T> source) {
            return source.DrawRandomP(new McraRandomGenerator());
        }

        /// <summary>
        /// Draws an object from the sourceObjects collection based on the probability-weight factor 
        /// extracted by WeightExtractor using the supplied random number generator.
        /// </summary>
        /// <param name="random">A uniform random number generator</param>
        /// <param name="probabilityExtractor">function delegate to calculate the probability a certain object is drawn</param>
        public static T DrawRandomProbExtractor<T>(this IEnumerable<T> source, IRandom random, Func<T, double> probabilityExtractor) {
            var u = random.NextDouble();
            var cumP = 0.0;
            foreach (var o in source) {
                cumP += probabilityExtractor(o);
                if (u <= cumP) {
                    return o;
                }
            }
            return default(T);
        }

        /// <summary>
        /// Draws a random object from the sourceObjects collection using the supplied random generator.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="random"></param>
        public static T DrawRandomP<T>(this IEnumerable<T> source, IRandom random) {
            var p = 1D / source.Count();
            var u = random.NextDouble();
            var cumP = 0.0;
            foreach (var item in source) {
                cumP += p;
                if (u <= cumP) {
                    return item;
                }
            }
            return default(T);
        }

        /// <summary>
        /// Draws a random object from the sourceObjects collection using the supplied random generator.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="random"></param>
        public static T DrawRandom<T>(this IEnumerable<T> source, IRandom random) {
            IList<T> sourceItems;
            if (source is IList<T>) {
                sourceItems = source as IList<T>;
            } else {
                sourceItems = source.ToList();
            }
            var n = sourceItems.Count;
            return sourceItems[random.Next(n)];
        }

        /// <summary>
        /// Draws an object from the sourceObjects collection based on the probability-weight
        /// factor extracted by WeightExtractor using the supplied random number generator.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="random">A uniform random number generator</param>
        /// <param name="probabilityExtractor">function delegate to calculate the probability a certain object is drawn</param>
        public static T DrawRandom<T>(this IEnumerable<T> source, IRandom random, Func<T, double> probabilityExtractor) {
            var u = random.NextDouble();
            var cumP = 0.0;
            foreach (var item in source) {
                cumP += probabilityExtractor(item);
                if (u <= cumP) {
                    return item;
                }
            }
            return default(T);
        }

        /// <summary>
        /// Draws an object from the sourceObjects collection based on the probability-weight 
        /// factor extracted by WeightExtractor using the supplied random number generator.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="random">A uniform random number generator</param>
        /// <param name="probabilityExtractor">function delegate to calculate the probability a certain object is drawn</param>
        /// <param name="numberOfDraws"></param>
        public static T[] DrawRandom<T>(this IEnumerable<T> source, IRandom random, Func<T, double> probabilityExtractor, int numberOfDraws) {
            var weights = source.Select(c => probabilityExtractor(c));
            var sumSamplingWeights = weights.Sum();
            var cumulativeWeights = weights.CumulativeWeights(c => c / sumSamplingWeights).ToArray();
            var drawnItems = new T[numberOfDraws];
            for (int i = 0; i < numberOfDraws; i++) {
                var u = random.NextDouble();
                var ix = Array.BinarySearch(cumulativeWeights, u);
                if (ix < 0) {
                    ix = ~ix;
                }
                drawnItems[i] = source.ElementAt(ix);
            }
            return drawnItems;
        }

        /// <summary>
        /// Draws a random object from the sourceObjects collection using the supplied random generator.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="random"></param>
        /// <param name="numberOfDraws"></param>
        public static T[] DrawRandom<T>(this IEnumerable<T> source, IRandom random, int numberOfDraws) {
            IList<T> sourceItems;
            if (source is IList<T>) {
                sourceItems = source as IList<T>;
            } else {
                sourceItems = source.ToList();
            }
            var n = sourceItems.Count;
            var drawnItems = new T[numberOfDraws];
            for (int i = 0; i < numberOfDraws; i++) {
                drawnItems[i] = sourceItems[random.Next(n)];
            }
            return drawnItems;
        }

        /// <summary>
        /// Fisher-Yates-Durstenfeld shuffle.
        /// From: https://stackoverflow.com/questions/5807128/an-extension-method-on-ienumerable-needed-for-shuffling
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="random"></param>
        /// <returns></returns>
        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source, IRandom random) {
            if (source == null) {
                throw new ArgumentNullException("source");
            }
            if (random == null) {
                throw new ArgumentNullException("random");
            }
            return source.shuffleIterator(random);
        }

        private static IEnumerable<T> shuffleIterator<T>(
            this IEnumerable<T> source, IRandom random) {
            var buffer = source.ToList();
            for (int i = 0; i < buffer.Count; i++) {
                int j = random.Next(i, buffer.Count);
                yield return buffer[j];
                buffer[j] = buffer[i];
            }
        }

        /// <summary>
        /// Projects all possible pair-combinations of the elements in the target collection.
        /// </summary>
        /// <typeparam name="TIn"></typeparam>
        /// <typeparam name="TOut"></typeparam>
        /// <param name="source"></param>
        /// <param name="selector"></param>
        /// <returns></returns>
        public static IEnumerable<TOut> SelectCombine<TIn, TOut>(this IEnumerable<TIn> source, Func<TIn, TIn, TOut> selector) {
            var s = source.ToList();
            var n = s.Count;
            for (int i = 0; i < n - 1; i++) {
                for (int j = i + 1; j < n; j++) {
                    yield return selector(s[i], s[j]);
                }
            }
        }

        public static IEnumerable<TResult> Zip<TFirst, TSecond, TThird, TResult>(this IEnumerable<TFirst> first, IEnumerable<TSecond> second, IEnumerable<TThird> third, Func<TFirst, TSecond, TThird, TResult> resultSelector) {
            Contract.Requires(first != null && second != null && third != null && resultSelector != null);
            using (IEnumerator<TFirst> iterator1 = first.GetEnumerator())
            using (IEnumerator<TSecond> iterator2 = second.GetEnumerator())
            using (IEnumerator<TThird> iterator3 = third.GetEnumerator()) {
                while (iterator1.MoveNext() && iterator2.MoveNext() && iterator3.MoveNext()) {
                    yield return resultSelector(iterator1.Current, iterator2.Current, iterator3.Current);
                }
            }
        }

        public static IEnumerable<TResult> Zip<TFirst, TSecond, TThird, TFourth, TResult>(this IEnumerable<TFirst> first, IEnumerable<TSecond> second, IEnumerable<TThird> third, IEnumerable<TFourth> fourth, Func<TFirst, TSecond, TThird, TFourth, TResult> resultSelector) {
            Contract.Requires(first != null && second != null && third != null && fourth != null && resultSelector != null);
            using (IEnumerator<TFirst> iterator1 = first.GetEnumerator())
            using (IEnumerator<TSecond> iterator2 = second.GetEnumerator())
            using (IEnumerator<TThird> iterator3 = third.GetEnumerator())
            using (IEnumerator<TFourth> iterator4 = fourth.GetEnumerator()) {
                while (iterator1.MoveNext() && iterator2.MoveNext() && iterator3.MoveNext() && iterator4.MoveNext()) {
                    yield return resultSelector(iterator1.Current, iterator2.Current, iterator3.Current, iterator4.Current);
                }
            }
        }

        public static void ForAll<T>(this IEnumerable<T> source, Action<T> action) {
            foreach (var item in source) {
                action(item);
            }
        }

        /// <summary>
        /// Creates a design matrix from the specified inputs.
        /// TODO: better document what this function does and/or remove it from this
        /// collection of extension methods.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static double[,] AsDesignMatrix(this IEnumerable<bool[]> source) {
            var rows = source.Count();
            var columns = source.First().Length + 1;
            int r = 0;
            var designMatrix = new double[rows, columns];
            foreach (var binaryValues in source) {
                designMatrix[r, 0] = 1;
                for (int c = 0; c < binaryValues.Length; c++) {
                    if (binaryValues[c]) {
                        designMatrix[r, c + 1] = 1;
                    }
                }
                r++;
            }
            return designMatrix;
        }

        /// <summary>
        /// PatternId is as follows for compound C1...C3 and 1 = positive concentration, 0 = zero concentration.
        /// patternId is equal to binary encoding
        /// id C1 C2 C3
        /// 0:  0  0  0
        /// 1:  0  0  1
        /// 2:  0  1  0
        /// 3:  0  1  1
        /// 4:  1  0  0
        /// 5:  1  0  1
        /// 6:  1  1  0
        /// 7:  1  1  1
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="truthExtractor"></param>
        /// <returns></returns>
        public static int GetPatternId<T>(this IEnumerable<T> source, Func<T, bool> truthExtractor) {
            var sourceList = source.ToList();
            var n = sourceList.Count;
            var result = 0;
            for (int i = 0; i < n; i++) {
                if (truthExtractor(sourceList[i])) {
                    result += Math.Pow(2, i).Floor();
                }
            }
            return result;
        }

        /// <summary>
        /// Finds the index of the first element matching the specified condition.
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <param name="items"></param>
        /// <param name="matchCondition"></param>
        /// <returns></returns>
        public static int FirstIndexMatch<TItem>(
            this IEnumerable<TItem> items,
            Func<TItem, bool> matchCondition
        ) {
            var index = 0;
            foreach (var item in items) {
                if (matchCondition.Invoke(item)) {
                    return index;
                }
                index++;
            }
            return -1;
        }
    }
}
