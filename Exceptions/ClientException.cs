using System;
using System.Runtime.Serialization;

namespace CommunicationLibrary
{
    [Serializable]
    public class ClientException : Exception
    {
        public ClientException()
        {
            // TODO: Implement exception
        }
        
        public ClientException(string message)
        {
            // TODO: Implement exception
        }

        public ClientException(string message, Exception inner)
        {
            // TODO: Implement exception
        }

        // This constructor is needed for serialization.
        public ClientException(SerializationInfo info, StreamingContext context)
        {
            // TODO: Implement exception
        }
    }
}
