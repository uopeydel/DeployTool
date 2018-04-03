using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TCP
{
    public class TcpClientHelper
    {
        private readonly TcpClient _client;
        private volatile bool _running = true;
        private readonly string _host;
        private readonly int _port;


        public TcpClientHelper(string host, int port)
        {
            _host = host;
            _port = port;
            _client = new TcpClient();
        }

        public void Dispose()
        {
            _client.Close();
            _running = false;
        }

        public string Id => _host + ":" + _port;

        public void Start()
        {
            if (_port == 0)
            {

            }
            _client.Connect(_host, _port);

            ThreadPool.QueueUserWorkItem(WaitSubResponse);
            WaitInputData(null);

        }

        private void WaitInputData(object data)
        {
            var dataSend = "";
            while (!dataSend.Equals("exit"))
            {
                if (!_client.Connected)
                {
                    SpinWait.SpinUntil(() => _client.Connected, TimeSpan.FromMilliseconds(5000));
                }
                else if (_client.Connected)
                {
                    Console.WriteLine("");

                    dataSend = Console.ReadLine();
                    byte[] bytesToSend = Encoding.ASCII.GetBytes(dataSend);
                    _client.GetStream().Write(bytesToSend, 0, bytesToSend.Length);

                }

            }

            Dispose();
        }

        private void WaitSubResponse(object data)
        {
            string dataReceived = "";
            if (!_client.Connected)
            {
                return;
            }

            var taskProcess = Task.Run(() =>
            {
                Thread.Sleep(TimeSpan.FromSeconds(5));
                ThreadPool.QueueUserWorkItem(WaitSubResponse);
            });
            taskProcess.Wait(10);

            var nvStream = _client.GetStream();

            try
            {
                byte[] buffer = new byte[_client.ReceiveBufferSize];

                int bytesRead = nvStream.Read(buffer, 0, _client.ReceiveBufferSize);

                dataReceived = Encoding.ASCII.GetString(buffer, 0, bytesRead);
            }
            catch
            {
                return;
            }

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("Response : " + dataReceived);
            Console.ResetColor();
            Console.WriteLine();



        }
    }
}
