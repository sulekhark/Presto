using System;
using System.IO;

public static class FilePackageHelper
{
    public static bool simulateError2;
    public static void HandleError(int errId, string msg)
    {
        if (errId == -1) return;
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
        }
    }

    public static void CheckInput(FileInfo fileInfo, string msg)
    {
        if (!fileInfo.Exists || simulateError2)
        {
            System.Console.WriteLine("Input argument error: Giving up...");
            ThrowArgumentEx(msg);
        }
    }

    public static void ThrowNullRefEx(string msg)
    {
        throw new NullReferenceException(msg);
    }

    public static void ThrowEx(string msg)
    {
        throw new Exception(msg);
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
