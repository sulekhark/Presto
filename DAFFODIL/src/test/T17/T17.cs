// Copyright (c) Sulekha Kulkarni.  All Rights Reserved.  Licensed under the MIT License.  See License.txt in the project root for license information.

﻿using System;
using System.IO;

namespace T17
{ 
        class T17
        {
            public delegate bool PS(string s);

            public static bool WriteToS(string str)
            {
                Console.WriteLine(str);
                return true;
            }

            public static bool WriteToF(string str)
            {
                Console.WriteLine(str);
                return true;
            }

            static void Main(string[] args)
            {
                PS ps1 = new PS(WriteToS);
                PS ps2 = new PS(WriteToF);
                PS ps = ps1;
                ps += ps2;
                bool success = ps("Hello World");
            }
        }
}
