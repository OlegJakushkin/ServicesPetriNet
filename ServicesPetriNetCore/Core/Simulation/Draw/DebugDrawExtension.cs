using System;
using System.Collections.Generic;
using System.Linq;
using ServicesPetriNet;
using ServicesPetriNet.Core;

namespace ServicesPetriNetCore.Core.Simulation.Draw
{
    public static class DebugDrawExtension
    {
        public static string DebugGraphToDot<T>(this SimulationControllerBase<T> simulation, Type[] filterOut = null) where T : Group
        {
            if (filterOut == null) {
                filterOut = new Type[] { };
            }
            var s = "digraph G {\n";
            var gu = new UIGraph();
            Action<Group> toDot = null;
            toDot = group =>
            {
                s += "subgraph place {\n" +
                     "node [shape=circle,fixedsize=true,label=\" \",height=.3,width=.3];\n";

                Func<Extensions.NodeDetails, UIGraphNode> getNode = d =>
                {
                    var forbidden = filterOut.Any(type => d.Host.From.GetType().IsAssignableFrom(type));
                    if (d.IsPatternMember)
                    {
                        forbidden = filterOut.Any(type => d.PatternSource.GetType().IsAssignableFrom(type));
                    }

                    var n = new UIGraphNode()
                    {
                        src = d
                    };
                    if (!forbidden)
                    {
                        n.remove = false;
                    }
                    else
                    {
                        n.remove = true;
                    }

                    return n;
                };

                foreach (var fieldDescriptor in @group.Descriptor.Places.Values)
                {
                    var d = fieldDescriptor.Value.DebugSource(group);
                    var n = getNode(d);
                    if (!n.remove)
                    {
                        s += d + " [xlabel = \"" + d.Name + "\"];\n";
                    }
                    gu.Nodes.Add(n, new List<UIGraphNode>());

                }

                s += "}\n";

                s += "subgraph transitions  {\n" +
                     "node [shape=rect,height=.4,width=.1,label=\"\",fillcolor=black, style=filled];\n";


                foreach (var fieldDescriptor in @group.Descriptor.Transitions.Values)
                {
                    var d = fieldDescriptor.Value.DebugSource(group);
                    var n = getNode(d);
                    if (!n.remove)
                    {
                        s += d + " [xlabel = \"" + d.Name + "\"];\n";
                    }
                    gu.Nodes.Add(n, new List<UIGraphNode>());
                }

                s += "}\n";

                foreach (var fieldDescriptor in @group.Descriptor.Transitions.Values)
                {
                    fieldDescriptor.Value.Links.ForEach(
                        link =>
                        {
                            var from = link.To.DebugSource(group);
                            var to = link.From.DebugSource(group);
                            var ff = getNode(from);
                            var tto = getNode(to);
                            gu.Nodes[ff].Add(tto);
                        }
                    );
                }
                group.Descriptor.ApplyToAllSubGroups(descriptor => toDot(descriptor.Value));
            };
            toDot(simulation.TopGroup);

            var ngu = new UIGraph();

            Action<UIGraphNode, UIGraphNode> act = null;
            act = (source, current) =>
            {
                gu.Nodes[current].ForEach(
                    node =>
                    {
                        if (node.remove)
                        {
                            act(source, node);
                        }
                        else
                        {
                            if (!ngu.Nodes.ContainsKey(source))
                            {
                                ngu.Nodes.Add(source, new List<UIGraphNode>());
                            }
                            ngu.Nodes[source].Add(node);
                        }
                    }
                );
            };
            foreach (var kn in gu.Nodes.Keys.Where((node, i) => !node.remove))
            {

                act(kn, kn);
            }

            foreach (var kvp in ngu.Nodes)
            {
                kvp.Value.ForEach(
                    node =>
                    {
                        s += kvp.Key.src + " -> " +
                             node.src + ";\n";
                    });

            }

            s += "}";
            return s;
        }
    }
}
