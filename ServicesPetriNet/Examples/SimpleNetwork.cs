using ServicesPetriNet.Core;
using ServicesPetriNet.Core.Transitions;

namespace ServicesPetriNet.Examples
{
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
        }

        public class Package : MarkType
        {
            public int Size, Number;
        }
    }
}
