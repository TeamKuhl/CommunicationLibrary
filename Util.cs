using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunicationLibrary
{
    /// <summary>
    ///     Utility class
    /// </summary>
    class Util
    {
        /// <summary>
        ///     Encode a given string with base64
        /// </summary>
        /// <param name="str">String to encode</param>
        /// <returns>Returns the base64-encoded string</returns>
        public static string base64Encode(string str)
        {
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(str));
        }

        /// <summary>
        ///     Decode a given base64 string
        /// </summary>
        /// <param name="str">Base64-encoded string</param>
        /// <returns>Returns the decoded string</returns>
        public static string base64Decode(string str)
        {
            return System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(str));
        }
    }
}
