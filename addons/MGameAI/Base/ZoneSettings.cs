using System.Collections;
using System.Collections.Generic;
namespace Munglo.AI.Base
{
    [System.Serializable]
    public class ZoneSettings
    {
        //[UnityEngine.SerializeField, UnityEngine.Range(1, 50)]
        private float range = 10;
        //[UnityEngine.Range(1, 50)]
        public int width = 10;
        //[UnityEngine.Range(0.1f, 10.0f)]
        public float baseWidth = 1.0f;
        //[UnityEngine.SerializeField, UnityEngine.Range(1, 20)]
        private int height = 10;
        //[UnityEngine.Range(-20, 0)]
        public int heightOffset = 0;
        //[UnityEngine.Range(0.0f, -10.0f)]
        public float rangeOffset = 0.0f;
        public float Range { get => range + rangeOffset; set { range = value - rangeOffset; } }
        public int Height { get => height + heightOffset; set { height = value - heightOffset; } }
    }
    [System.Serializable]
    public struct Edge
    {
        public int Start;
        public int End;
    }
}