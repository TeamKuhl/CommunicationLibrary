using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Threading;
using System.Net;


namespace CommunicationLibrary
{
    // Events
    public delegate void ServerReceiveHandler(TcpClient client, String type, String data);
    public delegate void ServerConnectHandler(TcpClient client);
    public delegate void ServerDisconnectHandler(TcpClient client);

    /// <summary>
    ///     Server class for TCP communication
    /// </summary>
    public class Server
    {
        // TCP
        private TcpListener tcpListener;
        private Thread listenThread;
        private List<TcpClient> allClients = new List<TcpClient>();
        private Boolean serverActive;

        // Event handler
        public ServerReceiveHandler onReceive;
        public ServerConnectHandler onConnect;
        public ServerDisconnectHandler onDisconnect;

        /// <summary>
        ///     Initialize the server 
        /// </summary>
        public Server()
        {
        }

        /// <summary>
        ///     Start the server
        /// </summary>
        /// <param name="port">The port to start the server on</param>
        /// <returns>Returns false if the server is already started</returns>
        public Boolean start(int port)
        {
            if (!serverActive)
            {
                serverActive = true;

                // Set up the TCP listener
                // TODO: Check if port is in use
                this.tcpListener = new TcpListener(IPAddress.Any, port);

                // Set up the listener thread and start the server
                this.listenThread = new Thread(new ThreadStart(ListenForClients));
                this.listenThread.Start();
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        ///     Stop the Server
        /// </summary>
        /// <returns>Returns false if the server is already stopped</returns>
        public Boolean stop()
        {
            // TODO: improve stop function
            if (serverActive)
            {
                serverActive = false;
                this.tcpListener.Stop();
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        ///     Send a string to a client
        /// </summary>
        /// <param name="client">The client to send the message to</param>
        /// <param name="type">The type of the message</param>
        /// <param name="message">The message to send</param>
        /// <returns>Returns true if the message was sent successfully</returns>
        public Boolean send(TcpClient client, String type, String message)
        {
            // Concatenate type and base64-encoded message with separator
            String data = type + ";" + Util.base64Encode(message) + "\n";

            // Get the NetworkStream for the target
            NetworkStream clientStream = client.GetStream();

            // Encode the message for transmission
            ASCIIEncoding encoder = new ASCIIEncoding();
            byte[] buffer = encoder.GetBytes(data);

            // Send the message to the client
            clientStream.Write(buffer, 0, buffer.Length);
            clientStream.Flush();

            return true;
        }

        /// <summary>
        ///     Send a string to all clients
        /// </summary>
        /// <param name="data">The message to send</param>
        /// <returns>Returns true if the message was sent to all clients successfully</returns>
        public Boolean sendToAll(String type, String message)
        {
            bool retVal = true;

            // Loop all clients
            foreach (TcpClient client in this.allClients)
            {
                // Only update retVal if it hasn't been set to false yet
                if (retVal)
                {
                    // Send message to current client
                    retVal = this.send(client, type, message);
                }
                else
                {
                    this.send(client, type, message);
                }
            }
            return retVal;
        }

        /// <summary>
        ///     Send data to all clients except one
        /// </summary>
        /// <param name="data">The message to send</param>
        /// <param name="exception">The client who won't get the message</param>
        /// <returns>Returns true if the message was sent to all clients successfully.</returns>
        public Boolean sendToAllExcept(TcpClient exception, String type, String message)
        {
            bool retVal = true;

            // Loop all clients
            foreach (TcpClient client in this.allClients)
            {
                if (client != exception)
                {
                    // Only update retVal if it hasn't been set to false yet
                    if (retVal)
                    {
                        // Send message to current client
                        retVal = this.send(client, type, message);
                    }
                    else
                    {
                        this.send(client, type, message);
                    }
                }
            }
            return retVal;
        }

        /// <summary>
        ///     Start the TCP listener so clients can connect
        /// </summary>
        private void ListenForClients()
        {
            if (serverActive) this.tcpListener.Start();
            else throw new ListenerException("Failed to start TCP listener: Server is not running!");

            while (serverActive)
            {
                Thread clientThread = new Thread(new ParameterizedThreadStart(HandleClientComm));

                // Catch exception when waiting for clients while the server is already closed
                try
                {

                    // Block until a client has connected to the server
                    TcpClient client = this.tcpListener.AcceptTcpClient();

                    // Create a thread to handle communication with this client
                    clientThread.Start(client);
                    // Second part = IP of client

                    // Call connect event
                    if(this.onConnect != null) this.onConnect(client);
                }
                catch (Exception) {}
            }
        }

        /// <summary>
        ///     Handle client communication.
        /// </summary>
        /// <param name="client">Client object</param>
        private void HandleClientComm(object client)
        {
            TcpClient tcpClient = (TcpClient)client;

            // Add to client list
            this.allClients.Add(tcpClient);

            NetworkStream clientStream = tcpClient.GetStream();

            byte[] message = new byte[4096];
            int bytesRead;
            string remainder = "";

            while (true)
            {
                bytesRead = 0;

                try
                {
                    // Block until a client sends a message
                    bytesRead = clientStream.Read(message, 0, 4096);
                }
                catch (System.IO.IOException)
                {
                    // A socket error has occured
                    break;
                }

                if (bytesRead == 0)
                {
                    // The client has disconnected from the server
                    break;
                }

                // Message was successfully received
                ASCIIEncoding encoder = new ASCIIEncoding();
                string get = remainder + encoder.GetString(message, 0, bytesRead);

                while (get.Contains('\n'))
                {
                    string[] data1 = get.Split(new Char[] { '\n' }, 2);
                    get = data1[1];
                    if (!data1[0].Contains(';')) throw new Exception("Received malformed message");
                    string[] data2 = data1[0].Split(new Char[] { ';' }, 2);
                    string type = data2[0];
                    string msg = Util.base64Decode(data2[1]);

                    // Call onReceive event
                    if (this.onReceive != null) this.onReceive(tcpClient, type, msg);
                }
                remainder = get;
            }

            // Disconnection event handler
            if(this.onDisconnect != null) this.onDisconnect(tcpClient);

            // Remove the client from active clients list
            this.allClients.Remove(tcpClient);

            // Close the socket as final  operation
            tcpClient.Close();
        }
    }
}
