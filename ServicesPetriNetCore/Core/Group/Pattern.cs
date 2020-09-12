using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ServicesPetriNet.Core
{
    public class Pattern
    {
        private readonly List<KeyValuePair<string, INode>> PatternNodes = new List<KeyValuePair<string, INode>>();
        private readonly List<Pattern> PatternPatterns = new List<Pattern>();
        public Pattern(Group hostCtx) { Host = hostCtx; }

        public Group Host { get; set; }


        protected void RegisterNode(string name)
        {
            var p = Extensions.InitSingleNode(this, name);
            RegisterNode(name, p);
        }

        protected void RegisterNode(string name, INode p)
        {
            PatternNodes.Add(new KeyValuePair<string, INode>(name, p));
        }

        protected void RegisterPattern(Pattern p) { PatternPatterns.Add(p); }

        protected void RegisterList(string name, int count)
        {
            var p = Extensions.InitListOfNodes(this, name, count);
            for (var i = 0; i < count; i++)
                PatternNodes.Add(new KeyValuePair<string, INode>(name + "_" + i + "_", p[i]));
        }

        protected void RegisterList<T>(string name, List<T> p) where T : INode
        {
            for (var i = 0; i < p.Count; i++)
                PatternNodes.Add(new KeyValuePair<string, INode>(name + "_" + i + "_", p[i]));
        }

        public virtual void RefreshHostDescriptor(IGroupDescriptor descriptor)
        {
            PatternNodes.Where(p => p.Value.GetType() == typeof(Place)).ToList().ForEach(
                pp =>
                {
                    var k = pp.Key + "_" + GetHashCode();
                    var v = new FieldDescriptor<Place> {
                        Value = pp.Value as Place,
                        Attributes =
                            new List<Attribute>() // GetType().GetField(pp.Key).GetCustomAttributes(true).Cast<Attribute>().ToList()
                    };
                    descriptor.Places.Add(
                        k,
                        v
                    );
                }
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
                    var val = new FieldDescriptor<Transition>()
                    {
                        Value = (Transition) p.Value
                    };

                    
                    if (f != null)
                    {
                        var attrs = f.GetCustomAttributes(true).Cast<Attribute>().ToList();
                        val.Attributes = attrs;
                    }

                    descriptor.Transitions.Add(key, val);
                }
            );
        }

        public void ApplyToNestedPatterns(Action<Pattern> act) { PatternPatterns.ForEach(pp => act(pp)); }

        public void ApplyToGeneratedNodes(Action<string, INode> act)
        {
            PatternNodes.ForEach(pp => act(pp.Key, pp.Value));
        }
    }
}
