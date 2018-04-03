using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TCP
{

    public class TcpServerHelper
    {
        private readonly TcpListener _tcpServer;
        private readonly object _syncRoot = new object();
        private volatile bool _isRunning = true;
        private string _id;
        private int _dynamicPort;
        private readonly int _port;

        public TcpServerHelper(int port)
        {
            _port = port;
            lock (_syncRoot)
            {
                _id = IPAddress.Loopback + ":" + port;
            }
            _tcpServer = new TcpListener(IPAddress.Loopback, port);
            _tcpServer.AllowNatTraversal(true);
        }

        public int Port
        {
            get
            {
                var toParse = _tcpServer.LocalEndpoint.ToString().Split(':')[1];
                return Int32.Parse(toParse);
            }
        }

        public void Dispose()
        {
            _isRunning = false;
        }

        public string Id
        {
            get
            {
                lock (_syncRoot)
                {
                    return _id;
                }
            }
        }

        public void Start()
        {
            int backlog = 5000;
            _tcpServer.Start(backlog);
            UpdateIds();

            WaitSubMessage(null);

            while (_isRunning)
            {
                Thread.Sleep(TimeSpan.FromMinutes(10));
            }
        }

        private void WaitSubMessage(object data)
        {
            var taskProcess = Task.Run(() =>
            {
                Thread.Sleep(TimeSpan.FromSeconds(5));
                ThreadPool.QueueUserWorkItem(WaitSubMessage);
            });
            taskProcess.Wait(10);

            if (_tcpServer.Pending())
            {
                TcpClient client = _tcpServer.AcceptTcpClient();
                while (client.Connected)
                {
                    WaitPubMessage(client);
                }
            }

        }

        private void UpdateIds()
        {
            _dynamicPort = Int32.Parse(_tcpServer.LocalEndpoint.ToString().Split(':')[1]);
            if (_port != _dynamicPort)
            {
                lock (_syncRoot)
                {
                    _id = IPAddress.Loopback + ":" + _dynamicPort;
                }
            }
        }

        private void WaitPubMessage(TcpClient client)
        {
            string dataReceived = "";
            NetworkStream nwStream = client.GetStream();

            try
            {
                byte[] buffer = new byte[client.ReceiveBufferSize];
                int bytesRead = nwStream.Read(buffer, 0, client.ReceiveBufferSize);
                dataReceived = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                if (dataReceived.Equals("exit"))
                {
                    client.Close();
                    client.Dispose();
                    return;
                }
            }
            catch
            {
                return;
            }

            SamplerProcess(nwStream, dataReceived);
        }

        private void SamplerProcess(NetworkStream nwStream, string dataReceived)
        {
            Console.WriteLine("Received : " + dataReceived);

            byte[] beforeStartTask = Encoding.ASCII.GetBytes("Please wait : " + dataReceived);
            nwStream.Write(beforeStartTask, 0, beforeStartTask.Length);



            var taskM = Task.Run(() =>
            {
                Thread.Sleep(TimeSpan.FromSeconds(5));
                byte[] bytesToSend = Encoding.ASCII.GetBytes("Finish : " + dataReceived);
                nwStream.Write(bytesToSend, 0, bytesToSend.Length);
            });
            taskM.Wait(10);

        }

    }

}
