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

        public FatTreeCluster(int hosts = 54, int portsOnRouter = 6)
        {
            Extensions.InitListOfNodes(this, nameof(RackNodes), hosts);
            var i = 0;
            var lan = RackNodes.ToDictionary(place => (i++).ToString(), place => place);
            Tree = new FatTree(this, lan, Fraction.One, portsOnRouter);
            RegisterPattern(Tree);
        }
    }
}
