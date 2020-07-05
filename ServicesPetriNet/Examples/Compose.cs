using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using ServicesPetriNet.Core;

namespace ServicesPetriNet.Examples
{
    public class Compose
    {
        [UsedImplicitly]
        public Message Action(List<SimpleNetwork.Package> ps)
        {
            var potential = ps.GroupBy(package => (Message) package.Parent)
                .ToDictionary(packages => packages.Key, packages => packages.ToList());
            var msg = potential.First();
            var (r, m) = msg.Key.Combine(msg.Value.Cast<IPart>().ToList());
            if (r) {
                var result = (Message) m;
                result.Length = msg.Value.Max(package => package.Number);
                return result;
            }

            return null;
        }
    }
}
