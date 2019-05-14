using System;
using System.Collections.Generic;
using System.Text;

namespace SAM.Interfaces
{
    /// <summary>
    /// A way to uniquly idenfity objects
    /// </summary>
    public interface IIdentifier
    {
        /// <summary>
        /// Unique ID
        /// </summary>
        /// <returns>int id</returns>
        int getId();
        /// <summary>
        /// The Original string
        /// </summary>
        /// <returns>Original string</returns>
        string getOriginal();
    }
}
