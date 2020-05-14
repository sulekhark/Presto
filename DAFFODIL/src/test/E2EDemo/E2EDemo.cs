// Copyright (c) Sulekha Kulkarni.  All Rights Reserved.  Licensed under the MIT License.  See License.txt in the project root for license information.

﻿using System;

namespace E2EDemo
{
    class E2EDemo
    {
        static bool flag = false;
        static void Main(string[] args)
        {
            M1();
            M2(null);
            System.Console.WriteLine("Done");
        }

        static void M1()
        {
            try
            {
                M4();
            }
            catch (Exception e)
            {
                System.Console.WriteLine(e.Message);
            }
            finally
            {
                M3();
            }
        }

        static void M4()
        {
            CFoo objFoo = new CFoo();
            M2(objFoo);
        }

        static void M2(CFoo paramFoo)
        {
            if (paramFoo != null)
            {
                M5();
                M6();
            }
        }

        static void M3() { if (flag) throw new ArgumentException(); }
        static void M5() { if (flag) throw new NullReferenceException(); }
        static void M6() { if (flag) throw new FieldAccessException(); }
    }

    class CFoo { }
}