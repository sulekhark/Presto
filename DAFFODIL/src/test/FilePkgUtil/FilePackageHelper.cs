using System;
using System.Collections.Generic;
using System.IO;

public static class FilePackageHelper
{
    public static bool logOnly;

    public static void HandleError(int errId, string msg)
    {
        if (logOnly)
        {
            System.Console.WriteLine("Error: ID:" + errId + " Error Msg:" + msg);
        }
        else
        {
            switch (errId)
            {
                case 0:
                    ThrowEx(msg);
                    break;
                case 1:
                    ThrowNullRefEx(msg);
                    break;
                case 2:
                    ThrowEx(msg);
                    break;
                case 3:
                    ThrowFileNotFoundEx(msg);
                    break;
            }
        }
    }

    public static void HandleInputError(string msg)
    {
        System.Console.WriteLine("Input argument error: Giving up...");
        ThrowArgumentEx(msg);
    }

    public static void ThrowNullRefEx(string msg)
    {
        throw new NullReferenceException(msg);
    }

    public static void ThrowEx(string msg)
    {
        throw new Exception(msg);
    }

    public static void ThrowFileNotFoundEx(string msg)
    {
        throw new FileNotFoundException(msg);
    }

    public static void ThrowArgumentEx(string msg)
    {
        throw new ArgumentException(msg);
    }

    public static bool IsFatal(Exception e)
    {
        return false;
    }
}
