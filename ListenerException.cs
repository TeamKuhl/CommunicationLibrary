using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunicationLibrary
{
    class ListenerException : Exception
    {
        public ListenerException() : base()
        {
        }

        public ListenerException(string message) : base(message)
        {
        }

        public ListenerException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
