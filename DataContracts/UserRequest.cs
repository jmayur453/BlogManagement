namespace BlogManagement.DataContracts
{
    public class LoginUserRequest
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
    }
    public class RegisterUserRequest
    {
        public required string FullName { get; set; }
        public required string MobileNo { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
    }
}
