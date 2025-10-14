using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace QoreDB.QueryEngine.Execution.Models
{
    /// <summary>
    /// A read-only dictionary that provides a projected view of a source dictionary.
    /// </summary>
    /// <remarks>
    /// This class is used to efficiently represent a subset of columns from a row
    /// without creating a new dictionary and copying the data. It holds a reference
    /// to the original row and the list of projected columns.
    /// </remarks>
    public class ProjectedRow : IDictionary<string, object>
    {
        private readonly IDictionary<string, object> _source;
        private readonly ICollection<string> _columns;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectedRow"/> class.
        /// </summary>
        /// <param name="source">The source dictionary.</param>
        /// <param name="columns">The collection of columns to project.</param>
        public ProjectedRow(IDictionary<string, object> source, ICollection<string> columns)
        {
            _source = source;
            _columns = columns;
        }

        /// <summary>
        /// Gets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key of the value to get.</param>
        /// <returns>The value associated with the specified key.</returns>
        /// <exception cref="KeyNotFoundException">The property is retrieved and <paramref name="key"/> is not found.</exception>
        /// <exception cref="NotSupportedException">The property is set.</exception>
        public object this[string key]
        {
            get
            {
                if (_columns.Contains(key))
                {
                    return _source[key];
                }
                throw new KeyNotFoundException();
            }
            set => throw new NotSupportedException();
        }

        /// <summary>
        /// Gets a collection containing the keys in the <see cref="ProjectedRow"/>.
        /// </summary>
        public ICollection<string> Keys => _columns;

        /// <summary>
        /// Gets a collection containing the values in the <see cref="ProjectedRow"/>.
        /// </summary>
        public ICollection<object> Values => _columns.Select(c => _source[c]).ToList();

        /// <summary>
        /// Gets the number of elements contained in the <see cref="ProjectedRow"/>.
        /// </summary>
        public int Count => _columns.Count;

        /// <summary>
        /// Gets a value indicating whether the <see cref="ProjectedRow"/> is read-only.
        /// </summary>
        public bool IsReadOnly => true;

        /// <summary>
        /// This operation is not supported.
        /// </summary>
        /// <exception cref="NotSupportedException">This operation is not supported.</exception>
        public void Add(string key, object value) => throw new NotSupportedException();

        /// <summary>
        /// This operation is not supported.
        /// </summary>
        /// <exception cref="NotSupportedException">This operation is not supported.</exception>
        public void Add(KeyValuePair<string, object> item) => throw new NotSupportedException();

        /// <summary>
        /// This operation is not supported.
        /// </summary>
        /// <exception cref="NotSupportedException">This operation is not supported.</exception>
        public void Clear() => throw new NotSupportedException();

        /// <summary>
        /// Determines whether the <see cref="ProjectedRow"/> contains a specific value.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="ProjectedRow"/>.</param>
        /// <returns>true if <paramref name="item"/> is found in the <see cref="ProjectedRow"/>; otherwise, false.</returns>
        public bool Contains(KeyValuePair<string, object> item) => _columns.Contains(item.Key) && _source.Contains(item);

        /// <summary>
        /// Determines whether the <see cref="ProjectedRow"/> contains an element with the specified key.
        /// </summary>
        /// <param name="key">The key to locate in the <see cref="ProjectedRow"/>.</param>
        /// <returns>true if the <see cref="ProjectedRow"/> contains an element with the key; otherwise, false.</returns>
        public bool ContainsKey(string key) => _columns.Contains(key);

        /// <summary>
        /// Copies the elements of the <see cref="ProjectedRow"/> to an <see cref="Array"/>, starting at a particular <see cref="Array"/> index.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="Array"/> that is the destination of the elements copied from <see cref="ProjectedRow"/>. The <see cref="Array"/> must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in <paramref name="array"/> at which copying begins.</param>
        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            foreach (var key in _columns)
            {
                array[arrayIndex++] = new KeyValuePair<string, object>(key, _source[key]);
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return _columns.Select(key => new KeyValuePair<string, object>(key, _source[key])).GetEnumerator();
        }

        /// <summary>
        /// This operation is not supported.
        /// </summary>
        /// <exception cref="NotSupportedException">This operation is not supported.</exception>
        public bool Remove(string key) => throw new NotSupportedException();

        /// <summary>
        /// This operation is not supported.
        /// </summary>
        /// <exception cref="NotSupportedException">This operation is not supported.</exception>
        public bool Remove(KeyValuePair<string, object> item) => throw new NotSupportedException();

        /// <summary>
        /// Gets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key whose value to get.</param>
        /// <param name="value">When this method returns, the value associated with the specified key, if the key is found; otherwise, the default value for the type of the <paramref name="value"/> parameter. This parameter is passed uninitialized.</param>
        /// <returns>true if the object that implements <see cref="ProjectedRow"/> contains an element with the specified key; otherwise, false.</returns>
        public bool TryGetValue(string key, out object value)
        {
            if (_columns.Contains(key))
            {
                return _source.TryGetValue(key, out value);
            }
            value = null;
            return false;
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>An <see cref="IEnumerator"/> object that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}