using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunicationLibrary
{
    class Test
    {
        // Objects for testing the components
        private Server server;
        private Client client;
        private int port = 45454;
        private string host = "127.0.0.1";

        // Implement ITestable (wip)
        public bool testComponent()
        {
            return testComponents();
        }

        /// <summary>
        ///     Test different methods of the server and client
        ///     Note that this will fail if port 45454 is already in use
        /// </summary>
        /// <returns>Returns true if all tests passed</returns>
        public bool testComponents()
        {
            if (!testUtil()) return false;
            if (!testServerStart()) return false;
            if (!testClientStart()) return false;
            if (!testClientConnect()) return false;
            if (!testClientDisconnect()) return false;
            if (!testServerStop()) return false;

            // Return true at last if there was nothing returned yet
            return true;
        }

        /// <summary>
        ///     Test whether the base64 encoding / decoding works correctly
        /// </summary>
        /// <returns>Returns true if all tests passed</returns>
        private static bool testUtil()
        {
            try
            {
                // Test if encoding works
                if (Util.base64Encode("Some\nstring\nwith\nnewlines\n") != "U29tZQpzdHJpbmcKd2l0aApuZXdsaW5lcwo=") return false;
                // Test if decoding works
                if (Util.base64Decode("U29tZQpzdHJpbmcKd2l0aApuZXdsaW5lcwo=") != "Some\nstring\nwith\nnewlines\n") return false;

            }
            catch (Exception)
            {
                // Return false as exceptions are obviously not the expected result
                return false;
            }
            // Return true at last if there was nothing returned yet
            return true;
        }

        private bool testServerStart()
        {
            try
            {
                server = new Server();
                if (!server.start(45454)) return false;
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        private bool testClientStart()
        {
            try
            {
                client = new Client();
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        private bool testClientConnect()
        {
            try
            {
                if (!client.connect(host, port)) return false;
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        private bool testClientDisconnect()
        {
            try
            {
                if (!client.disconnect()) return false;
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        private bool testServerStop()
        {
            try
            {
                if (!server.stop()) return false;
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
    }
}
