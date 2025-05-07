using System;
namespace Munglo.AI.Debug
{
    public struct AICustomDataStruct : System.IComparable<AICustomDataStruct>
    {
        public string sourceClass;
        public string message;
        public float value;
        public float normalizedValue;

        public int CompareTo(AICustomDataStruct other)
        {
            return this.sourceClass.CompareTo(other.sourceClass);
        }

        public override bool Equals(System.Object obj)
        {
            if ((obj == null) || !this.GetType().Equals(obj.GetType()))
            {
                return false;
            }
            return ((AICustomDataStruct)obj).sourceClass == this.sourceClass;
        }

        public bool Equals(AICustomDataStruct obj)
        {
            if (!this.GetType().Equals(obj.GetType()))
            {
                return false;
            }
            return obj == this;
        }
        public static bool operator ==(AICustomDataStruct x, AICustomDataStruct y)
        {
            return x.sourceClass == y.sourceClass;
        }
        public static bool operator !=(AICustomDataStruct x, AICustomDataStruct y)
        {
            return !(x == y);
        }
        public override string ToString()
        {
            return $"({sourceClass})::{message}";
        }

        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }
    }//EOF STRUCT
}