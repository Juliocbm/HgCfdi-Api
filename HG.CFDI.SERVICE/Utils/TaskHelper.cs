using System;
using System.Threading.Tasks;
using Serilog;

namespace HG.CFDI.SERVICE.Utils
{
    public static class TaskHelper
    {
        public static async Task RunSafeAsync(Func<Task> task)
        {
            try
            {
                await task();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unhandled exception in background task");
            }
        }
    }
}
