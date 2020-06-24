using System;
using System.Collections.Generic;

public static class FilePackageHelper
{
    static bool logOnly;

    public static void HandleError(int errId, string msg)
    {
        if (logOnly)
        {
        }
        else
        {
            switch (errId)
            {
                case 1:
                    ThrowExcep(msg);
                    break;
            }
        }
    }

    public static void HandleSeriousError(int errId, string msg)
    {
    }
}
