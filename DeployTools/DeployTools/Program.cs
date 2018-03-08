using System;
using System.IO;

namespace DeployTools
{
    class Program
    {
        static void Main(string[] args)
        {
            var serverPublicIp = "192.168.0.1";

            #region RunPowerShellCommand

            var currentDirectory = Directory.GetCurrentDirectory();
            Console.WriteLine("currentDirectory    => " + currentDirectory);
            var parent = Directory.GetParent(currentDirectory);
            var solutionDirectory = Directory.GetParent(parent?.Parent?.FullName).FullName;
            Console.WriteLine("solutionDirectory   => " + solutionDirectory);

            var serverApiDirectory = $@"{solutionDirectory}\src\Private.Server.Api";
            var ps1ScriptFileName = "l_package.ps1";
            var ps1FilePath = $@"{serverApiDirectory}\{ps1ScriptFileName}";


            var powerShellFileName = "powershell.exe";
            var powerShellDirectory = $@"C:\Windows\System32\WindowsPowerShell\v1.0\{powerShellFileName}";

            DeployToolsPowerShell.ExcuePowerShellCommand(serverApiDirectory, ps1ScriptFileName, ps1FilePath, powerShellFileName, powerShellDirectory);

            #endregion


            //TODO learn sftp  scp or other
            #region FtpFileToServer

            var zipFileDirectory = $@"{serverApiDirectory}\DescZipDir";
            var extension = "*.7z";
            string[] filePaths = Directory.GetFiles($@"{zipFileDirectory}\", extension, SearchOption.TopDirectoryOnly);
            if (filePaths.Length == 0 || filePaths == null)
            {
                Console.WriteLine($"Not found any file have extenson <[ {extension} ]> in this directory  <[ {zipFileDirectory} ]>.");
                Console.WriteLine("\nAnykey to Exit.");
                Console.ReadKey();
                Environment.Exit(0);
            }

            FileInfo lastedFile = null;
            foreach (var filePath in filePaths)
            {
                FileInfo nowFileInfo = new FileInfo(filePath);
                lastedFile = lastedFile == null ?
                    nowFileInfo :
                    nowFileInfo.CreationTime > lastedFile.CreationTime ?
                    nowFileInfo : lastedFile;
            }
            var zipDir = new DirectoryInfo(zipFileDirectory);
            var fileDir = new DirectoryInfo(lastedFile?.Directory?.FullName ?? throw new DirectoryNotFoundException());
            if (!zipDir.FullName.Equals(fileDir.FullName))
            {
                Console.WriteLine($"Have some trouble with directory <[ {zipFileDirectory} ]>.");
                Console.WriteLine("\nAnykey to Exit.");
                Console.ReadKey();
                Environment.Exit(0);
            }

            Console.WriteLine(lastedFile.Name);

            var fileName = lastedFile.Name;

            var ftpUrl = $"ftp://{serverPublicIp}";

            var ftpRequestUrl = $@"{ftpUrl}/zip/{fileName}";

            var ftpUserName = "lapadol";
            var ftpPassword = "password";

            var ftpToSendFilePath = lastedFile.FullName;
            DeployToolsFtp.FtpFileToServer(ftpRequestUrl, ftpUserName, ftpPassword, ftpToSendFilePath);



            var serviceFile = "kestrel-private-.service";
            if (!File.Exists(serviceFile))
            {
                Console.WriteLine($"Not found <[ {serviceFile} ]> file.");
                Console.WriteLine("\nAnykey to Exit.");
                Console.ReadKey();
                Environment.Exit(0);
            }

            FileInfo serviceFileInfo = new FileInfo(serviceFile);
            var ftpRequestUrlServiceFile = $@"{ftpUrl}/zip/{serviceFile}";
            DeployToolsFtp.FtpFileToServer(ftpRequestUrlServiceFile, ftpUserName, ftpPassword, serviceFileInfo.FullName);



            #endregion


            #region SshToServer
            Console.WriteLine(" ============================= SshToServer ============================= ");
            //https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/linux-nginx?tabs=aspnetcore2x
            var sshUrl = serverPublicIp;
            var sshUserName = "lapadol";
            var sshPassword = "password";

            var commands = new string[]
            {
                $"cd /home/{sshUserName}",
                //"mkdir zip",
                $"cd /home/{sshUserName}/var",
                //"mkdir www",

                "sudo apt-get install nginx",
                "sudo service nginx start",

                $"cd /home/{sshUserName}/zip/",
                "sudo apt-get install unrar",
                $"7zr e {fileName}",

                //todo remove file in old www
                $"cd  /{lastedFile.Name}",
                "mv . ../www",


                //process start nginx
                $"cd /home/{sshUserName}/zip/",
                $"sudo mv {serviceFile} /etc/systemd/system/",
                "cd /etc/systemd/system/",
                $"sudo systemctl enable {serviceFile}",
                $"sudo systemctl start {serviceFile}",
                $"systemctl status {serviceFile}"

            };
            DeployToolsSsh.SshCommandToServer(sshUrl, sshUserName, sshPassword, commands);

            #endregion 

            Console.WriteLine("");
            Console.ReadKey();
        }


        //sudo apt-get install ufw
        //sudo ufw enable
        //sudo ufw allow 80/tcp
        //sudo ufw allow 443/tcp

        //sudo apt-get update
        //sudo apt-get install build-essential zlib1g-dev libpcre3-dev libssl-dev libxslt1-dev libxml2-dev libgd2-xpm-dev libgeoip-dev libgoogle-perftools-dev libperl-dev

        //wget http://www.nginx.org/download/nginx-1.10.0.tar.gz
        //tar zxf nginx-1.10.0.tar.gz

        //  - // /etc/nginx/proxy.conf 
    }
}
