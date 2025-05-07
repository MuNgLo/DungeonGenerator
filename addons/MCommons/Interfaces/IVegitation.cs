using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Munglo.Commons;

public interface IVegitation
{
    public int AIID { get; }
    public void VegitationReset();
    public void VegitationUpdate(float delta);
    public float Consume();
}
