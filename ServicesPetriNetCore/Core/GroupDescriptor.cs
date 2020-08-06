using System;
using System.Collections.Generic;
using System.Linq;
using static ServicesPetriNet.Extensions;

namespace ServicesPetriNet.Core {
    public class GroupDescriptor : IGroupDescriptor
    {
        public static GroupDescriptor CreateInstance(Group instance) { return new GroupDescriptor(instance); }
        private Group host;

        private GroupDescriptor(Group instance)
        {
            Marks = new List<MarkType>();
            MarkTypes = new List<Type>();
            Patterns = new List<Pattern>();
            host = instance;
            Refresh();
        }

        public static List<Type> GetAllMarkTypes(List<MarkType> marks, List<Transition> transitions)
        {
            var result = marks.Select(m => m.GetType()).ToList();
            result.AddRange(transitions.SelectMany(t => t.Links.Select(link => link.What)));
            return result.Distinct().ToList();
        }

        public Dictionary<string, FieldDescriptor<Place>> Places { get; set; }
        public Dictionary<string, FieldDescriptor<Transition>> Transitions { get; set; }
        public Dictionary<string, FieldDescriptor<Group>> SubGroups { get; set; }
        public List<Type> MarkTypes { get; set; }
        public List<MarkType> Marks { get; set; }
        public List<Pattern> Patterns { get; set; }
        public void Refresh() { 
            Places = GetAllPlaces(host);
            Transitions = GetAllTransitions(host);
            SubGroups = GetAllGroups(host);
            Marks = Places.SelectMany(p => p.Value.Value.GetMarks()).ToList();
            MarkTypes = GetAllMarkTypes(Marks, Transitions.Values.Select(descriptor=> descriptor.Value ).ToList());
            Patterns.ForEach(p => p.RefreshHostDescriptor(this));
        }

        public void ApplyToAllSubGroups(Action<FieldDescriptor<Group>> action)
        {
            foreach (var fieldDescriptor in SubGroups) {
                action(fieldDescriptor.Value);
            }
        }
    }
}
