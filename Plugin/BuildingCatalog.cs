using System.Collections.Generic;
using System.Linq;
using Assets.Behaviour.Flow;
using Assets.Source.Flow;
using Assets.Source.Player;

namespace KingdomHotkeyCustomizer
{
    internal static class BuildingCatalog
    {
        internal static List<FlowElement> All()
        {
            if (GamePlayer.Current != null) return FlowElement.GetAllTypes().Where(e => e.RequiredTech != null).ToList();
            // Some native constructors consult Current even for previews. In the
            // main menu, provide a temporary in-memory context synchronously and
            // restore null before returning. Do not populate the game's preview
            // cache with this context, load a kingdom, or write a save.
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
