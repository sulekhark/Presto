using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace T2_NS
{
    public class T2
    {
        public void foo_diff()
        {
            int x, y, w, z;

            x = 5;
            x = x - 3;
            if (x < 3)
            {
                y = x * 2;
                w = y;
            }
            else
            {
                y = x - 3;
            }
            w = x - y;
            z = x + y;
        }
    }

    public class NotReq
    {
    }
}
