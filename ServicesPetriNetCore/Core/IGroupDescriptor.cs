using System;
using System.Collections.Generic;

namespace ServicesPetriNet.Core
{
    public class FieldDescriptor<T>
    {
        public T Value;
        public List<Attribute> Attributes = new List<Attribute>();
    }
    public interface IGroupDescriptor
    {


        Dictionary<string, FieldDescriptor<Place>> Places { get; set; }

        Dictionary<string, FieldDescriptor<Transition>> Transitions { get; set; }
        Dictionary<string, FieldDescriptor<Group>> SubGroups { get; set; }
        List<Pattern> Patterns { get; set; }

        List<Type> MarkTypes { get; set; }
        List<MarkType> Marks { get; set; }
        void Refresh();
    }
}
