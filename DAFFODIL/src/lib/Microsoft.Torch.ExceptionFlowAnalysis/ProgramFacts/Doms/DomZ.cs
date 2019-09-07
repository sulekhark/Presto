
namespace Microsoft.Torch.ExceptionFlowAnalysis.ProgramFacts.Doms
{
    public class DomZ : Dom<int>
    {
        public DomZ() : base("Z")
        {
            for (int i = 0; i < 64; i++)
            {
                base.Add(i);
            }
        }
    }
}
