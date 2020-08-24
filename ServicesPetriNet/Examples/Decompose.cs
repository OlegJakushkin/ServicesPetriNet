using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace ServicesPetriNet.Examples
{
    public class Decompose : ActionBase
    {
        [UsedImplicitly]
        public List<SimpleNetwork.Package> Action(Message m)
        {
            var mtu = 1400;
            var p = new SimpleNetwork.Package {
                Size = mtu,
                Parent = m
            };
            return Enumerable.Repeat(p, m.Length / mtu).ToList();
        }
    }
}
