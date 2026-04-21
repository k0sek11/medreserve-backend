using Microsoft.AspNetCore.Identity;

namespace Medreserve.Features.User;

public class User : IdentityUser
{
    public int TestField { get; set; }
}
