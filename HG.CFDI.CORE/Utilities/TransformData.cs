using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HG.CFDI.CORE.Utilities
{
    public class TransformData
    {
        // Método para remover acentos
        public static string RemoverAcentos(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            return string.Concat(
                input.Normalize(NormalizationForm.FormD)
                     .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
            ).Normalize(NormalizationForm.FormC);
        }
    }
}
