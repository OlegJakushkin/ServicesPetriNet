using System;
using System.Collections.Generic;
using ServicesPetriNet.Core;

namespace ServicesPetriNet
{
    public struct LinkKey : IEqualityComparer<LinkKey>
    {
        public Type MarkType;
        public string LinkName;

        public override bool Equals(object? obj)
        {
            var result = false;
            if (obj != null)
            {
                var other = (LinkKey)obj;
                result = (other.MarkType == MarkType && other.LinkName == LinkName) ;
            }

            return result;
        }

        public bool Equals(LinkKey x, LinkKey y) { return x.Equals(y); }

        public int GetHashCode(LinkKey obj) { return MarkType.GetHashCode() + LinkName.GetHashCode(); }
    }
}