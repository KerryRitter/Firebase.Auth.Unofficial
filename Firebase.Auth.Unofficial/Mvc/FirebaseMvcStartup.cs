using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace Firebase.Auth.Unofficial.Mvc
{
    public static class FirebaseMvcStartup
    {
        public static void Configure(string apiKey, IServiceCollection services, Action<AuthorizationOptions> configure = null)
        {
            var config = new FirebaseConfig {ApiKey = apiKey};

            services.AddAuthorization(o =>
            {
                o.AddPolicy("Firebase", p => p.Requirements.Add(new FirebasePolicyRequirement(config)));

                configure?.Invoke(o);
            });

            services.AddScoped<IAuthorizationHandler, FirebaseAuthorizationHandler>();
            services.AddScoped<IFirebaseAuthProvider, FirebaseAuthProvider>();
            services.AddSingleton(p => config);
        }
    }
}
