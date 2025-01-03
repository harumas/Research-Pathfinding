using System.Collections.Generic;

namespace PathFinder.Core
{
    public interface ISolver
    {
        List<int> Solve(int start, int goal);
    }
}