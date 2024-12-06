using System;

namespace PathFinder.Core
{
    /// <summary>
    /// 探索用ノード
    /// </summary>
    public class Node : IEquatable<Node>
    {
        public readonly int Index;
        public readonly Vector2 Position;

        public int Time { get; set; }
        public float H { get; set; }
        public float F => Time + H;
        public Node Parent { get; set; }

        public Node(int index, Vector2 position)
        {
            Index = index;
            Position = position;
        }

        public Node Clone()
        {
            return new Node(Index, Position) { Time = Time, H = H, Parent = Parent };
        }

        public void Reset()
        {
            Time = 0;
            H = 0;
            Parent = null;
        }

        public bool Equals(Node other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Index == other.Index && Position.Equals(other.Position) && Time == other.Time;
        }
    }
}