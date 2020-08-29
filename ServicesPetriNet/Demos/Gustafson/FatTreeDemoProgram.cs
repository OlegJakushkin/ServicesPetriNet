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

            public FatTreeCluster(int hosts = 8, int portsOnRouter=4)
            {
                Extensions.InitListOfNodes(this, nameof(RackNodes), hosts);
                var i = 0;
                var lan = RackNodes.ToDictionary(place => (i++).ToString(), place => place);
                Tree = new FatTree(this, lan, Fraction.One, portsOnRouter);
                RegisterPattern(Tree);
            }
        }


        public static void Main()
        {
            var simulation = new SimulationControllerBase<FatTreeCluster>(generator: () => new FatTreeCluster());
            simulation.SimulationStep();

            var s = "digraph G {\n";

            Action<Group> toDot = null;
            toDot = group =>
            {
                s += "subgraph place {\n" +
                     "node [shape=circle,fixedsize=true,label=\" \",height=.3,width=.3];\n";
                foreach (var fieldDescriptor in @group.Descriptor.Places.Values) {
                    s += fieldDescriptor.Value.DebugSource(group) + ";\n";
                }

                s += "}\n";

                s += "subgraph transitions  {\n" +
                     "node [shape=rect,height=.4,width=.2,label=\" \",fillcolor=black, style=filled];\n";

                foreach (var fieldDescriptor in @group.Descriptor.Transitions.Values) {
                    s+= fieldDescriptor.Value.DebugSource(group) + ";\n";
                }

                s += "}\n";

                foreach (var fieldDescriptor in @group.Descriptor.Transitions.Values) {
                   fieldDescriptor.Value.Links.ForEach(
                        link =>
                        {
                            s += link.To.DebugSource(group) + " -> " +
                                 link.From.DebugSource(group) + ";\n"; 
                        }
                    );
                }
                group.Descriptor.ApplyToAllSubGroups(descriptor => toDot(descriptor.Value));
            };
            toDot(simulation.TopGroup);
            s += "}";
            Console.Write(s);
            File.WriteAllText("./fattree.dot", s);
            Console.ReadLine();
        }
    }
}
