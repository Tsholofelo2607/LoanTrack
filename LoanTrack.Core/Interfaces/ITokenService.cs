using LoanTrack.Core.Entities;

namespace LoanTrack.Core.Interfaces;

public interface ITokenService
{
    string GenerateAccessToken(User user, IList<string> roles);
    string GenerateRefreshToken();
}