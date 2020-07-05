using ServicesPetriNet.Core;
using ServicesPetriNet.Core.Transitions;

namespace ServicesPetriNet.Examples
{
    public class SimpleNetwork : Group<SimpleNetwork>
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
