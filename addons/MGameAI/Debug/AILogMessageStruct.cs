using System;
namespace Munglo.AI.Debug
{
    public struct AILogMessageStruct
    {
        public AILogMessageStruct(string msg, int id, Object source)
        {
            sourceObject = source; AIObjectID = id; message = msg;
        }
        public Object sourceObject;
        public int AIObjectID;
        public string message;
    }
}