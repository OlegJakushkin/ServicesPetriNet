using System;
using System.Collections.Generic;

namespace ServicesPetriNet.Core
{
    public class FieldDescriptor<T>
    {
        public List<Attribute> Attributes = new List<Attribute>();
        public T Value;
    }
}
