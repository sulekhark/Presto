// Copyright (c) Sulekha Kulkarni.  All Rights Reserved.  Licensed under the MIT License.  See License.txt in the project root for license information.

﻿
namespace T18
{
    public class T18
    {
        static void Main(string[] args)
        {
            A aaa = new A();
            MyObj ooo = new MyObj(aaa);
            A bbb = ooo.MyValue;
        }
    }

    public class MyObj
    {
        private A val;
        private int count;

        public A MyValue
        {
            get
            {
                count++;
                return val;
            }
        }
           
        public MyObj (A ip)
        {
            val = ip;
            count = 0;
        }

    }

    public class A { }
} 
