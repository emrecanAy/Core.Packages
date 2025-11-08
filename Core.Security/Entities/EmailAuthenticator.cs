using Core.Persistence.Repositories;

namespace Core.Security.Entities;
public class EmailAuthenticator : Entity<int>
{
    public EmailAuthenticator() { }
    public EmailAuthenticator(int userId, bool ısVerified)
    {
        UserId = userId;
        IsVerified = ısVerified;
    }
    public EmailAuthenticator(int id, int userId, bool ısVerified) : base(id)
    {
        UserId = userId;
        IsVerified = ısVerified;
    }

    public int UserId { get; set; }
    public bool IsVerified { get; set; }
    public string? ActivationKey { get; set; }

    //Relations
    public virtual User User { get; set; } = null!;
}
