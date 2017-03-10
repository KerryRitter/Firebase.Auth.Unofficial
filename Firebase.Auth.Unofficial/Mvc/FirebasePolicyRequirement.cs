using Microsoft.AspNetCore.Authorization;

namespace Firebase.Auth.Unofficial.Mvc
{
    public class FirebasePolicyRequirement : IAuthorizationRequirement
    {
        public readonly FirebaseConfig Config;

        public FirebasePolicyRequirement(FirebaseConfig config)
        {
            Config = config;
        }
    }

}
