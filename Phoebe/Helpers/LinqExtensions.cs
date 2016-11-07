using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Collections.Generic;

namespace Toggl.Phoebe.Helpers
{
    public static class LinqExtensions
    {
        /// <summary>
        /// Filters out null results from chooser function
        /// </summary>
        public static IEnumerable<U> Choose<T, U> (this IEnumerable<T> items, Func<T, U> chooser)
        where U : class
        {
            foreach (var item in items)
            {
                var res = chooser(item);
                if (res != null)
                {
                    yield return res;
                }
            }
        }

        /// <summary>
        /// Replaces the first item for which predicates evaluates to true, or adds the replacement at the end
        /// </summary>
        public static IEnumerable<T> ReplaceOrAppend<T> (this IEnumerable<T> items, T replacement, Func<T, bool> predicate)
        {
            var replaced = false;
            foreach (var item in items)
            {
                if (!replaced && predicate(item))
                {
                    replaced = true;
                    yield return replacement;
                }
                else
                {
                    yield return item;
                }
            }

            if (!replaced)
            {
                yield return replacement;
            }
        }

        public static bool SequenceEqual<T1, T2> (this IEnumerable<T1> seqA, IEnumerable<T2> seqB, Func<T1, T2, bool> compare)
        {
            var bothFinished = false;
            var enum1 = seqA.GetEnumerator();
            var enum2 = seqB.GetEnumerator();

            while (!bothFinished)
            {
                var firstFinished = !enum1.MoveNext();
                var secondFinished = !enum2.MoveNext();

                if (firstFinished && secondFinished)
                {
                    bothFinished = true;
                }
                else if (firstFinished || secondFinished)
                {
                    return false;
                }
                else
                {
                    if (!compare(enum1.Current, enum2.Current))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public static bool TryFindKey<TKey, TValue> (this IDictionary<TKey, TValue> dic,
                out TKey result, Func<KeyValuePair<TKey, TValue>, bool> predicate)
        {
            foreach (var kv in dic)
            {
                if (predicate(kv))
                {
                    result = kv.Key;
                    return true;
                }
            }
            result = default (TKey);
            return false;
        }

        public static IEnumerable<T> CollapsePairs<T> (this IEnumerable<T> items, Func<T, T, T> collapse)
        where T : class
        {
            T previous = null;
            foreach (var current in items)
            {
                if (previous == null)
                {
                    previous = current;
                }
                else
                {
                    var res = collapse(previous, current);
                    if (res == null)
                    {
                        yield return previous;
                        previous = current;
                    }
                    else
                    {
                        yield return res;
                        previous = null;
                    }
                }
            }

            if (previous != null)
            {
                yield return previous;
            }
        }

        public static void ForEach<T> (this IEnumerable<T> items, Action<T> action)
        {
            foreach (var item in items)
            {
                action(item);
            }
        }

        /// <summary>
        /// Returns the index of the first element fulfilling the predicate. -1 if none does.
        /// </summary>
        public static int IndexOf<T> (this IEnumerable<T> items, Func<T, bool> predicate)
        {
            var i = 0;
            foreach (var item in items)
            {
                if (predicate(item))
                {
                    return i;
                }
                i++;
            }
            return -1;
        }
    }
}

