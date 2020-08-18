using JetBrains.Annotations;

namespace ServicesPetriNet.Core.Transitions
{
    public class GenerateOne<Tout>
        where Tout : MarkType, new()
    {
        [UsedImplicitly]
        public Tout Action()
        {
            var result = new Tout();
            return result;
        }
    }
}
