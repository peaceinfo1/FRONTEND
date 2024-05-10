using System;
using System.Collections.Generic;
using System.Text;

namespace Utils
{
    public class Helper
    {
        public static string GetOTP()
        {
            int _min = 100000;
            int _max = 999999;
            Random _rdm = new Random();
            return _rdm.Next(_min, _max).ToString();
        }
    }
}
