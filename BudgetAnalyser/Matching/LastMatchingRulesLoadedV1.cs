using Rees.Wpf;

namespace BudgetAnalyser.Matching
{
    public class LastMatchingRulesLoadedV1 : IPersistent
    {
        public object Model { get; set; }

        public int Sequence { get { return 100; } }
        
        public T AdaptModel<T>()
        {
            return (T)Model;
        }
    }
}