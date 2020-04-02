using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using ServicesPetriNet.Core;
using ServicesPetriNet.Core.Transitions;
using static ServicesPetriNet.Extensions;

namespace ServicesPetriNet.Examples
{
    public class SimpleTwoHostNetwork : Group
    {
        public Place ServiceA, ServiceB;

        public Transition DecomposeA, ComposeB, DecomposeB, ComposeA;

        public SimpleNetwork FromA, FromB;

        public SimpleTwoHostNetwork()
        {
            DecomposeA.Action<Decompose>()
                .In<Message>(ServiceA)
                .Out<List<SimpleNetwork.Package>>(FromA.NetworkFrom);
            ComposeB.Action<Compose>()
                .In<List<SimpleNetwork.Package>>(FromA.NetworkTo)
                .Out<Message>(ServiceB);
            DecomposeB.Action<Decompose>()
                .In<Message>(ServiceB)
                .Out<List<SimpleNetwork.Package>>(FromB.NetworkFrom);
            ComposeA.Action<Compose>()
                .In<List<SimpleNetwork.Package>>(FromB.NetworkTo)
                .Out<Message>(ServiceA);

            GroupDescriptor = new GroupDescriptor<SimpleTwoHostNetwork>(this) {
                Marks = At(ServiceA, MarkType.Create<Message>(128))
                    .At(ServiceB, MarkType.Create<Message>(256))
                    .At(ServiceB, MarkType.Create<Message>(128))
            };
        }

        public class Message : MarkType
        {
            public int Length;
        }

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

        public class Decompose
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


        public class SimpleNetwork : Group
        {
            public Place NetworkFrom, NetworkTo;
            private Place Channel;

            private Transition Send, Receive;

            public SimpleNetwork()
            {
                Send.Action<OneToOne<Package>>()
                    .In<Package>(NetworkFrom)
                    .Out<Package>(Channel);
                Receive.Action<OneToOne<Package>>()
                    .In<Package>(Channel)
                    .Out<Package>(NetworkTo);

                GroupDescriptor = new GroupDescriptor<SimpleNetwork>(this);
            }

            public class Package : MarkType
            {
                public int Size, Number;
            }
        }
    }
}
