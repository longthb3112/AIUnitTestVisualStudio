using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIUnittestExtension.ToolWindows.AI
{
    public interface IAI
    {
        Task<string> ExecuteAsync(string prompt, string apiKey, string model);
    }
}
