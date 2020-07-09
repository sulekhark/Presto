// Copyright (c) Sulekha Kulkarni.  All Rights Reserved.  Licensed under the MIT License.  See License.txt in the project root for license information.

﻿using System;
using System.IO;
using System.Threading.Tasks;

namespace T5
{
    class T5
    {
        public static void Main()
        {
            Task<int> task = HandleFoo();
            task.Wait();
        }

        static async Task<int> HandleFoo()
        {        
            int count = await foo();
            return count;
        }

        static Task<int> foo()
        {
            Task<int> tmo = Task<int>.Run(() =>
            {
                int i = 7;
                return i;
            });
            return tmo;
        }
    }
}
