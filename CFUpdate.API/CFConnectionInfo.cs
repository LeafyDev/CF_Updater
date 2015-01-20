// This program is a private software, based on c# source code.
// To sell or change credits of this software is forbidden,
// except if someone approve it from CFUpdate.API INC. team.
//  
// Copyrights (c) 2014 CFUpdate.API INC. All rights reserved.

namespace CFUpdate.API
{
    /// <summary>
    ///   Provides properties necessary for authenticating to the Cloudflare API
    /// </summary>
    public class CFConnectionInfo
    {
        /// <summary>
        ///   The email.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        ///   The token.
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        ///   The Id.
        /// </summary>
        public string ID { get; set; }
    }
}