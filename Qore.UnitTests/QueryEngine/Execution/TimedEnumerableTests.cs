using NUnit.Framework;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using QoreDB.QueryEngine.Execution;
using System.Threading;

namespace Qore.UnitTests.QueryEngine.Execution
{
    [TestFixture]
    public class TimedEnumerableTests
    {
        [Test]
        public void TimedEnumerable_Should_Correctly_Enumerate_All_Items()
        {
            // Arrange
            var source = new List<IDictionary<string, object>>
            {
                new Dictionary<string, object> { { "id", 1 } },
                new Dictionary<string, object> { { "id", 2 } },
                new Dictionary<string, object> { { "id", 3 } }
            };

            TimeSpan executionTime = TimeSpan.Zero;
            var timedEnumerable = new TimedEnumerable(source, ts => executionTime = ts);

            // Act
            var result = timedEnumerable.ToList();

            // Assert
            result.Should().BeEquivalentTo(source);
        }

        [Test]
        public void TimedEnumerable_Should_Measure_Execution_Time()
        {
            // Arrange
            var source = new List<IDictionary<string, object>>
            {
                new Dictionary<string, object> { { "id", 1 } }
            };

            TimeSpan executionTime = TimeSpan.Zero;
            var timedEnumerable = new TimedEnumerable(source, ts => executionTime = ts);

            // Act
            // We need to actually enumerate the enumerable for the time to be measured.
            timedEnumerable.ToList();

            // Assert
            executionTime.Should().BeGreaterThan(TimeSpan.Zero);
        }

        [Test]
        public void TimedEnumerable_With_Empty_Source_Should_Not_Throw()
        {
            // Arrange
            var source = new List<IDictionary<string, object>>();
            TimeSpan executionTime = TimeSpan.Zero;
            var timedEnumerable = new TimedEnumerable(source, ts => executionTime = ts);

            // Act
            Action act = () => timedEnumerable.ToList();

            // Assert
            act.Should().NotThrow();
            executionTime.Should().BeLessThan(TimeSpan.FromMilliseconds(1));
        }
        
        [Test]
        public void TimedEnumerable_Should_Be_Accurate()
        {
            // Arrange
            var source = new SlowEnumerable();
            TimeSpan executionTime = TimeSpan.Zero;
            var timedEnumerable = new TimedEnumerable(source, ts => executionTime = ts);

            // Act
            timedEnumerable.ToList();

            // Assert
            executionTime.TotalMilliseconds.Should().BeGreaterThanOrEqualTo(100);
        }

        private class SlowEnumerable : IEnumerable<IDictionary<string, object>>
        {
            public IEnumerator<IDictionary<string, object>> GetEnumerator()
            {
                yield return new Dictionary<string, object> { { "id", 1 } };
                Thread.Sleep(100);
                yield return new Dictionary<string, object> { { "id", 2 } };
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }
}