using Newtonsoft.Json;

namespace Firebase.Auth.Unofficial
{
    public class FirebaseAccountInfoResponse
    {
        [JsonProperty("users")]
        public FirebaseAccountInfo[] Accounts { get; set; }
    }

    public class FirebaseAccountInfo
    {
        [JsonProperty("localId")]
        public string LocalId { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("emailVerified")]
        public bool EmailVerified { get; set; }
    }
}
