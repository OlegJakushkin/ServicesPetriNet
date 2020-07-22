using ServicesPetriNet.Core;
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
                .Out<SimpleNetwork.Package>(FromA.NetworkFrom);
            ComposeB.Action<Compose>()
                .In<SimpleNetwork.Package>(FromA.NetworkTo, Link.Count.All)
                .Out<Message>(ServiceB);
            DecomposeB.Action<Decompose>()
                .In<Message>(ServiceB)
                .Out<SimpleNetwork.Package>(FromB.NetworkFrom, Link.Count.All);
            ComposeA.Action<Compose>()
                .In<SimpleNetwork.Package>(FromB.NetworkTo, Link.Count.All)
                .Out<Message>(ServiceA);

            Marks = At(ServiceA, MarkType.Create<Message>(128))
                    .At(ServiceB, MarkType.Create<Message>(256))
                    .At(ServiceB, MarkType.Create<Message>(128))
            ;
        }
    }
}
