using System;
using System.Collections.Generic;

namespace ServicesPetriNet.Core
{
    public interface IGroupDescriptor
    {
        Dictionary<string, FieldDescriptor<Place>> Places { get; set; }

        Dictionary<string, FieldDescriptor<Transition>> Transitions { get; set; }
        Dictionary<string, FieldDescriptor<Group>> SubGroups { get; set; }
        List<Pattern> Patterns { get; set; }

        List<Type> MarkTypes { get; set; }
        List<MarkType> Marks { get; set; }

        Group Host { get; set; }
        void Refresh();

        void ApplyToAllSubGroups(Action<FieldDescriptor<Group>> action);
        dynamic DebugGetMarksTree();
    }
}
