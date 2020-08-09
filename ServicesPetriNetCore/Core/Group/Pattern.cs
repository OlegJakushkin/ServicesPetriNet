using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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


            var pns = PatternNodes.Where(p => p.Value.GetType() == typeof(Transition)).ToList();


            pns.ForEach(
                p =>
                {
                    var key = p.Key + "_" + GetHashCode();
                    var t = GetType();
                    var f = t.GetField(
                        p.Key,
                        BindingFlags.Public | BindingFlags.NonPublic |
                        BindingFlags.Instance
                    );
                    var attrs = f.GetCustomAttributes(true).Cast<Attribute>().ToList();
                    var val = new FieldDescriptor<Transition> {
                        Value = (Transition) p.Value,
                        Attributes = attrs
                    };
                    descriptor.Transitions.Add(key, val);
                }
            );
        }
    }
}
