using System.Collections.Generic;
using System.Linq;
using KingdomFoundation.Api;

namespace KingdomHotkeyCustomizer
{
    internal static class BuildingCatalog
    {
        internal static List<WorkspaceObject> All()
        {
            return FoundationApi.WorkspaceAccess.DiscoverCatalog().Where(element => element.RequiredResearch != null).ToList();
        }
    }
}
