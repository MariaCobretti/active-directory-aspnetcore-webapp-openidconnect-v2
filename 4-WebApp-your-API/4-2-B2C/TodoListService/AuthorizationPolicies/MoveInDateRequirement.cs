using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Identity.Web;
using TodoListService.Models;

namespace TodoListService.AuthorizationPolicies
{
    public class MoveInDateRequirement : IAuthorizationRequirement { }

    public class EditTaskAuthorizationHandler : AuthorizationHandler<MoveInDateRequirement, Todo>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, MoveInDateRequirement requirement, Todo resource)
        {
            if (context.User.Claims.All(x => x.Type != ClaimConstants.MoveInDate))
            {
                return Task.CompletedTask;
            }

            Claim scopeClaim = context?.User?.FindFirst(ClaimConstants.MoveInDate);

            if (scopeClaim != null && scopeClaim.Value.Split(' ').Contains(resource.MoveInDate))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }

}
