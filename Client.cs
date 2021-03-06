﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Threading;
using System.Net;

namespace CommunicationLibrary
{
    // Eventhandler
    public delegate void ClientReceiveHandler(String type, String message);

    /// <summary>
    ///     Client class to handle communication with a server
    /// </summary>
    public class Client
    {
        // TCP
        private TcpClient client;
        private NetworkStream clientStream;

        /// <summary>
        ///     Client message event for incoming messages
        /// </summary>
        public ClientReceiveHandler onReceive;

        /// <summary>
        /// Connect to a server
        /// </summary>
        /// <param name="ip">The IP address of the server</param>
        /// <param name="port">The port of the server</param>
        /// <returns>Returns true of the connection attempt succeeded</returns>
        public Boolean connect(String ip, int port)
        {
            this.client = new TcpClient();

            try
            {
                // Get server and connect
                IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
                this.client.Connect(serverEndPoint);

                // Get stream
                this.clientStream = this.client.GetStream();

                // New thread
                Thread listener = new Thread(messageListener);

                // Dtart listener
                listener.Start();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        ///     Disconnect from the Server
        /// </summary>
        /// <returns></returns>
        public bool disconnect()
        {
            client.Close();
            return true;
        }

        /// <summary>
        ///     Send a string to server
        /// </summary>
        /// <param name="type">The type of the message</param>
        /// <param name="message">The message to send</param>
        /// <returns>Returns true if the message was sent successfully</returns>
        public bool send(String type, String message)
        {
            // Concatenate type and base64-encoded message with separator
            String data = Util.base64Encode(type) + ";" + Util.base64Encode(message) + "\n";

            // encode message
            ASCIIEncoding encoder = new ASCIIEncoding();
            byte[] buffer = encoder.GetBytes(data);

            // Send string to the server
            this.clientStream.Write(buffer, 0, buffer.Length);
            this.clientStream.Flush();

            return false;
        }

        /// <summary>
        ///     Listen for messages from Server.
        /// </summary>
        private void messageListener()
        {
            byte[] message = new byte[4096];
            int bytesRead;
            string remainder = "";

            // Loop forever while the client is connected
            while (client.Connected)
            {
                try
                {
                    bytesRead = 0;
                    try
                    {
                        // Wait for a message
                        bytesRead = this.clientStream.Read(message, 0, 4096);
                    }
                    catch
                    {
                        // Error
                        break;
                    }

                    if (bytesRead == 0)
                    {
                        // Connection closed?
                        break;
                    }


                    // Read message
                    ASCIIEncoding encoder = new ASCIIEncoding();
                    string get = remainder + encoder.GetString(message, 0, bytesRead);

                    while (get.Contains('\n'))
                    {
                        string[] data1 = get.Split(new Char[] { '\n' }, 2);
                        get = data1[1];
                        if (!data1[0].Contains(';')) throw new Exception("Received malformed message");
                        string[] data2 = data1[0].Split(new Char[] { ';' }, 2);
                        string type = Util.base64Decode(data2[0]);
                        string msg = Util.base64Decode(data2[1]);

                        // Call onReceive event
                        if (this.onReceive != null) this.onReceive(type, msg);
                    }
                    remainder = get;
                }
                catch
                { }
            }
        }
    }
}
