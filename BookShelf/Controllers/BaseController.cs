using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace BookShelf.Controllers;

public abstract class BaseController : Controller
{
    protected Guid GetCurrentUserId()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out Guid userId))
            {
                return userId;
            }
        }

        // Return empty GUID if user is not authenticated
        return Guid.Empty;
    }
}