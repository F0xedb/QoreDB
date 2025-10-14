using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace QoreDB.QueryEngine.Execution
{
    /// <summary>
    /// Wraps an <see cref="IEnumerable{IDictionary{string, object}}"/> to time the enumeration process.
    /// </summary>
    public class TimedEnumerable : IEnumerable<IDictionary<string, object>>
    {
        private readonly IEnumerable<IDictionary<string, object>> _source;
        private readonly Action<TimeSpan> _setExecutionTime;

        /// <summary>
        /// Initializes a new instance of the <see cref="TimedEnumerable"/> class.
        /// </summary>
        /// <param name="source">The source enumerable to wrap.</param>
        /// <param name="setExecutionTime">An action to call with the total enumeration time.</param>
        public TimedEnumerable(IEnumerable<IDictionary<string, object>> source, Action<TimeSpan> setExecutionTime)
        {
            _source = source;
            _setExecutionTime = setExecutionTime;
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<IDictionary<string, object>> GetEnumerator()
        {
            return new TimedEnumerator(_source.GetEnumerator(), _setExecutionTime);
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>An <see cref="IEnumerator"/> object that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    /// <summary>
    /// Wraps an <see cref="IEnumerator{IDictionary{string, object}}"/> to time the enumeration process.
    /// </summary>
    public class TimedEnumerator : IEnumerator<IDictionary<string, object>>
    {
        private readonly IEnumerator<IDictionary<string, object>> _source;
        private readonly Action<TimeSpan> _setExecutionTime;
        private readonly Stopwatch _stopwatch;

        /// <summary>
        /// Initializes a new instance of the <see cref="TimedEnumerator"/> class.
        /// </summary>
        /// <param name="source">The source enumerator to wrap.</param>
        /// <param name="setExecutionTime">An action to call with the total enumeration time.</param>
        public TimedEnumerator(IEnumerator<IDictionary<string, object>> source, Action<TimeSpan> setExecutionTime)
        {
            _source = source;
            _setExecutionTime = setExecutionTime;
            _stopwatch = new Stopwatch();
        }

        /// <summary>
        /// Advances the enumerator to the next element of the collection.
        /// </summary>
        /// <returns>true if the enumerator was successfully advanced to the next element; false if the enumerator has passed the end of the collection.</returns>
        public bool MoveNext()
        {
            _stopwatch.Start();
            var result = _source.MoveNext();
            _stopwatch.Stop();
            _setExecutionTime(_stopwatch.Elapsed);
            return result;
        }

        /// <summary>
        /// Sets the enumerator to its initial position, which is before the first element in the collection.
        /// </summary>
        public void Reset()
        {
            _source.Reset();
        }

        /// <summary>
        /// Gets the element in the collection at the current position of the enumerator.
        /// </summary>
        public IDictionary<string, object> Current => _source.Current;

        /// <summary>
        /// Gets the current element in the collection.
        /// </summary>
        object IEnumerator.Current => Current;

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _source.Dispose();
        }
    }
}