using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace HG.CFDI.CORE.Models
{
    public class GeneralResponse<T>
    {
        public string Message { get; set; } // es necesario remplazar obsoleto
        public T Data { get; set; }// es necesario remplazar obsoleto

        public int TotalRecords { get; set; }
        public List<T> Items { get; set; } = new List<T>();
        public bool IsSuccess { get; set; } = true;//En el caso de los post y los put aplica como validador de accion completa para alertas de procesos exitosos
        public T Item { get; set; }
        public List<string> ErrorList { get; set; } = new List<string>();

        // Método para agregar errores específicos con información de línea y archivo
        public void AddError(
            string errorMessage,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            IsSuccess = false;
            var formattedError = $"{errorMessage}";
            //var formattedError = $"{errorMessage} (in {memberName} at {filePath}, line {lineNumber})";//Uso posible para logs
            ErrorList.Add(formattedError);
        }

    }
}
