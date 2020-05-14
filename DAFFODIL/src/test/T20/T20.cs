// Copyright (c) Sulekha Kulkarni.  All Rights Reserved.  Licensed under the MIT License.  See License.txt in the project root for license information.

﻿namespace T20
{
    class T20
    {
        static void Main(string[] args)
        {
            FooBase fooB = new FooBase();
            Process(fooB);
            FooDerived fooD = new FooDerived();
            Process(fooD);
        }

        static void Process(IFooBase fooParam)
        {
            FooDerived fooParamD = fooParam as FooDerived;
            if (fooParamD != null) fooParamD.FooDFunc();
            fooParam.FooBFunc();
        }
    }

    interface IFooBase
    {
        void FooBFunc();
    }

    interface IFooDerived : IFooBase
    {
        void FooDFunc();
    }

    class FooBase : IFooBase
    {
        public void FooBFunc() { }
    }

    class FooDerived : IFooDerived
    {
        public void FooBFunc() { }
        public void FooDFunc() { }
    }
}
