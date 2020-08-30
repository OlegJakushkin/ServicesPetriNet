using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Fractions;
using ScottPlot;
using ServicesPetriNet.Core;

namespace ServicesPetriNet
{
    public class FatTreeDemoProgram
    {
        public class FatTreeCluster : Group
        {
            public List<Place> RackNodes;
            public FatTree Tree;

            public FatTreeCluster(int hosts = 54, int portsOnRouter=6)
            {
                Extensions.InitListOfNodes(this, nameof(RackNodes), hosts);
                var i = 0;
                var lan = RackNodes.ToDictionary(place => (i++).ToString(), place => place);
                Tree = new FatTree(this, lan, Fraction.One, portsOnRouter);
                RegisterPattern(Tree);
            }
        }

        public class UIGraphNode : IEquatable<UIGraphNode>
        {
            public object src;
            public bool remove;

            public bool Equals(UIGraphNode other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return Equals(src, other.src);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((UIGraphNode) obj);
            }

            public override int GetHashCode()
            {
                return (src != null ? src.GetHashCode() : 0);
            }
        }
        public class UIGraph
        {
            public readonly Dictionary<UIGraphNode, List<UIGraphNode>> Nodes = new Dictionary<UIGraphNode, List<UIGraphNode>>();
        }

        public static void Main()
        {
            var simulation = new SimulationControllerBase<FatTreeCluster>(generator: () => new FatTreeCluster());
            simulation.SimulationStep();

            var s = "digraph G {\n";

            var filterOut = new Type[] {
                typeof(NetworkChannel)
            };
            var gu = new UIGraph();
            Action<Group> toDot = null;
            toDot = group =>
            {
                s += "subgraph place {\n" +
                     "node [shape=circle,fixedsize=true,label=\" \",height=.3,width=.3];\n";

                Func<Extensions.NodeDetails, UIGraphNode> getNode = d =>
                {
                    var forbidden = filterOut.Any(type =>  d.Host.From.GetType().IsAssignableFrom(type));
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

                foreach (var fieldDescriptor in @group.Descriptor.Places.Values) {
                    var d = fieldDescriptor.Value.DebugSource(group);
                    var n = getNode(d);
                    if (!n.remove) {
                        s += d + " [xlabel = \"" + d.Name + "\"];\n";
                    }
                    gu.Nodes.Add(n, new List<UIGraphNode>());

                }

                s += "}\n";

                s += "subgraph transitions  {\n" +
                     "node [shape=rect,height=.4,width=.1,label=\"\",fillcolor=black, style=filled];\n";


                foreach (var fieldDescriptor in @group.Descriptor.Transitions.Values) {
                    var d = fieldDescriptor.Value.DebugSource(group);
                    var n = getNode(d);
                    if (!n.remove) {
                        s += d + " [xlabel = \"" + d.Name + "\"];\n";
                    }
                    gu.Nodes.Add(n, new List<UIGraphNode>());
                }

                s += "}\n";

                foreach (var fieldDescriptor in @group.Descriptor.Transitions.Values) {
                   fieldDescriptor.Value.Links.ForEach(
                        link =>
                        {
                            var from = link.To.DebugSource(group);
                            var to = link.From.DebugSource(group);
                            var ff =getNode(from);
                            var tto =getNode(to);
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
            foreach (var kn in gu.Nodes.Keys.Where((node, i) => !node.remove)) {

                act(kn, kn);
            }

            foreach (var kvp in ngu.Nodes) {
                kvp.Value.ForEach(
                    node =>
                    {
                        s += kvp.Key.src + " -> " +
                             node.src + ";\n";
                    });
     
            }

            s += "}";
            Console.Write(s);
            File.WriteAllText("./fattree.dot", s);
            Console.ReadLine();
        }
    }
}
