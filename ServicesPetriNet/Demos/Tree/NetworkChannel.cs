using Dynamitey.DynamicObjects;
using ServicesPetriNet.Core;
using ServicesPetriNet.Core.Transitions;

namespace ServicesPetriNet
{
    public class NetworkChannel : Pattern
    {
        private Place Channel;
        public Place NetworkFrom, NetworkTo;

        private Transition Send, Receive;

        public NetworkChannel(Group ctx, Place networkTo) : base(ctx)
        {
            NetworkTo = networkTo;

            RegisterNode(nameof(NetworkFrom));
            RegisterNode(nameof(Channel));
            RegisterNode(nameof(Send));
            RegisterNode(nameof(Receive));

            Send.Action<OneToOne<Package>>()
                .In<Package>(NetworkFrom)
                .Out<Package>(Channel);
            Receive.Action<OneToOne<Package>>()
                .In<Package>(Channel)
                .Out<Package>(NetworkTo);
        }
    }
}