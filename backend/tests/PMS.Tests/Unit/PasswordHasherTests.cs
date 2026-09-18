using PMS.Infrastructure.Services;

namespace PMS.Tests.Unit;

public class PasswordHasherTests
{
    private readonly PasswordHasher _hasher = new();

    [Fact]
    public void Hash_ThenVerify_WithCorrectPassword_Succeeds()
    {
        var hash = _hasher.Hash("Sup3rSecret!");

        Assert.True(_hasher.Verify(hash, "Sup3rSecret!"));
    }

    [Fact]
    public void Verify_WithWrongPassword_Fails()
    {
        var hash = _hasher.Hash("Sup3rSecret!");

        Assert.False(_hasher.Verify(hash, "WrongPassword!"));
    }

    [Fact]
    public void Hash_ProducesDifferentOutput_ForSameInput()
    {
        var hash1 = _hasher.Hash("Sup3rSecret!");
        var hash2 = _hasher.Hash("Sup3rSecret!");

        Assert.NotEqual(hash1, hash2); // salted
    }
}
