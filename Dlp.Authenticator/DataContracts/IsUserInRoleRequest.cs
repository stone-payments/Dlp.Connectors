using System;
using System.Collections.Generic;

namespace Dlp.Authenticator.DataContracts {

    /// <summary>
    /// Class containing the data to check the user's roles.
    /// </summary>
    public sealed class IsUserInRoleRequest : AbstractRequest {

        /// <summary>
        /// Instantiates the IsUserInRoleRequest class.
        /// </summary>
        public IsUserInRoleRequest() : base() {

            this.RoleCollection = new List<string>();
        }

        /// <summary>
        /// Gets or sets the user key to be validated.
        /// </summary>
        public Guid UserKey { get; set; }

        /// <summary>
        /// Gets or sets the roles to be checked.
        /// </summary>
        public List<string> RoleCollection { get; set; }
    }
}
