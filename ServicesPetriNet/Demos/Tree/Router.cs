using System.Collections.Generic;
using JetBrains.Annotations;
using ServicesPetriNet.Core;

namespace ServicesPetriNet
{
    public class Router : Pattern
    {
        private Place Buffer;
        private Transition Routing;

        public Router(Group ctx, List<NetworkChannel> links /*, Tree Topology*/ ) : base(ctx)
        {
            RegisterNode(nameof(Routing));
            RegisterNode(nameof(Buffer));

            //Paths to solve:
            //Static routing tables - OSPF
            // Return link id from action
        }

    }
    
    /*
    public class Add : ActionBase
    {
        // In ActionBase
        public object Ctx;

        [UsedImplicitly]
        public List<Package> Action(Package p)
        {
            // Ctx -> Tree Topology
            // to = ruting(destination)
            // retun to //  List{0,0,k,0}
        }
    }
    */
}