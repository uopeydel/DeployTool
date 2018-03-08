using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace DeployTools
{
    public static class DeployToolsFtp
    {
        public static void FtpFileToServer(string requestUrl, string userName, string password, string filePath)
        {
            Console.WriteLine("Start frp file process.");
            Console.WriteLine(requestUrl);
            FtpWebRequest request =
                (FtpWebRequest)WebRequest.Create(requestUrl);
            request.Credentials = new NetworkCredential(userName, password);
            request.Method = WebRequestMethods.Ftp.UploadFile;

            using (Stream fileStream = File.OpenRead(filePath))
            using (Stream ftpStream = request.GetRequestStream())
            {
                byte[] buffer = new byte[10240];
                int read;
                while ((read = fileStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ftpStream.Write(buffer, 0, read);
                    Console.WriteLine("Uploaded {0} bytes", fileStream.Position);
                }
            }


            Console.WriteLine("\n");
            Console.WriteLine("End process,Anykey to continue.");
            Console.ReadKey();
        }
    }
}
