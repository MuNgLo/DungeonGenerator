using System;
using System.Collections.Generic;
namespace Munglo.AI
{
    public struct ActionNonPossibleDebugInfoStruct : IComparer<ActionNonPossibleDebugInfoStruct>, System.IComparable<ActionNonPossibleDebugInfoStruct>
    {
        public string name;
        public string message;
        public ActionNonPossibleDebugInfoStruct(string n, string m)
        {
            name = n;
            message = m;
        }

        public int Compare(ActionNonPossibleDebugInfoStruct x, ActionNonPossibleDebugInfoStruct y)
        {
            return x.name.CompareTo(y.name);
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
            return ((ActionNonPossibleDebugInfoStruct)obj).name == this.name;
        }

        public bool Equals(ActionNonPossibleDebugInfoStruct obj)
        {
            if ((obj == null) || !this.GetType().Equals(obj.GetType()))
            {
                return false;
            }
            return obj == this;
        }

        public static bool operator ==(ActionNonPossibleDebugInfoStruct x, ActionNonPossibleDebugInfoStruct y)
        {
            return x.name == y.name;
        }
        public static bool operator !=(ActionNonPossibleDebugInfoStruct x, ActionNonPossibleDebugInfoStruct y)
        {
            return !(x == y);
        }

        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }
    }
}