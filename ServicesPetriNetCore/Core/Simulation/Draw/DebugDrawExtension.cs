using System;
using System.Collections.Generic;
using System.Linq;
using ServicesPetriNet;
using ServicesPetriNet.Core;

namespace ServicesPetriNetCore.Core.Simulation.Draw
{
    public static class DebugDrawExtension
    {
        public static string DebugGraphToDot<T>(this SimulationControllerBase<T> simulation, Type[] filterOut = null)
            where T : Group
        {
            if (filterOut == null) filterOut = new Type[] { };
            var s = @"digraph G {
                    rankdir=LR;
                    graph[K=1. sep=31 overlap=false outputorder=edgesfirst ranksep=5 concentrate=true];
                    edge[arrowhead=vee, arrowtail=inv, arrowsize=.7,  fontsize=10, color=darkgray]";

            //Gather
            var gu = new UIGraph();
            Action<Group> toDot = null;
            toDot = group =>
            {
                s += "subgraph place {\n" +
                     "node [shape=circle, style=filled, fixedsize=true, fillcolor=white, border=black, fontcolor=gray, label=\" \",height=.3,width=.3];\n";

                Func<Extensions.NodeDetails, UIGraphNode> getNode = d =>
                {
                    var forbidden = filterOut.Any(type => d.Host.From.GetType().IsAssignableFrom(type));
                    if (d.IsPatternMember)
                        forbidden = filterOut.Any(type => d.PatternSource.GetType().IsAssignableFrom(type));

                    var n = new UIGraphNode
                    {
                        src = d
                    };
                    if (!forbidden) n.remove = false;
                    else n.remove = true;

                    return n;
                };

                foreach (var fieldDescriptor in group.Descriptor.Places.Values)
                {
                    var d = fieldDescriptor.Value.DebugSource(group);
                    var n = getNode(d);
                    if (!n.remove) s += d + " [xlabel = \"" + d.Name + "\"];\n";
                    gu.Nodes.Add(n, new List<UIGraphNode>());
                }

                s += "}\n";

                s += "subgraph transitions  {\n" +
                     "node [shape=rect,height=.4,width=.1,label=\"\",fillcolor=black, style=filled];\n";


                foreach (var fieldDescriptor in group.Descriptor.Transitions.Values)
                {
                    var d = fieldDescriptor.Value.DebugSource(group);
                    var n = getNode(d);
                    if (!n.remove) s += d + " [xlabel = \"" + d.Name + "\"];\n";
                    gu.Nodes.Add(n, new List<UIGraphNode>());
                }

                s += "}\n";

                foreach (var fieldDescriptor in group.Descriptor.Transitions.Values)
                    fieldDescriptor.Value.Links.ForEach(
                        link =>
                        {
                            var from = link.From.DebugSource(group);
                            var to = link.To.DebugSource(group);
                            var ff = getNode(from);
                            var tto = getNode(to);
                            gu.Nodes[ff].Add(tto);
                        }
                    );
                group.Descriptor.ApplyToAllSubGroups(descriptor => toDot(descriptor.Value));
            };
            toDot(simulation.TopGroup);

            //Map filter
            var ngu = new UIGraph();
            Action<UIGraphNode, UIGraphNode, List<UIGraphNode>> act = null;
            act = (source, current, visited) =>
            {
                gu.Nodes[current].ForEach(
                    node =>
                    {
                        if (visited.Contains(node))
                        {
                            return;
                        }
                        visited.Add(node);
                        if (node.remove)
                        {
                            act(source, node, visited);
                        }
                        else
                        {
                            if (!ngu.Nodes.ContainsKey(source)) ngu.Nodes.Add(source, new List<UIGraphNode>());
                            ngu.Nodes[source].Add(node);
                        }
                    }
                );
            };
            foreach (var kn in gu.Nodes.Keys.Where((node, i) => !node.remove)) act(kn, kn, new List<UIGraphNode>());

            //Reduce
            var ngu2 = new UIGraph();
            foreach (var kvp in ngu.Nodes)
            {
                ngu2.Nodes.Add(kvp.Key, kvp.Value.Distinct().ToList());
            }

            //Print
            foreach (var kvp in ngu2.Nodes)
                kvp.Value.ForEach(
                    node =>
                    {
                        s += kvp.Key.src + " -> " +
                             node.src + ";\n";
                    }
                );

            s += "}";
            return s;
        }
    }
}
