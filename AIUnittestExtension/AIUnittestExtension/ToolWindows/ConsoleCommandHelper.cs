using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIUnittestExtension
{
  public  class ConsoleCommandHelper
    {
        public static async Task<string> RunCommandAsync(string appPath, string args = "")
        {
            return await Task.Run(() =>
            {
                try
                {
                    if (!File.Exists(appPath))
                        return $"Error: File not found - {appPath}";

                    ProcessStartInfo psi = new ProcessStartInfo
                    {
                        FileName =  appPath,
                        Arguments = args,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    using Process process = new Process { StartInfo = psi };
                    process.Start();

                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();
                    process.WaitForExit();

                    return string.IsNullOrEmpty(error) ? output : $"Error: {error}";
                }
                catch (Exception ex)
                {
                    return $"Exception: {ex.Message}";
                }
            });
        }
    }
}
