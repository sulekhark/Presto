// Copyright (c) Sulekha Kulkarni.  All Rights Reserved.  Licensed under the MIT License.  See License.txt in the project root for license information.

﻿using System;

namespace T6
{
    class T6
    {
        static void Main(string[] args)
        {
            bar();
        }
        public static void bar()
        {
            Console.WriteLine(foo(x => x + 1, 3));
        }
        static int foo(Func<int, int> f, int a)
        {
            return f(a);
        }
    }
}
