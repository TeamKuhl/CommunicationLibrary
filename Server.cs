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
    ///     Server Class for TCP Communication.
    /// </summary>
    public class Server
    {
        // TCP
        private TcpListener tcpListener;
        private Thread listenThread;
        private List<TcpClient> allClients = new List<TcpClient>();
        private Boolean serverActive;

        // Event Handler
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
        ///     Starts the server
        /// </summary>
        /// <param name="port">The port to start the server on.</param>
        /// <returns>Success of start process.</returns>
        public Boolean start(int port)
        {
            if (!serverActive)
            {
                serverActive = true;

                // set up tcp listener
                this.tcpListener = new TcpListener(IPAddress.Any, port);

                // set up thread & start the server
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
        ///     Stops the Server
        /// </summary>
        /// <returns></returns>
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
        ///     Sends string to a client
        /// </summary>
        /// <param name="client">The client to send the message to.</param>
        /// <param name="message">The message to send.</param>
        /// <returns>Success of send process.</returns>
        public Boolean send(TcpClient client, String type, String message)
        {
            // parse
            String data = type + ";" + message;

            // get client stream
            NetworkStream clientStream = client.GetStream();

            // encode message
            ASCIIEncoding encoder = new ASCIIEncoding();
            byte[] buffer = encoder.GetBytes(data);

            // send to client
            clientStream.Write(buffer, 0, buffer.Length);
            clientStream.Flush();

            return true;
        }

        /// <summary>
        ///     Send data object to all clients
        /// </summary>
        /// <param name="data">The message to send.</param>
        /// <returns>Success of send process.</returns>
        public Boolean sendToAll(String type, String message)
        {
            // loop all clients
            foreach (TcpClient client in this.allClients)
            {
                // send message
                this.send(client, type, message);
            }
            return false;
        }

        /// <summary>
        ///     Send data to all clients except one
        /// </summary>
        /// <param name="data">The message to send.</param>
        /// <param name="exception">The client who wont get the message</param>
        /// <returns>Success of send process.</returns>
        public Boolean sendToAllExcept(TcpClient exception, String type, String message)
        {
            // loop all clients
            foreach (TcpClient client in this.allClients)
            {
                if (client != exception)
                {
                    // send message
                    this.send(client, type, message);
                }
            }
            return false;
        }

        /// <summary>
        ///     Listener for Communication
        /// </summary>
        private void ListenForClients()
        {
            if (serverActive) this.tcpListener.Start();

            while (serverActive)
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

                    // call connect event
                    if(this.onConnect != null) this.onConnect(client);
                    
                }
                catch (Exception e)
                {

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
                    break;
                }

                if (bytesRead == 0)
                {
                    //the client has disconnected from the server
                    break;
                }

                //message has successfully been received
                ASCIIEncoding encoder = new ASCIIEncoding();
                String receivedMessage = encoder.GetString(message, 0, bytesRead);

                String[] data = receivedMessage.Split(new Char[]{';'});

                String type = data[0];
                data = data.Where(w => w != data[0]).ToArray();
                String msg = string.Join(";", data);
                
                // call onReceive event
                if(this.onReceive != null) this.onReceive(tcpClient, type, msg);


            }

            // disconnect event handler
            if(this.onDisconnect != null) this.onDisconnect(tcpClient);

            // remove from array
            this.allClients.Remove(tcpClient);

            tcpClient.Close();
        }

    }
}
