using System;

namespace ServicesPetriNet.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class PrioretyAttribute : Attribute
    {
        public PrioretyAttribute(int Priorety = 999) { Priorety = Priorety; }

        public int Priorety { get; }
    }
}
