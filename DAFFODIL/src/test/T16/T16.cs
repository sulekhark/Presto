// Copyright (c) Sulekha Kulkarni.  All Rights Reserved.  Licensed under the MIT License.  See License.txt in the project root for license information.

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace T16
{
    class T16
    {
        static void Main(string[] args)
        {
            Foo foo;
            GetFoo(out foo);
            Str bar;
            bar.fld = foo;
            Str baz = bar;
            Foo taz = bar.fld;
            // bar.dummy();
        }

        public static void GetFoo(out Foo fooP)
        {
            fooP = new Foo();
        }
    }

    class Foo { }
   
    struct Str
    {
        public Foo fld;
        // public void dummy() { }
    }
   
}
