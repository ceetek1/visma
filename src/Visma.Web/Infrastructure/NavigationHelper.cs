using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace Visma.Web.Infrastructure;

/// <summary>
/// Builds sidebar links that "light up" automatically as teammates add controllers.
/// If a controller/action doesn't exist yet (e.g. Person 2 hasn't pushed VisitorsController),
/// the link points to a friendly "Coming soon" page instead of a 404, so nobody has to
/// edit the layout when their feature lands.
/// </summary>
public class NavigationHelper(IActionDescriptorCollectionProvider actions)
{
    /// <summary>True if an MVC action with this controller + action name is registered.</summary>
    public bool Exists(string controller, string action = "Index") =>
        actions.ActionDescriptors.Items
               .OfType<ControllerActionDescriptor>()
               .Any(a => a.ControllerName.Equals(controller, StringComparison.OrdinalIgnoreCase) &&
                         a.ActionName.Equals(action, StringComparison.OrdinalIgnoreCase));
}
