using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectEngine.Logic
{
    public interface ILogicObj
    {
        void Update(TimeSpan elapsed, TimeSpan totalTime);
    }
}
