using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Munglo.Movement.Input
{
    [Serializable]
    public class InputSettings
    {
        public float _sensitivity = 30.0f;
        public bool _invertMouse = false;
        public float _ySensMultiplier = 1.0f;
    }
}
