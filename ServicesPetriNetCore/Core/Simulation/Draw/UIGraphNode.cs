using System;

namespace ServicesPetriNetCore.Core.Simulation.Draw
{
    public class UIGraphNode : IEquatable<UIGraphNode>
    {
        public object src;
        public bool remove;

        public bool Equals(UIGraphNode other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(src, other.src);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((UIGraphNode)obj);
        }

        public override int GetHashCode()
        {
            return (src != null ? src.GetHashCode() : 0);
        }
    }
}
