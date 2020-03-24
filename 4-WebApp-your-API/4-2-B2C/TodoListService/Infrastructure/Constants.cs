using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TodoListService.Infrastructure
{
    public static class Constants
    {
        /// <summary>
        /// Contains a list of all the Azure AD app roles this app depends on and works with.
        /// </summary>
        public static class AppRole
        {
            /// <summary>
            /// User readers can read basic profiles of all users in the directory.
            /// </summary>
            public const string UserReaders = "UserReaders";

            /// <summary>
            /// Directory viewers can view objects in the whole directory.
            /// </summary>
            public const string DirectoryViewers = "DirectoryViewers";
        }

        public static class Groups
        {
            public const string Admin = "Admin";
            public const string User = "User";
        }

        /// <summary>
        /// Wrapper class the contain all the authorization policies available in this application.
        /// </summary>
        public static class AuthorizationPolicies
        {
            public const string AssignmentToUserReaderRoleRequired = "AssignmentToUserReaderRoleRequired";
            public const string AssignmentToDirectoryViewerRoleRequired = "AssignmentToDirectoryViewerRoleRequired";

            public const string AssignmentToAdminGroupRequired = "AssignmentToAdminGroupRequired";
        }
    }
}
