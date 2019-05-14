using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SAM.IO
{
    class RemoteHandler
    {
        /// <summary>
        /// Connects to a server and tests the connection.
        /// </summary>
        /// <param name="server">Server's IP, defaults at local</param>
        /// <param name="port"></param>
        /// <returns>TcpClient</returns>

        public static TcpClient Connect(string server = "127.0.0.1", Int32 port = 9999)
        {
            int tries = 0;
            try
            {
                var client = new TcpClient(server, port);

                CallServer(client.GetStream(), "test");
                return client;
                
            }
            catch (ArgumentNullException e)
            {
                Console.WriteLine("ArgumentNullException: {0}", e);
            }
            catch (SocketException)
            {
                if (tries <= 10)
                {
                    ++tries;
                    return Connect(port: 9999 + tries);
                }
            }
            throw new Exception("Did not manage to create a Socket at port 9999-10009");
        }


        /// <summary>
        /// This method writes a message to the network stream and reads the response
        /// </summary>
        /// <param name="stream">A network stream (TCPClient)</param>
        /// <param name="message">Message to write to the stream</param>
        /// <returns>the response</returns>
        public static string CallServer(NetworkStream stream, string message)
        {
            // Get a client stream for reading and writing.
            //  Stream stream = client.GetStream();

            //byte[] data = Encoding.UTF8.GetBytes(message);
            byte[] data = Encoding.UTF8.GetBytes(message);
            // Send the message to the connected TcpServer. 

            stream.Write(data, 0, data.Length);

            // Receive the TcpServer.response.
            
            // Buffer to store the response bytes.
            data = new byte[256];

            string responseData = string.Empty;
            Int32 bytes;

            // Read the first batch of the TcpServer response bytes.
            while((bytes = stream.Read(data, 0, data.Length)) != 0) break;

            return Encoding.UTF8.GetString(data, 0, bytes);
        }
        /// <summary>
        /// This puts a canary up. It blocks untill it recieves an incoming connection
        /// </summary>
        public static void AwaitSignal()
        {
            IPAddress ipAddress = IPAddress.Parse("127.0.0.1");

            TcpListener listener = new TcpListener(ipAddress, 9998);

            listener.Start();
            Console.WriteLine("Waiting for server's ready signal");
            Socket client = listener.AcceptSocket();
            Console.WriteLine("Signal Recieved");
        }
    }
}
