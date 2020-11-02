using System;
namespace Lab1
{
    public static class Class26NumSystem
    {
        public static string To26(uint num)
        {
            string res = "";
            uint[] nums = new uint[100];
            int i = 0;
            while (num >= 26)
            {
                nums[i] = num % 26;
                i++;
                num = num / 26 - 1;
            }
            nums[i] = num;
            for (int j = i; j >= 0; j--)
                res += ((char)('A' + nums[j])).ToString();
            return res;
        }
        //public static uint To10(string s)
        //{
        //    return 0;
        //}
    }
}
