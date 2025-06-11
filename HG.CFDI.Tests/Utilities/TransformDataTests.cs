using HG.CFDI.CORE.Utilities;
using Xunit;

namespace HG.CFDI.Tests.Utilities
{
    public class TransformDataTests
    {
        [Theory]
        [InlineData("Árbol número ñandú", "Arbol numero nandu")]
        [InlineData("", "")]
        public void RemoverAcentos_RemovesDiacritics(string input, string expected)
        {
            var result = TransformData.RemoverAcentos(input);
            Assert.Equal(expected, result);
        }

        [Fact]
        public void RemoverAcentos_NullInput_ReturnsNull()
        {
            string? input = null;
            var result = TransformData.RemoverAcentos(input);
            Assert.Null(result);
        }
    }
}
