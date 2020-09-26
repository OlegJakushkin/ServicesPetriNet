﻿using System;
using System.Collections.Generic;
using System.Linq;
using Fractions;
using ServicesPetriNet.Core;

namespace ServicesPetriNet
{


    public class FatTree : Pattern
    {
        public Dictionary<string, Place> Endpoints;
        private readonly List<NetworkChannel> Links = new List<NetworkChannel>();
        private  List<Transition> Converters;
        private List<Place> Routers;
        public List<Type> ToPackageConverters;
        private List<Transition> ToPackageTransitions;

        public FatTree(Group ctx, Dictionary<string, Place> endpoints, Fraction convertersSpeed, int Kport = 6,
            Dictionary<Type, Type> converters = null) : base(ctx)
        {
            if (converters != null)
            {
                RegisterList(nameof(Converters), endpoints.Count * converters.Count);

                var w = 0;
                foreach (var endpoint in endpoints)
                {
                    foreach (var converter in converters)
                    {
                        Converters[w].Action(converter.Key)
                            .In(converter.Value, endpoint.Value)
                            .Out<Package>(endpoint.Value, Link.Count.All);
                        w++;
                    }
                }
            }

            var C = Kport / 2;
            var H = endpoints.Count / 2;
            var D = Math.Log(Convert.ToDouble(H), Convert.ToDouble(C));
            var L = Math.Pow(C, D - 1);

            var totallSwitches = (2 * D - 1) * L;
            Endpoints = endpoints;
            RegisterList(nameof(Routers), Convert.ToInt32(totallSwitches));
            var ts = 0;

            //Leaves
            var max = Endpoints.Count / C;
            for (var i = 0; i < max; i++) {
                var re = Routers[ts++];

                foreach (var ne in Endpoints.Values.Skip(C * i).Take(C)) {
                    var l = new NetworkChannel(ctx, ne, re);
                    Links.Add(l);
                    RegisterPattern(l);
                }
            }

            //Trunk
            var range = C;
            var step = 1;
            var current = ts;
            var layer = Convert.ToInt32(L * 2);

            while (current + layer < Routers.Count) {
                for (var i = current; i < current + layer; i++) {
                    var re = Routers[i];
                    var k = i - layer;
                    var mul = k / range * range;
                    var r = k % range;
                    for (var j = 0; j < range; j += step) {
                        var ne = Routers[mul + j + r];
                        var l = new NetworkChannel(ctx, ne, re);
                        Links.Add(l);
                        RegisterPattern(l);
                    }
                }

                step *= range;
                range *= range;
                current = current + layer;
            }

            //Root
            for (var i = current; i < current + layer / 2; i++) {
                var re = Routers[i];
                var k = i - layer;
                var mul = k / range * range;
                for (var j = current - layer + i % step; j < current; j += step) {
                    var ne = Routers[j];
                    var l = new NetworkChannel(ctx, ne, re);
                    Links.Add(l);
                    RegisterPattern(l);
                }
            }
        }
    }
}
