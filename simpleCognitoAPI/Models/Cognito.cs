namespace simpleCognitoAPI
{ 
    public class Token
    {
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
    }

    public class Userdata
    {
        public string? Username { get; set; }
        public string? Password { get; set; }
    }
    public class RegistRespond
    {
        public string? status { get; set; } 
        public string? message { get; set; }
    }
}