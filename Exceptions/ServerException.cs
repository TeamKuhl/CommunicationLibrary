using System;
using System.Runtime.Serialization;

namespace CommunicationLibrary
{
    [Serializable]
    public class ServerException : Exception
    {
        public ServerException()
        {
            // TODO: Implement exception
        }
        
        public ServerException(string message)
        {
            // TODO: Implement exception
        }

        public ServerException(string message, Exception inner)
        {
            // TODO: Implement exception
        }

        // This constructor is needed for serialization.
        public ServerException(SerializationInfo info, StreamingContext context)
        {
            // TODO: Implement exception
        }
    }
}
