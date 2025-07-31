using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gg.parse.rulefunctions
{
    public interface IRuleComposition<T> where T : IComparable<T>
    {
        IEnumerable<RuleBase<T>> SubRules { get; }
    }
}
