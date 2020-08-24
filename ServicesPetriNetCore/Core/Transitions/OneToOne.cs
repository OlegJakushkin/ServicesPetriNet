using JetBrains.Annotations;

namespace ServicesPetriNet.Core.Transitions
{
    public class OneToOne<Tin, Tout> : ActionBase
        where Tin : MarkType
        where Tout : MarkType, new()
    {
        [UsedImplicitly]
        public Tout Action(Tin ps)
        {
            var result = new Tout();
            result.Parent = ps.Parent;

            if (result is IPart rr &&
                ps is IPart src) {
                rr.Number = src.Number;
                rr.From = src.From;
            }

            return result;
        }
    }

    public class OneToOne<T> : OneToOne<T, T>
        where T : MarkType, new() { }
}
