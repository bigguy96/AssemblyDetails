using System.Runtime.Remoting.Metadata.W3cXsd2001;

namespace ClassLibrary
{
    /// <summary>
    /// User class.
    /// </summary>
    public class User
    {
        /// <summary>
        /// User's first name.
        /// </summary>
        public string First { get; set; }
        
        /// <summary>
        /// User's last name.
        /// </summary>
        public string Last { get; set; }

        /// <summary>
        /// Get users information.
        /// </summary>
        /// <returns>Users fullname.</returns>
        public string GetUser()
        {
            return $"{First} {Last}";
        }

        /// <summary>
        /// This is just a test function.
        /// </summary>
        /// <param name="a">just a string</param>
        /// <param name="b">just an integer</param>
        /// <param name="c">just a boolean</param>
        /// <returns>A meaningless string.</returns>
        /// <remarks>This is just to test the XML documentation.</remarks>
        public string Test(string a, int b, bool c)
        {
            return "test";
        }
    }
}