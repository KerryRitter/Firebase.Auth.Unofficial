using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Firebase.Auth.Unofficial
{
	/// <summary>
	/// The auth token provider.
	/// </summary>
	public class FirebaseAuthProvider : IFirebaseAuthProvider
	{
	    private readonly FirebaseConfig _config;
	    private const string GoogleIdentityUrl = "https://www.googleapis.com/identitytoolkit/v3/relyingparty/verifyAssertion?key={0}";
		private const string GoogleSignUpUrl = "https://www.googleapis.com/identitytoolkit/v3/relyingparty/signupNewUser?key={0}";
		private const string GooglePasswordUrl = "https://www.googleapis.com/identitytoolkit/v3/relyingparty/verifyPassword?key={0}";
		private const string GooglePasswordResetUrl = "https://www.googleapis.com/identitytoolkit/v3/relyingparty/getOobConfirmationCode?key={0}";
        private const string GoogleSetAccountUrl = "https://www.googleapis.com/identitytoolkit/v3/relyingparty/setAccountInfo?key={0}";
        private const string GoogleGetAccountUrl = "https://www.googleapis.com/identitytoolkit/v3/relyingparty/getAccountInfo?key={0}";

		public FirebaseAuthProvider(FirebaseConfig config)
		{
		    _config = config;
		}

		public async Task<FirebaseAuthLink> SignInWithEmailAndPasswordAsync(string email, string password)
		{
			var content = $"{{\"email\":\"{email}\",\"password\":\"{password}\",\"returnSecureToken\":true}}";

			return await SignInWithPostContentAsync(GooglePasswordUrl, content).ConfigureAwait(false);
		}

		public async Task<FirebaseAuthLink> CreateUserWithEmailAndPasswordAsync(string email, string password)
		{
			var content = $"{{\"email\":\"{email}\",\"password\":\"{password}\",\"returnSecureToken\":true}}";

			return await SignInWithPostContentAsync(GoogleSignUpUrl, content).ConfigureAwait(false);
        }

        public async Task<FirebaseAccountInfoResponse> GetAccountInfoAsync(string idToken)
        {
            using (var client = new HttpClient())
            {
                var postContent = $"{{\"idToken\":\"{idToken}\"}}";

                var response = await client.PostAsync(new Uri(string.Format(GoogleGetAccountUrl, _config.ApiKey)), new StringContent(postContent, Encoding.UTF8, "application/json"));
                var responseData = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                if (!response.IsSuccessStatusCode)
                {
                    throw new FirebaseInvalidTokenException("Invalid token");
                }

                return JsonConvert.DeserializeObject<FirebaseAccountInfoResponse>(responseData);
            }
        }

        public async Task SendPasswordResetEmailAsync(string email)
		{
			var content = $"{{\"requestType\":\"PASSWORD_RESET\",\"email\":\"{email}\"}}";

            using (var client = new HttpClient())
            {
                var response = await client.PostAsync(new Uri(string.Format(GooglePasswordResetUrl, _config.ApiKey)), new StringContent(content, Encoding.UTF8, "application/json")).ConfigureAwait(false);

                response.EnsureSuccessStatusCode();
            }
        }

        internal async Task<FirebaseAuthLink> SignInWithOAuthAsync(FirebaseAuthType authType, string oauthAccessToken)
        {
            var providerId = GetProviderId(authType);
            var content = $"{{\"postBody\":\"access_token={oauthAccessToken}&providerId={providerId}\",\"requestUri\":\"http://localhost\",\"returnSecureToken\":true}}";

            return await SignInWithPostContentAsync(GoogleIdentityUrl, content).ConfigureAwait(false);
        }

        internal async Task<FirebaseAuthLink> SignInAnonymouslyAsync()
        {
            var content = $"{{\"returnSecureToken\":true}}";

            return await SignInWithPostContentAsync(GoogleSignUpUrl, content).ConfigureAwait(false);
        }

        internal async Task<FirebaseAuthLink> LinkAccountsAsync(FirebaseAuth auth, string email, string password)
		{
			var content = $"{{\"idToken\":\"{auth.FirebaseToken}\",\"email\":\"{email}\",\"password\":\"{password}\",\"returnSecureToken\":true}}";

			return await SignInWithPostContentAsync(GoogleSetAccountUrl, content).ConfigureAwait(false);
		}

		internal async Task<FirebaseAuthLink> LinkAccountsAsync(FirebaseAuth auth, FirebaseAuthType authType, string oauthAccessToken)
		{
			var providerId = GetProviderId(authType);
			var content = $"{{\"idToken\":\"{auth.FirebaseToken}\",\"postBody\":\"access_token={oauthAccessToken}&providerId={providerId}\",\"requestUri\":\"http://localhost\",\"returnSecureToken\":true}}";

			return await SignInWithPostContentAsync(GoogleIdentityUrl, content).ConfigureAwait(false);
		}

		private async Task<FirebaseAuthLink> SignInWithPostContentAsync(string googleUrl, string postContent)
        {
            using (var client = new HttpClient())
            {
                var response = await client.PostAsync(new Uri(string.Format(googleUrl, _config.ApiKey)), new StringContent(postContent, Encoding.UTF8, "application/json")).ConfigureAwait(false);
			    var responseData = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

			    if (!response.IsSuccessStatusCode)
			    {
				    var jsonReturn = JObject.Parse(responseData);
				    var message = (string)jsonReturn["error"]["message"];

				    // Login
				    // Email address not found in database
				    if (message.Equals("EMAIL_NOT_FOUND"))
				    {
					    throw new FirebaseInvalidEmailException("Email address not found");
				    }
				    // Login
				    // Invalid password supplied
				    else if (message.Equals("INVALID_PASSWORD"))
				    {
					    throw new FirebaseIncorrectPasswordException("Incorrect passord");
				    }
				    // New User
				    // Email address already exists
				    else if (message.Equals("EMAIL_EXISTS"))
				    {
					    throw new FirebaseUsedEmailException("Email address already exists");
				    }
				    // New User
				    // Week Password
				    else if (message.Contains("WEAK_PASSWORD"))
				    {
					    throw new FirebaseWeakPasswordException("Weak password, must be at least 6 characters");
				    }
				    // Just end on default status check
				    else
				    {
					    response.EnsureSuccessStatusCode();
				    }
			    }

			    var user = JsonConvert.DeserializeObject<FirebaseUser>(responseData);
			    var auth = JsonConvert.DeserializeObject<FirebaseAuthLink>(responseData);

			    auth.User = user;
			    auth.AuthProvider = this;

			    return auth;
            }
        }

        private string GetProviderId(FirebaseAuthType authType)
		{
			switch (authType)
			{
				case FirebaseAuthType.Facebook:
					return "facebook.com";
				case FirebaseAuthType.Google:
					return "google.com";
				case FirebaseAuthType.Github:
					return "github.com";
				case FirebaseAuthType.Twitter:
					return "twitter.com";
				default: throw new NotImplementedException("");
			}
		}
	}
}
