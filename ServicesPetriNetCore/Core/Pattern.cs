using System;
using System.Collections.Generic;
using System.Linq;

namespace ServicesPetriNet.Core
{
    public class Pattern
    {
        private readonly List<KeyValuePair<string, INode>> PatternNodes = new List<KeyValuePair<string, INode>>();

        public Pattern()
        {
            Extensions.InitAllTypeInstances<Place>(this);
            Extensions.InitAllTypeInstances<Transition>(this);
        }

        protected void Register(string name)
        {
            var p = Extensions.InitSingleNode(this, name);
            PatternNodes.Add(new KeyValuePair<string, INode>(name, p));
        }

        public virtual void RefreshHostDescriptor(IGroupDescriptor descriptor)
        {
            PatternNodes.Where(p => p.Value.GetType() == typeof(Place)).ToList().ForEach(
                p => descriptor.Places.Add(
                    p.Key + "_" + GetHashCode(),
                    new FieldDescriptor<Place> {
                        Value = p.Value as Place,
                        Attributes = GetType().GetField(p.Key).GetCustomAttributes(true).Cast<Attribute>().ToList()
                    }
                )
            );
            PatternNodes.Where(p => p.Value.GetType() == typeof(Transition)).ToList().ForEach(
                p => descriptor.Transitions.Add(
                    p.Key + "_" + GetHashCode(),
                    new FieldDescriptor<Transition> {
                        Value = p.Value as Transition,
                        Attributes = GetType().GetField(p.Key).GetCustomAttributes(true).Cast<Attribute>().ToList()
                    }
                )
            );
        }
    }
}
