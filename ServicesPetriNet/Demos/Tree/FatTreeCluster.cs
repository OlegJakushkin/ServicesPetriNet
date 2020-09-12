using System;
using System.Collections.Generic;
using System.Linq;
using Fractions;
using ServicesPetriNet.Core;

namespace ServicesPetriNet
{
    public class FatTreeCluster : Group
    {
        public List<Place> RackNodes;
        public FatTree Tree;

        public class Mark : MarkType
        {
            public string To;
        }

        public class Convert : Converter<Mark>
        {
            public override List<Package> Action(Mark input)
            {
                var pkgs = base.Action(input);
                pkgs.ForEach(package => package.To = input.To);
                return pkgs;
            }
        }

        public FatTreeCluster(int hosts = 54, int portsOnRouter = 6)
        {
            Extensions.InitListOfNodes(this, nameof(RackNodes), hosts);
            
            
            var i = 0;
            var lan = RackNodes.ToDictionary(place => (i++).ToString(), place => place);

            var to = lan.Last();
            Marks = Extensions.At(lan.First().Value, MarkType.Create<Mark>(to.Key));
            var Converters = new Dictionary<Type, Type>();
            Converters.Add(typeof(Convert), typeof(Mark));

            Tree = new FatTree(this, lan, Fraction.One, portsOnRouter, Converters);
            RegisterPattern(Tree);
        }
    }
}
