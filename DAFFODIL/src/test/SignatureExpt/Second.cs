// Copyright (c) Sulekha Kulkarni.  All Rights Reserved.  Licensed under the MIT License.  See License.txt in the project root for license information.

﻿namespace SignatureExpt.NSSecond
{
    class A
    {
        public void Foo()
        {
            NSCommon.MyObj fooObj = new NSCommon.MyObj();
            NSCommon.MyWrapper fooWrapper = new NSCommon.MyWrapper(fooObj);
        }

        public NSCommon.MyWrapper Bar(int i)
        {
            NSCommon.MyObj barObj = new NSCommon.MyObj();
            NSCommon.MyWrapper barWrapper = new NSCommon.MyWrapper(barObj);
            return barWrapper;
        }
    }
}

