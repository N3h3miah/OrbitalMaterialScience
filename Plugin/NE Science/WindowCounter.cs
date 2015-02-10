using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NE_Science
{
    class WindowCounter
    {
        private static int counter = 1;

        public static int getNextWindowID()
        {
            return ++counter;
        }
    }
}
