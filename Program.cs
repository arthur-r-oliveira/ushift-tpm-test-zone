using System;
using System.Diagnostics;
using System.IO;

namespace TpmTestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("--- TPM Access Test Application ---");
            
            string tpmDevice = "/dev/tpmrm0";
            if (File.Exists(tpmDevice))
            {
                Console.WriteLine($"[SUCCESS] Found TPM Resource Manager at {tpmDevice}");
            }
            else
            {
                Console.WriteLine($"[FAILURE] {tpmDevice} not found. Check MicroShift GDP configuration.");
                return;
            }

            try
            {
                Console.WriteLine("Requesting 16 random bytes from hardware TPM...");
                var psi = new ProcessStartInfo
                {
                    FileName = "tpm2_getrandom",
                    Arguments = "--hex 16",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false
                };

                using var process = Process.Start(psi);
                if (process != null)
                {
                    string output = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();
                    
                    if (process.ExitCode == 0)
                        Console.WriteLine($"[SUCCESS] TPM Data: {output.Trim()}");
                    else
                        Console.WriteLine($"[FAILURE] Exit Code: {process.ExitCode}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] {ex.Message}");
            }
        }
    }
}
