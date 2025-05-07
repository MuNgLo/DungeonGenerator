using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Munglo.Commons;
/// <summary>
/// Declaring the common movement related values for AI
/// </summary>
public interface IMovement
{
    float MaxRandomMoveDistance { get; }
    float GroundSpeed { get; }
}// EOI

