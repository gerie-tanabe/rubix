using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

namespace Rubix
{
    public static class Extensions
    {
        public static bool IsNumeric(string s)
        {
            s=s.Replace(" ", string.Empty);
           double output;
           return Double.TryParse(s,out output);
          

        }
    }
}
