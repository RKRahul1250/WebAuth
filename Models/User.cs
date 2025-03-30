namespace WebAuth.Models
{
    public class User
    {
        public required string Username { get; set; }
        public required string Salt { get; set; }
        public required string HashedPassword { get; set; }
    }
}