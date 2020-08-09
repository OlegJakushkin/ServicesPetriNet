using System;

namespace ServicesPetriNet.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class TimeScaleAttribute : Attribute
    {
        public TimeScaleAttribute(int Scale = 1) { Scale = Scale; }

        public int Scale { get; }
    }
}
