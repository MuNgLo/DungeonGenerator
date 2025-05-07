using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Munglo.Commons
{
    /*
     * This defines the delgates thater used to hook up modifiers. This way a stomache script will be able to tell AIMind to hook up its
     * multiplier modifier with eat actions
     * 
     */
    /// <summary>
    /// returns the value to add to priority
    /// </summary>
    /// <returns></returns>
    public delegate int ModifierValue();
    /// <summary>
    /// 1.0f is 100%. Lower is a effectively a negative modifier.
    /// </summary>
    /// <returns></returns>
    public delegate float ModifierMultiplier(); 
}
