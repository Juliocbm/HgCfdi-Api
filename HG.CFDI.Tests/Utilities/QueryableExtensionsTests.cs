using System.Collections.Generic;
using System.Linq;
using HG.CFDI.CORE.Utilities;
using Xunit;

namespace HG.CFDI.Tests.Utilities
{
    public class QueryableExtensionsTests
    {
        private class TestItem
        {
            public string Name { get; set; } = string.Empty;
            public int Age { get; set; }
        }

        [Fact]
        public void OrderByDynamic_SortsByPrimaryAndSecondary()
        {
            var data = new List<TestItem>
            {
                new TestItem { Name = "Bob", Age = 30 },
                new TestItem { Name = "Ana", Age = 20 },
                new TestItem { Name = "Bob", Age = 25 }
            };

            var result = data.AsQueryable()
                              .OrderByDynamic("Name", false, "Age", true)
                              .ToList();

            Assert.Equal(new[] { "Ana", "Bob", "Bob" }, result.Select(x => x.Name).ToArray());
            Assert.Equal(new[] { 20, 30, 25 }, result.Select(x => x.Age).ToArray());
        }
    }
}
