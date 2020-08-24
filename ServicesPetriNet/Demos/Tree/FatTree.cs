using System;
using System.Collections.Generic;
using System.Linq;
using Fractions;
using ServicesPetriNet.Core;
using ServicesPetriNet.Core.Transitions;
using ServicesPetriNet.Examples;

namespace ServicesPetriNet
{
    public class Package : MarkType
    {
        public int Size, Number;
    }
    public class NetworkChannel : Pattern
    {
        private Place Channel;
        public Place NetworkFrom, NetworkTo;

        private Transition Send, Receive;

        public NetworkChannel(Group ctx) : base(ctx)
        {
            RegisterNode(nameof(Send));
            RegisterNode(nameof(Receive));
            RegisterNode(nameof(Channel));
            Send.Action<OneToOne<Package>>()
                .In<Package>(NetworkFrom)
                .Out<Package>(Channel);
            Receive.Action<OneToOne<Package>>()
                .In<Package>(Channel)
                .Out<Package>(NetworkTo);
        }
    }

    public class Converter<T> : IAction
        where T : MarkType, new()
    {
        public Type From => typeof(T);

        public virtual List<Package> Action(T input)
        {
            var result = new List<Package>() {
                new Package() {
                    Number = 0,
                    Size = 1
                }
            };
            Host.From.Decompose(input, result.AsParts());
            return result;
        }

        public Transition Host { get; set; }
    }
    

    public class FatTree : Pattern
    {
        private List<Transition> ToPackageTransitions;

        private List<Place> Routers;
        private List<NetworkChannel> Links;

        public Dictionary<string, Place> Endpoints;
        public List<Type> ToPackageConverters;

        public FatTree(Group ctx, Dictionary<string, Place> endpoints, Fraction convertersSpeed, int Kport = 4, params Type[] toPackageConverters) : base(ctx)
        {
            int C = Kport / 2;
            int serversCount = C * C;
            int totalSwitches = 2 * C + C * C;
                
            Endpoints = endpoints;
            RegisterList(nameof(Routers), totalSwitches);

            for (int i = 0; i < Endpoints.Count; i++) {
                
            }
        }
    }
}
