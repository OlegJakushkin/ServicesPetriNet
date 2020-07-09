using System;
using System.Collections.Generic;
using ServicesPetriNet.Core;
using ServicesPetriNet.Examples;
using static ServicesPetriNet.Extensions;

namespace ServicesPetriNet
{
    /*
            ServiceA:Message -(0.5)-> NetworkA:Packages {
                var mtu = 1400
                var count = Message.Length / mtu
                return Packages{From = Message, Size = mtu, Count = count, Left = count}
            }

            NetworkA:Packages:ps -(0.5)-> ChannelA:Packet, NetworkA:Packages {
                var id = ps.Left--
                p = Packet{From = Packages, Size = Packages.mtu, Number = id}
                if(ps.Left == 0) 
                    return (p, ps)
                else 
                    return (p, null)
            }

            ChannelA:Packet:in -(p=0.5, t=0.01)-> ChannelB:Packet, ChannelA:Packet {
                if (coin())
                    return (null, in)
                else
                    return (Packet(in), null)
            }

             ChannelB:Packet:in:all -(0.5)-> NetworkB:Packages:all { // Map many into one or subsets into dicts

                var out = in.GroupBy(p=>p.From)
                .ToDictionary(g => g.Key, g.ToList())
                .Where(g => g.Key.Count > p.Value.Count)кцу4
                .Select(p => {
                    var totall = p.Key.Count
                    var seq = Enumerable.Range(0, totall).ToList();

                    var consistant = seq.All(n => p.Value.Any(part => part.Number == n));
                    if (consistant) {
                        return Packages(p.Key)
                    }
                    return null
                }).Where(p => p != null)
                .ToList();

                return out;

            }
*/

    // todo simple net
    // todo simple router https://www.youtube.com/watch?v=rYodcvhh7b8
    // 

    class Program
    {
        static void Main(string[] args)
        {
            var Simulation = new SimpleTwoHostNetwork();
            //Draw(Simulation);
            Run(Simulation, 100);
            Console.ReadLine();
        }

        public class Graph
        {
            public Dictionary<string, INode> Nodes = new Dictionary<string, INode>();
            public List<Link> Edges = new List<Link>();

        }

        private static void Draw<T>(T simulation)
        where T : Group
        {
            var groups = GetAllTypeInstancesBasedOn<T, Group>(simulation);
            groups.Add(nameof(simulation), simulation);
            foreach (var @group in groups) {

            }
        }

        private static void Run<T>(T simulation, int steps)
            where T : Group
        {
            var groups = GetAllGroups(simulation);
            groups.Add(nameof(simulation), simulation);
            foreach (var @group in groups)
            {
                Console.WriteLine(@group.Key);
            }
        }
    }
}
