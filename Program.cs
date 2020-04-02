﻿using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using ServicesPetriNet.Core;
using ServicesPetriNet.Core.Attributes;
using ServicesPetriNet.Core.Transitions;
using static ServicesPetriNet.Extensions;

namespace ServicesPetriNet
{
    /*
            ServiceA:Message -(0.5)-> NetworkA:Packages {
                var mtu = 1400
                var count = Message.Length / mtu
                return Packages{From = Message, Size = mtu, Count = count, Left = count}
            }

            NetworkA:Packages:ps -(0.5)-> ChannelA:Packet, NetworkA:Packages {
                var id = ps.Left--
                p = Packet{From = Packages, Size = Packages.mtu, Number = id}
                if(ps.Left == 0) 
                    return (p, ps)
                else 
                    return (p, null)
            }

            ChannelA:Packet:in -(p=0.5, t=0.01)-> ChannelB:Packet, ChannelA:Packet {
                if (coin())
                    return (null, in)
                else
                    return (Packet(in), null)
            }

             ChannelB:Packet:in:all -(0.5)-> NetworkB:Packages:all { // Map many into one or subsets into dicts

                var out = in.GroupBy(p=>p.From)
                .ToDictionary(g => g.Key, g.ToList())
                .Where(g => g.Key.Count > p.Value.Count)
                .Select(p => {
                    var totall = p.Key.Count
                    var seq = Enumerable.Range(0, totall).ToList();

                    var consistant = seq.All(n => p.Value.Any(part => part.Number == n));
                    if (consistant) {
                        return Packages(p.Key)
                    }
                    return null
                }).Where(p => p != null)
                .ToList();

                return out;

            }
*/

    // todo simple net
    // todo simple router https://www.youtube.com/watch?v=rYodcvhh7b8
    

    class Program
    {
        static void Main(string[] args) { }
    }
}
