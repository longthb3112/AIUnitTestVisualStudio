using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIUnittestExtension.ToolWindows
{
   public interface IUnitTestHandler
    {
        Task<string> GenerateUnitTestAsync(string selectedFilePath, string outPutFolderPath, string apiKey, string model, string inputPrompt);
    }
}
