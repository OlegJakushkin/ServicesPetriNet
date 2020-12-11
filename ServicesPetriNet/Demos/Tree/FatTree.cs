using System;
using System.Collections.Generic;
using System.Linq;
using Fractions;
using ServicesPetriNet.Core;

namespace ServicesPetriNet
{
    
    public class FatTreeAlgorithm
    {
        public class Node
        {
            private static int globalIds = 0;
            private static int GetNextId()
            {
                 globalIds += 1;
                 return globalIds;
            }
            public static Dictionary<int, Node> Nodes = new Dictionary<int, Node>();

            public int id;
            public Node()
            {
                id = GetNextId();
                Nodes.Add(id, this);
            }
        }

        public class TLink: IEquatable<TLink>
        {

            public bool Equals(TLink other)
            {
                throw new NotImplementedException();
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((TLink) obj);
            }

            public override int GetHashCode()
            {
                throw new NotImplementedException();
            }
        }
        public class FatTree
        {
            public List<KeyValuePair<Node, Node>> Links = new List<KeyValuePair<Node, Node>>();
            public List<Node> CoreSwitchList = new List<Node>();
            public List<Node> AggSwitchList = new List<Node>();
            public List<Node> EdgeSwitchList= new List<Node>();
            public List<Node> HostList = new List<Node>();
            private int _iAggLayerSwitch;
            private int _iEdgeLayerSwitch;
            private int _iHost;
            private int _iCoreLayerSwitch;

            public FatTree(int k, int density)
            {
                var pod = k;
                _iCoreLayerSwitch = (int)Math.Pow((k / 2), 2);
                _iAggLayerSwitch = k * k / 2;
                _iEdgeLayerSwitch = k * k / 2;
                _iHost = _iEdgeLayerSwitch * density;

                addNodes(_iCoreLayerSwitch, 1, CoreSwitchList);
                addNodes(_iAggLayerSwitch, 2, AggSwitchList);
                addNodes(_iEdgeLayerSwitch, 3, EdgeSwitchList);
                addNodes(_iHost, 4, HostList);
                createLink(pod, density);
            }

            private void addNodes(int number, int level, List<Node> switch_list)
            {
                for (int x = 0; x < number; x++)
                {    
                    var node = new Node();
                    switch_list.Add(node);
                }

            }

            private void createLink(int pod, int density)
            {
                int end = pod / 2;
                for (int x = 0; x < end; x += _iAggLayerSwitch)
                {
                    for (int i = 0; i < end; i++)
                    {
                        for (int j = 0; j < end; j++)
                        {
                            var core_ind = i * end + j;
                            var agg_ind = x + i;
                            addLink(
                                CoreSwitchList[core_ind],
                                AggSwitchList[agg_ind]);

                        }

                    }
                }
                for (int x = 0; x < end; x += _iAggLayerSwitch)
                {
                    for (int i = 0; i < end; i++)
                    {
                        for (int j = 0; j < end; j++)
                        {
                            addLink(
                                AggSwitchList[x + i],
                                EdgeSwitchList[x + j]);
                        }

                    }
                }
                for (int x = 0; x < _iEdgeLayerSwitch; x += 1)
                {
                    for (int i = 0; i < density; i++)
                    {
                            addLink(
                                EdgeSwitchList[x],
                                HostList[density * x + i]);
                    }
                }
            }

            private void addLink(Node a, Node b)
            {
                Links.Add(new KeyValuePair<Node, Node>(a, b));
            }
        }
        public class Router : Node
        {
            // Const, known links
        }
        public class Edge : Node
        {
            // Only one link
        }

        List<Edge> Edges;
        List<Node> Routers;

        public FatTreeAlgorithm(int RouterLinksCount = 4, int EdgesCount = 16)
        {
            Edges = Enumerable.Range(1, EdgesCount)
                .Select(i=>new Edge())
                .ToList();

            var C = RouterLinksCount / 2; // No more than half links go to Edges
            var H = EdgesCount / 2; // Roters 
            var D = Math.Log(Convert.ToDouble(H), Convert.ToDouble(C)); 
            var L = Math.Pow(C, D - 1);

            var totallSwitches = (2 * D - 1) * L;
        }
    }

    public class FatTree : Pattern
    {
        public Dictionary<string, Place> Endpoints;
        private readonly List<NetworkChannel> Links = new List<NetworkChannel>();
        private  List<Transition> Converters;
        private List<Place> Routers;
        public List<Type> ToPackageConverters;
        private List<Transition> ToPackageTransitions;

        public FatTree(Group ctx, Dictionary<string, Place> endpoints, Fraction convertersSpeed, int Kport = 6,
            Dictionary<Type, Type> converters = null) : base(ctx)
        {
            if (converters != null)
            {
                RegisterList(nameof(Converters), endpoints.Count * converters.Count);

                var w = 0;
                foreach (var endpoint in endpoints)
                {
                    foreach (var converter in converters)
                    {
                        Converters[w].Action(converter.Key)
                            .In(converter.Value, endpoint.Value)
                            .Out<Package>(endpoint.Value, Link.Count.All);
                        w++;
                    }
                }
            }

            var C = Kport / 2;
            var H = endpoints.Count / 2;
            var D = Math.Log(Convert.ToDouble(H), Convert.ToDouble(C));
            var L = Math.Pow(C, D - 1);

            var totallSwitches = (2 * D - 1) * L;
            Endpoints = endpoints;
            RegisterList(nameof(Routers), Convert.ToInt32(totallSwitches));
            var ts = 0;

            //Leaves
            var max = Endpoints.Count / C;
            for (var i = 0; i < max; i++) {
                var re = Routers[ts++];

                foreach (var ne in Endpoints.Values.Skip(C * i).Take(C)) {
                    var l = new NetworkChannel(ctx, ne, re);
                    Links.Add(l);
                    RegisterPattern(l);
                }
            }

            //Trunk
            var range = C;
            var step = 1;
            var current = ts;
            var layer = Convert.ToInt32(L * 2);

            while (current + layer < Routers.Count) {
                for (var i = current; i < current + layer; i++) {
                    var re = Routers[i];
                    var k = i - layer;
                    var mul = k / range * range;
                    var r = k % range;
                    for (var j = 0; j < range; j += step) {
                        var ne = Routers[mul + j + r];
                        var l = new NetworkChannel(ctx, ne, re);
                        Links.Add(l);
                        RegisterPattern(l);
                    }
                }

                step *= range;
                range *= range;
                current = current + layer;
            }

            //Root
            for (var i = current; i < current + layer / 2; i++) {
                var re = Routers[i];
                var k = i - layer;
                var mul = k / range * range;
                for (var j = current - layer + i % step; j < current; j += step) {
                    var ne = Routers[j];
                    var l = new NetworkChannel(ctx, ne, re);
                    Links.Add(l);
                    RegisterPattern(l);
                }
            }
        }
    }
    
}
