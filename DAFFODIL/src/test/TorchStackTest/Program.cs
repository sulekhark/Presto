// Copyright (c) Sulekha Kulkarni.  All Rights Reserved.  Licensed under the MIT License.  See License.txt in the project root for license information.

﻿using System;
using System.Threading.Tasks;


namespace TorchStackTest
{
    class Program 
    {
        static void Main(string[] args)
        {
            int y = 0;
            M2("", ref y);
            string s = "Everything fine.";
            try
            {
                M1();
            }
            catch (Exception e)
            {
                s = e.Message;
            }
            finally
            {
                Console.WriteLine("Done: {0}", s);
            }
        }

        static void M1()
        {
            int y = 0;
            M2("", ref y);
        }

        static void M2(string x, ref int y)
        {
             M3(null);
        }

        static void M3(string str)
        {
            if (str != null) throw new Exception();
        }
    }

}
