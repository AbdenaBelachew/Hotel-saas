namespace PMS.Domain.Entities;

public class UserProperty
{
    public Guid UserId { get; set; }
    public Guid PropertyId { get; set; }

    public User User { get; set; } = null!;
    public Property Property { get; set; } = null!;
}
