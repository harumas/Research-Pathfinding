namespace PathFinder.Core
{
    public readonly struct AgentContext
    {
        public readonly int AgentIndex;
        public readonly int Position;
        public readonly int Goal;

        public AgentContext(int agentIndex, int position, int goal)
        {
            AgentIndex = agentIndex;
            Position = position;
            Goal = goal;
        }
    }
}