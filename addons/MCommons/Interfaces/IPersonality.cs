using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Munglo.Commons;
/// <summary>
/// The baseclass for any NPC behaviour controlling class
/// </summary>
public interface IPersonality
{
    /// <summary>
    /// Call this to inject a value of nutrition into the personality carrying NPC. Consider 100 a full pizza meal where you get stuffed.
    /// For larger/smaller NPC's they will handle modifying the incoming value as fit
    /// </summary>
    /// <param name="value"></param>
    public void Feed(float value);
    /// <summary>
    /// Anything with personality has to be able to starve
    /// </summary>
    public void Starve();
}
