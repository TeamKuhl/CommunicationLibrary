using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using Newtonsoft.Json;

namespace BomberLib
{
    /// <summary>
    ///     Server Class for TCP Communication.
    /// </summary>
    public class Server
    {
        // TCP
        private TcpListener tcpListener;
        private Thread listenThread;
        private List<TcpClient> allClients = new List<TcpClient>();
        private Log log;
        private Boolean loggingEnabled;
        private Boolean serveractive;

        /// <summary>
        ///     Initialize the server with logging
        /// </summary>
        /// <param name="thelog"></param>
        public Server(Log thelog)
        {
            log = thelog;
            loggingEnabled = true;
        }

        /// <summary>
        ///     Initialize the server without logging
        /// </summary>
        public Server()
        {
            loggingEnabled = false;
        }

        /// <summary>
        ///     Starts the server
        /// </summary>
        /// <param name="port">The port to start the server on.</param>
        /// <returns>Success of start process.</returns>
        public Boolean start(int port)
        {
            if (!serveractive)
            {
                serveractive = true;

                // set up tcp listener
                this.tcpListener = new TcpListener(IPAddress.Any, port);

                // set up thread & start the server
                this.listenThread = new Thread(new ThreadStart(ListenForClients));
                this.listenThread.Start();
                if (loggingEnabled) log.info("Server started on port " + port);
                return true;
            }
            else
            {
                if (loggingEnabled) log.error("Server is already running");
                return false;
            }
        }

        /// <summary>
        ///     Stops the Server
        /// </summary>
        /// <returns></returns>
        public Boolean stop()
        {
            if (serveractive)
            {
                serveractive = false;
                if (loggingEnabled) log.warn("Server is going to stop");
                this.tcpListener.Stop();
                return true;
            }
            else
            {
                if (loggingEnabled) log.error("Server is not running");
                return false;
            }
        }

        /// <summary>
        ///     Converts object to string and send to client
        /// </summary>
        /// <param name="client">The client to send the object to.</param>
        /// <param name="data">The object to send.</param>
        /// <returns></returns>
        public Boolean send(TcpClient client, Object data)
        {
            // convert to json
            string json = JsonConvert.SerializeObject(data);

            // send to client
            return this.sendString(client, json);
        }

        /// <summary>
        ///     Sends string to a client
        /// </summary>
        /// <param name="client">The client to send the message to.</param>
        /// <param name="message">The message to send.</param>
        /// <returns>Success of send process.</returns>
        private Boolean sendString(TcpClient client, String message)
        {
            // get client stream
            NetworkStream clientStream = client.GetStream();

            // encode message
            ASCIIEncoding encoder = new ASCIIEncoding();
            byte[] buffer = encoder.GetBytes(message);

            // send to client
            clientStream.Write(buffer, 0, buffer.Length);
            clientStream.Flush();

            return true;
        }

        /// <summary>
        ///     Send data object to all clients
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <returns>Success of send process.</returns>
        public Boolean sendToAll(String message)
        {
            // loop all clients
            foreach (TcpClient client in this.allClients)
            {
                // send message
                this.send(client, message);
            }
            return false;
        }

        /// <summary>
        ///     Send data to all clients except one
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <param name="exception">The client who wont get the message</param>
        /// <returns>Success of send process.</returns>
        public Boolean sendToAllExcept(TcpClient exception, String message)
        {
            // loop all clients
            foreach (TcpClient client in this.allClients)
            {
                if (client != exception)
                {
                    // send message
                    this.send(client, message);
                }
            }
            return false;
        }

        /// <summary>
        ///     Listener for Communication
        /// </summary>
        private void ListenForClients()
        {
            if(serveractive) this.tcpListener.Start();

            while (serveractive)
            {
                Thread clientThread = new Thread(new ParameterizedThreadStart(HandleClientComm));

                //getting error when waiting for client while server is already closed
                try
                {

                    //blocks until a client has connected to the server
                    TcpClient client = this.tcpListener.AcceptTcpClient();

                    //create a thread to handle communication 
                    //with connected client
                    clientThread.Start(client);
                    // second part = IP of client
                    if (loggingEnabled) log.info("New connection to " + ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString());
                }
                catch (Exception e)
                {
                    if (loggingEnabled) log.warn("Server stopped");
                }
            }
        }

        /// <summary>
        ///     Handles client communication.
        /// </summary>
        /// <param name="client">Client object</param>
        private void HandleClientComm(object client)
        {
            TcpClient tcpClient = (TcpClient)client;

            // add to client list
            this.allClients.Add(tcpClient);

            NetworkStream clientStream = tcpClient.GetStream();

            byte[] message = new byte[4096];
            int bytesRead;

            while (true)
            {
                bytesRead = 0;

                try
                {
                    //blocks until a client sends a message
                    bytesRead = clientStream.Read(message, 0, 4096);
                }
                catch
                {
                    //a socket error has occured
                    if(loggingEnabled) log.error("a socket error has occurred");
                    break;
                }

                if (bytesRead == 0)
                {
                    //the client has disconnected from the server
                    break;
                }

                //message has successfully been received
                ASCIIEncoding encoder = new ASCIIEncoding();
                String newstring = encoder.GetString(message, 0, bytesRead);
                Console.WriteLine(newstring);
                this.sendToAllExcept(tcpClient, newstring);
            }

            // remove from array
            this.allClients.Remove(tcpClient);

            tcpClient.Close();
            if(loggingEnabled) log.info("the client has disconnected from the server");
        }

    }
}
