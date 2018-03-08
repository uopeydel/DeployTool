using System;
using System.Collections.Generic;
using System.Text;
using Renci.SshNet;

namespace DeployTools
{
    public static class DeployToolsSsh
    {
        public static void SshCommandToServer(string sshUrl, string sshUserName, string sshPassword, string[] commands)
        {
            using (var client = new SshClient(sshUrl, sshUserName, sshPassword))
            {
                client.Connect();
                foreach (var cmd in commands)
                {
                    var res = client.RunCommand(cmd);
                    Console.WriteLine("Run ==========> " + res.CommandText);
                    Console.WriteLine("ExitStatus   => " + res.ExitStatus);
                    Console.WriteLine("Result       => " + res.Result);
                    Console.WriteLine("OutputStream => " + res.OutputStream);
                    Console.WriteLine("Error        => " + res.Error);
                    Console.WriteLine("\n");
                }

                client.Disconnect();
            }

            Console.WriteLine("\n");
            Console.WriteLine("Anykey to continue.");
            Console.ReadKey();
        }
    }
}
