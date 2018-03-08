using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace DeployTools
{
    public static class DeployToolsCmd
    {

        public static void ExcueCmdCommand()
        {
            #region CreateProcess

            Console.WriteLine("Start Process run Cmd.");
            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    //WorkingDirectory = @"c:\",
                    FileName = "cmd.exe",
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
                "%cd%" ,
                "cd ..",
               "%cd%" ,
                "cd ..",
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
            Console.WriteLine("End Process run Cmd.");
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
