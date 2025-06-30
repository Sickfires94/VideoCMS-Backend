namespace Backend.Services.Interfaces
{
    public interface ITokenClaimsAccessor
    {
        string? getLoggedInUserName();
        string? getLoggedInUserEmail();
        int? getLoggedInUserId();
    }
}
