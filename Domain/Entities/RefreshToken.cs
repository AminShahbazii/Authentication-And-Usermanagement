

namespace Domain.Entities
{
    public class RefreshToken
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Token { get; set; }
        public DateTime Expire { get; set; }
        public DateTime Created { get; set; }
        public DateTime? RevokeAt { get; set; }


        public string UserId { get; set; }
        public User User { get; set; }
    }
}
