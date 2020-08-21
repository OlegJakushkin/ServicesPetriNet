using Dynamitey.DynamicObjects;

namespace ServicesPetriNet
{
    public class PlotFrame
    {
        public double Key;
        public double Value;
        public PlotFrame(double key, double value)
        {
            Key = key;
            Value = value;
        }
    }

    public class Program
    {
        private static void Main(string[] args)
        {
            GustafsonLawDemoProgram.Main();
            //AmdahlLawDemoProgram.Main();
        }
    }
}
