using System.Collections.Generic;
using ServicesPetriNet;

namespace ServicesPetriNetCore.Core.Simulation.Draw
{
    public class UIGraph
    {
        public readonly Dictionary<UIGraphNode, List<UIGraphNode>> Nodes = new Dictionary<UIGraphNode, List<UIGraphNode>>();
    }
}
