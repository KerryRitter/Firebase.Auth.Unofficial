using System;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Primitives;

namespace Firebase.Auth.Unofficial.Mvc
{
    public class FirebaseAuthorizationHandler : AuthorizationHandler<FirebasePolicyRequirement>
    {
        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, FirebasePolicyRequirement requirement)
        {
            var filterContext = context.Resource as AuthorizationFilterContext;

            if (filterContext?.HttpContext != null)
            {
                var headers = filterContext.HttpContext.Request?.Headers;
                if (headers?.TryGetValue("Authorization", out StringValues authorizationHeader) == false)
                {
                    context.Fail();
                }
                else
                {
                    var token = authorizationHeader.FirstOrDefault()
                        ?.Split(' ')
                        ?.LastOrDefault()
                        ?.Trim();
                    try
                    {
                        IFirebaseAuthProvider firebaseAuthProvider = new FirebaseAuthProvider(requirement.Config);

                        var accountInfo = await firebaseAuthProvider.GetAccountInfoAsync(token);

                        if (accountInfo?.Accounts?.Any() == true)
                        {
                            var account = accountInfo.Accounts.First();
                            var identity = new GenericIdentity(account.Email);
                            filterContext.HttpContext.User = new ClaimsPrincipal(identity);
                            context.Succeed(requirement);
                        }
                        else
                        {
                            context.Fail();
                        }
                    }
                    catch (Exception)
                    {
                        context.Fail();
                    }
                }
            }
        }
    }
}
