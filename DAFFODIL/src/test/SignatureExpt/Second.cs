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

