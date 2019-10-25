namespace SIO.Identity.Verify.Response
{
    public class VerifyResponse
    {
        public string Email { get; set; }
        public string Token { get; set; }

        public VerifyResponse(string email, string token)
        {
            Email = email;
            Token = token;
        }
    }
}
