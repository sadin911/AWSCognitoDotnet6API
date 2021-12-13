using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Amazon;
using Amazon.CognitoIdentityProvider;
using Amazon.Extensions.CognitoAuthentication;
using Amazon.Runtime;
using Amazon.CognitoIdentityProvider.Model;
namespace simpleCognitoAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CognitoController : ControllerBase
    {
        private readonly ILogger<CognitoController> _logger;
        private readonly IConfiguration _config;
        public CognitoController(ILogger<CognitoController> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
        }
        public static async Task<SignUpResponse> RegisUserAsync(string username, string password,IConfiguration config)
        {
            var UserPoolClientId = config["AWS:UserPoolClientId"];
            var UserPoolId = config["AWS:UserPoolId"];
            var AccessId = config["AWS:AccessId"];
            var AccessSecret = config["AWS:AccessSecret"];
            var credentials = new BasicAWSCredentials(AccessId, AccessSecret);
            var provider = new AmazonCognitoIdentityProviderClient(credentials, RegionEndpoint.APSoutheast1);
            var pool = new CognitoUserPool(UserPoolId, UserPoolClientId, provider, "");
            Dictionary<string, string> userAttributes = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            { "name", username}
        };
            Dictionary<string, string> validationData = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            { "name", username}
        };
            AdminConfirmSignUpRequest req = new AdminConfirmSignUpRequest()
            {
                Username = username,
                UserPoolId = "ap-southeast-1_VUkxhbBya"
            };
            List<AttributeType> attributeType = new List<AttributeType>();
            attributeType.Add(new AttributeType()
            {
                Name = "name",
                Value = username
            });

            SignUpRequest signupReq = new SignUpRequest()
            {
                ClientId = UserPoolClientId,
                Password = password,
                Username = username,
                UserAttributes= attributeType

            };
            try
            {
                SignUpResponse signUpResult = await provider.SignUpAsync(signupReq);
                await provider.AdminConfirmSignUpAsync(req);
                Console.WriteLine(signUpResult);
                return signUpResult;
                
            } catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return null;
            }
        }
        public static async Task<Token> GetCredsAsync(string username, string password,IConfiguration config)
        {
            var UserPoolClientId = config["AWS:UserPoolClientId"];
            var UserPoolId = config["AWS:UserPoolId"];
            var AccessId = config["AWS:AccessId"];
            var AccessSecret = config["AWS:AccessSecret"];

            AmazonCognitoIdentityProviderClient provider =
                new AmazonCognitoIdentityProviderClient(new AnonymousAWSCredentials(), RegionEndpoint.APSoutheast1);
            CognitoUserPool userPool = new CognitoUserPool(UserPoolId, UserPoolClientId, provider);
            CognitoUser user = new CognitoUser(username, UserPoolClientId, userPool, provider);
            InitiateSrpAuthRequest authRequest = new InitiateSrpAuthRequest()
            {
                Password = password
            };

            AuthFlowResponse authResponse = await user.StartWithSrpAuthAsync(authRequest).ConfigureAwait(false);
            var accessToken = authResponse.AuthenticationResult.AccessToken;
            var refreshToken = authResponse.AuthenticationResult.RefreshToken;
            Token token = new Token
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
            };
            Console.WriteLine(accessToken);
            return token;
        }

        public static async void SetPasswordAsync(string username, string newPassword,IConfiguration config)
        {
            var UserPoolClientId = config["AWS:UserPoolClientId"];
            var UserPoolId = config["AWS:UserPoolId"];
            var AccessId = config["AWS:AccessId"];
            var AccessSecret = config["AWS:AccessSecret"];

            var credentials = new BasicAWSCredentials(AccessId, AccessSecret);
            var provider = new AmazonCognitoIdentityProviderClient(credentials, RegionEndpoint.APSoutheast1);
            AdminSetUserPasswordRequest passwordResetReq = new AdminSetUserPasswordRequest()
            {
                Password = newPassword,
                Permanent = true,
                Username = username,
                UserPoolId = UserPoolId
            };
            await provider.AdminSetUserPasswordAsync(passwordResetReq).ConfigureAwait(true);
        }


       

        [HttpPost("signup")]
        public RegistRespond Signup([FromBody] Userdata userdata)
        {
            try
            {
                RegisUserAsync(userdata.Username, userdata.Password,_config);
                return new RegistRespond
                {
                    status = "success",
                    message = "OK"
                };
            }catch (Exception ex)
            {
                return new RegistRespond
                {
                    status = "success",
                    message = ex.ToString()
                };
            }
            
        }
        

        [HttpPost("signin")]
        public async Task<Token> Signin([FromBody] Userdata userdata)
        {
            var token = await GetCredsAsync(userdata.Username, userdata.Password,_config);
            return token;
        }

        [HttpPost("setPassword")]
        public async void SetPassword([FromBody] Userdata userdata)
        {
            SetPasswordAsync(userdata.Username, userdata.Password,_config);
        }


    }
}