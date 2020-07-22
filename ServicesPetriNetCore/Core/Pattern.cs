using System.Collections.Generic;
using System.Linq;

namespace ServicesPetriNet.Core
{
    public  class Pattern
    {
        private List<KeyValuePair< string,  INode>> PatternNodes = new List<KeyValuePair<string, INode>>();

        public Pattern()
        {
            Extensions.InitAllTypeInstances<Place>(this);
            Extensions.InitAllTypeInstances<Transition>(this);
        }

        protected void Register(  string name)
        {
            var p =Extensions.InitSingleNode(this, name);
            PatternNodes.Add(new KeyValuePair<string, INode>(name, p));
        }

        public virtual void RefreshHostDescriptor(IGroupDescriptor descriptor)
        {
            //TODO
            PatternNodes.Where(p=> p.Value is Place).
                ToList()
                .ForEach(p=> descriptor.Places.Add(p.Key + "_" + this.GetHashCode(), (Place) p.Value));
            PatternNodes.Where(p => p is Transition).
                ToList().ForEach(p => descriptor.Transitions.Add(p.Key + "_" + this.GetHashCode(), p.Value as Transition));

        }
    }
}
