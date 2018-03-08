using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace DeployTools
{
    public static class DeployToolsPowerShell
    {

        public static void ExcuePowerShellCommand(string serverApiDirectory, string ps1ScriptFileName, string ps1FilePath, string powerShellFileName, string powerShellDirectory)
        {
            Console.WriteLine("Check file for Start Process run PowerShell.");
            if (!File.Exists(ps1FilePath))
            {
                throw new FileNotFoundException(ps1FilePath);
            }
            else
            {
                Console.WriteLine("ps1FilePath         => " + ps1FilePath);
            }

            if (!File.Exists(powerShellDirectory))
            {
                var errorText = $"File <[ {powerShellFileName} ]> in directory <[ {powerShellDirectory} ]> not found.";
                Console.WriteLine(errorText);
                throw new FileNotFoundException(errorText);
            }

            #region CreateProcess

            Console.WriteLine("Start Process run PowerShell.");
            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    WorkingDirectory = serverApiDirectory,
                    FileName = powerShellDirectory,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardInput = true,
                    CreateNoWindow = false,
                    WindowStyle = ProcessWindowStyle.Normal,
                }
            };
            if (System.Environment.OSVersion.Version.Major >= 6)
            {
                proc.StartInfo.Verb = "runas";
            }
            else
            {
                var errorText = "Os is to old version, Pls use Vista or higher.";
                Console.WriteLine(errorText);
                throw new VersionNotFoundException(errorText);
            }
            #endregion

            var commands = new string[]
            {
                "Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass" , //fix for powershell script not digitally signed
                $@".\{ps1ScriptFileName}"
            };

            proc.Start();

            try
            {
                ProcessStartArguments(proc, commands);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.WriteLine("\nAnykey to Exit.");
                Console.ReadKey();
                Environment.Exit(0);
            }

            proc.Close();
            Console.WriteLine("End Process run PowerShell.");
            Console.WriteLine("\n");
            Console.WriteLine("Anykey to continue.");
            Console.ReadKey();
        }

        private static void ProcessStartArguments(Process process, string[] arguments)
        {
            using (StreamWriter sw = process.StandardInput)
            {
                foreach (var argument in arguments)
                {
                    if (sw.BaseStream.CanWrite)
                    {
                        sw.WriteLine(argument);
                    }
                }
            }

            var result = "";
            while (!process.StandardOutput.EndOfStream)
            {
                result += process.StandardOutput.ReadLine() + "\n";
            }
            Console.WriteLine(" Result =>" + result);
        }

    }
}
