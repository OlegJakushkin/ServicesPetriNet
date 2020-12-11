using Dynamitey.DynamicObjects;
using ServicesPetriNet.Core;
using ServicesPetriNet.Core.Transitions;

namespace ServicesPetriNet
{

    public class NetworkChannel : Pattern
    {
        private Place ChannelS, ChannelR;
        public Place NetworkFrom, NetworkTo;

        private Transition SendT, SendR, ReceiveT, ReceiveR;

        public NetworkChannel(Group ctx, Place networkFrom, Place networkTo) : base(ctx)
        {
            NetworkFrom = networkFrom;
            NetworkTo = networkTo;

            RegisterNode(nameof(SendT));
            RegisterNode(nameof(SendR));
            RegisterNode(nameof(ReceiveT));
            RegisterNode(nameof(ReceiveR));
            RegisterNode(nameof(ChannelS));
            RegisterNode(nameof(ChannelR));

            SendT.Action<OneToOne<Package>>()
                .In<Package>(NetworkFrom)
                .Out<Package>(ChannelS);
            SendR.Action<OneToOne<Package>>()
                .In<Package>(ChannelS)
                .Out<Package>(NetworkTo);

            ReceiveT.Action<OneToOne<Package>>()
                .In<Package>(NetworkTo)
                .Out<Package>(ChannelR);
            ReceiveR.Action<OneToOne<Package>>()
                .In<Package>(ChannelR)
                .Out<Package>(NetworkFrom);
        }
    }



}