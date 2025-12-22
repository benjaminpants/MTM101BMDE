using System;
using System.Collections.Generic;
using System.Text;

namespace MTM101BaldAPI
{
    /// <summary>
    /// Represents a boolean value that can either "agree" to be compared with another, or forcefully make the return value itself.
    /// </summary>
    public enum BooleanHandshake
    {
        /// <summary>
        /// This will always return false in a comparison, even if the other is AlwaysTrue.
        /// </summary>
        AlwaysFalse = 1,
        /// <summary>
        /// This will always return true in a comparison, even if the other is FalseIfAgree.
        /// </summary>
        AlwaysTrue = 2,
        /// <summary>
        /// This is the flag that represents if a value is agreeing with the other one.
        /// </summary>
        IfAgree = 4,
        /// <summary>
        /// This will return false in a comparison if the other is TrueIfAgree or AlwaysFalse.
        /// </summary>
        FalseIfAgree = AlwaysFalse | IfAgree,
        /// <summary>
        /// This will return true in a comparison if the other is TrueIfAgree or AlwaysTrue.
        /// </summary>
        TrueIfAgree = AlwaysTrue | IfAgree,
    }

    public static class BooleanHandshakeExtensions
    {
        public static bool CompareIfAgree(this BooleanHandshake me, BooleanHandshake other)
        {
            if ((me == BooleanHandshake.IfAgree) && (other == BooleanHandshake.IfAgree)) return true;
            // if both agree, then perform standard bool logic.
            // if both are forcefully trying to return a value, also perform standard bool logic since we have to have a winner.
            if ((me.HasFlag(BooleanHandshake.IfAgree) && other.HasFlag(BooleanHandshake.IfAgree)) ||
                (!me.HasFlag(BooleanHandshake.IfAgree) && !other.HasFlag(BooleanHandshake.IfAgree)))
            {
                // both have to be true if they agree.
                return me.HasFlag(BooleanHandshake.AlwaysTrue) && other.HasFlag(BooleanHandshake.AlwaysTrue);
            }
            // since we already handled the case if both agree, we only need to figure out which one agrees and return the value of the one that doesnt.
            if (me.HasFlag(BooleanHandshake.IfAgree))
            {
                return other.HasFlag(BooleanHandshake.AlwaysTrue);
            }
            return me.HasFlag(BooleanHandshake.AlwaysTrue);
        }

        public static bool AsBool(this BooleanHandshake me)
        {
            return me.HasFlag(BooleanHandshake.AlwaysTrue);
        }

        public static bool CompareIfAgree(this BooleanHandshake me, bool other)
        {
            return CompareIfAgree(me, other ? BooleanHandshake.TrueIfAgree : BooleanHandshake.FalseIfAgree);
        }
    }
}
