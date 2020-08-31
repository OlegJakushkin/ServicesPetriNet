using System.Collections.Generic;

namespace ServicesPetriNetCore.Core.Simulation.Draw
{
    public class UIGraph
    {
        public readonly Dictionary<UIGraphNode, List<UIGraphNode>> Nodes =
            new Dictionary<UIGraphNode, List<UIGraphNode>>();
    }
}
