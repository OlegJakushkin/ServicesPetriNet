using System;
using Fractions;

namespace ServicesPetriNet.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class TimeScaleAttribute : Attribute
    {
        public TimeScaleAttribute(int scale = 1) { Scale = scale; }

        public TimeScaleAttribute(double scale = 1) { Scale = Fraction.FromDoubleRounded(scale); }

        public TimeScaleAttribute(int numerator = 1, int denominator = 1)
        {
            Scale = new Fraction(numerator, denominator);
        }

        public Fraction Scale { get; }
    }
}
