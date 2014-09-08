using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectEngine
{
    public interface ICompositeDrawableObj
    {
        List<DrawableObj> GetDrawablePart();
    }
}
