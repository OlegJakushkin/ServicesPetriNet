using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using static ServicesPetriNet.Extensions;

namespace ServicesPetriNet.Core
{
    public class GroupDescriptor : IGroupDescriptor
    {
        private readonly Group host;

        private GroupDescriptor(Group instance)
        {
            Marks = new List<MarkType>();
            MarkTypes = new List<Type>();
            Patterns = new List<Pattern>();
            host = instance;
            Refresh();
        }

        public Dictionary<string, FieldDescriptor<Place>> Places { get; set; }
        public Dictionary<string, FieldDescriptor<Transition>> Transitions { get; set; }
        public Dictionary<string, FieldDescriptor<Group>> SubGroups { get; set; }
        public List<Type> MarkTypes { get; set; }
        public List<MarkType> Marks { get; set; }
        public List<Pattern> Patterns { get; set; }

        public void Refresh()
        {
            Places = GetAllPlaces(host);
            Transitions = GetAllTransitions(host);
            SubGroups = GetAllGroups(host);
            Marks = Places.SelectMany(p => p.Value.Value.GetMarks()).ToList();
            MarkTypes = GetAllMarkTypes(Marks, Transitions.Values.Select(descriptor => descriptor.Value).ToList());
            Patterns.ForEach(p => p.RefreshHostDescriptor(this));
        }

        public void ApplyToAllSubGroups(Action<FieldDescriptor<Group>> action)
        {
            foreach (var fieldDescriptor in SubGroups) action(fieldDescriptor.Value);
        }

        public dynamic DebugGetMarksTree()
        {
            var result = new ExpandoObject();

            Action<Group> a = null;

            a = g =>
            {
                var p = g.Descriptor.Places;
                foreach (var fieldDescriptor in p)
                    result.TryAdd(g.GetType().Name + "." + fieldDescriptor.Key, fieldDescriptor.Value.Value.GetMarks());
                g.Descriptor.ApplyToAllSubGroups(descriptor => a(descriptor.Value));
            };

            a(host);


            return result;
        }

        public static GroupDescriptor CreateInstance(Group instance) { return new GroupDescriptor(instance); }

        public static List<Type> GetAllMarkTypes(List<MarkType> marks, List<Transition> transitions)
        {
            var result = marks.Select(m => m.GetType()).ToList();
            result.AddRange(transitions.SelectMany(t => t.Links.Select(link => link.What)));
            return result.Distinct().ToList();
        }
    }
}
