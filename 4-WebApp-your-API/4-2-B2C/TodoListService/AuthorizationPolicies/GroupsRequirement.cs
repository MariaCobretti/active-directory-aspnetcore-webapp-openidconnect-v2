using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.Identity.Web;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace TodoListService.AuthorizationPolicies
{
    /// <summary>
    /// Requirement used in authorization policies, to check if the groups claim has at least one of the requirement values.
    /// Since the class also extends AuthorizationHandler, its dependency injection is done out of the box.
    /// </summary>
    public class GroupsRequirement : AuthorizationHandler<GroupsRequirement>, IAuthorizationRequirement
    {
        string[] _acceptedGroups;

        public GroupsRequirement(params string[] acceptedGroups)
        {
            _acceptedGroups = acceptedGroups;
        }


        /// <summary>
        /// AuthorizationHandler that will check if the groups claim has at least one of the requirement values
        /// </summary>
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, GroupsRequirement requirement)
        {
            var groupClaims = context.User?.Claims
                .Where(c => c.Type == ClaimConstants.Groups)
                .Select(c => c.Value)
                .ToList();

            // If there are no groups, do not process
            if (groupClaims == null || !groupClaims.Any())
            {
                return Task.CompletedTask;
            }

            if (requirement._acceptedGroups.Any(group => groupClaims.Contains(group))) 
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }

    }
}
