using System;
using System.Collections.Generic;
namespace Munglo.AI
{
    public struct ActionDebugInfoStruct : IComparer<ActionDebugInfoStruct>, System.IComparable<ActionDebugInfoStruct>, System.IComparable<ActionNonPossibleDebugInfoStruct>
    {
        public string name;
        public string message;
        public int priorityFull;
        public int startPriority;
        public int minPriority;
        public int maxPriority;
        public int modInternal;
        public float modValuePriority;
        public float modMultiplierPriority;
        public ActionDebugInfluenceStruct[] influences;
        public int clampedPriority { get => Math.Clamp(priorityFull, minPriority, maxPriority); }

        public int Compare(ActionDebugInfoStruct x, ActionDebugInfoStruct y)
        {
            return x.name.CompareTo(y.name);
        }
        public int Compare(ActionNonPossibleDebugInfoStruct x, ActionDebugInfoStruct y)
        {
            return x.name.CompareTo(y.name);
        }
        public int Compare(ActionDebugInfoStruct x, ActionNonPossibleDebugInfoStruct y)
        {
            return x.name.CompareTo(y.name);
        }
        public int CompareTo(ActionDebugInfoStruct obj)
        {
            return this.name.CompareTo(obj.name);
        }
        public int CompareTo(ActionNonPossibleDebugInfoStruct obj)
        {
            return this.name.CompareTo(obj.name);
        }

        public override bool Equals(System.Object obj)
        {
            if ((obj == null) || !this.GetType().Equals(obj.GetType()))
            {
                return false;
            }
            return ((ActionDebugInfoStruct)obj).name == this.name;
        }

        public bool Equals(ActionDebugInfoStruct obj)
        {
            if (!this.GetType().Equals(obj.GetType()))
            {
                return false;
            }
            return obj == this;
        }

        public static bool operator ==(ActionDebugInfoStruct x, ActionDebugInfoStruct y)
        {
            return x.name == y.name && x.name == y.name;
        }
        public static bool operator !=(ActionDebugInfoStruct x, ActionDebugInfoStruct y)
        {
            return !(x == y);
        }

        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }
    }//EOF STRUCT
}