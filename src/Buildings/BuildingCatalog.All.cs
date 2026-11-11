using System.Collections.Generic;
using System.Linq;
using Assets.Behaviour.Flow;
using Assets.Source.Flow;
using Assets.Source.Player;
using KingdomFoundation.Api;

namespace KingdomHotkeyCustomizer
{
    internal static partial class BuildingCatalog
    {
        internal static List<FlowElement> All()
        {
            if (FoundationApi.World.Player != null)
                return FoundationApi.Machines.Catalog.Where(e => e.RequiredTech != null).ToList();

            var previous = GamePlayer.Current;
            try
            {
                GamePlayer.Current = new GamePlayer();
                GamePlayer.Current.Flows.Add(new FlowDiagram());
                return FlowPrefabSet.Ordered.Select(p => FlowElement.Create(p.Identifier))
                    .Where(e => e.RequiredTech != null).ToList();
            }
            finally { GamePlayer.Current = previous; }
        }
    }
}
