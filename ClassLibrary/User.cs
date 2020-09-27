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
    }
}