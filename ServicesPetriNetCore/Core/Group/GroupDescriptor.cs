using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Newtonsoft.Json;
using static ServicesPetriNet.Extensions;

namespace ServicesPetriNet.Core
{
    public class GroupDescriptor : IGroupDescriptor
    {
        private GroupDescriptor(Group instance)
        {
            Marks = new List<MarkType>();
            MarkTypes = new List<Type>();
            Patterns = new List<Pattern>();
            Host = instance;
            Refresh();
        }

        [JsonIgnore]
        public Group Host { get; set; }

        [JsonIgnore]
        public Dictionary<string, FieldDescriptor<Place>> Places { get; set; }

        [JsonIgnore]
        public Dictionary<string, FieldDescriptor<Transition>> Transitions { get; set; }

        [JsonIgnore]
        public Dictionary<string, FieldDescriptor<Group>> SubGroups { get; set; }

        [JsonIgnore]
        public List<Type> MarkTypes { get; set; }

        [JsonIgnore]
        public List<MarkType> Marks { get; set; }

        [JsonIgnore]
        public List<Pattern> Patterns { get; set; }

        public void Refresh()
        {
            Places = GetAllPlaces(Host);
            Transitions = GetAllTransitions(Host);
            SubGroups = GetAllGroups(Host);
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
            Refresh();
            var result = new ExpandoObject();

            Action<Group> a = null;

            a = g =>
            {
                var p = g.Descriptor.Places;
                foreach (var fieldDescriptor in p)
                    result.TryAdd(g.GetType().Name + "." + fieldDescriptor.Key, fieldDescriptor.Value.Value.GetMarks());
                g.Descriptor.ApplyToAllSubGroups(descriptor => a(descriptor.Value));
            };

            a(Host);


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
