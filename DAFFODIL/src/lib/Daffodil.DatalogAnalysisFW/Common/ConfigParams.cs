// Copyright (c) Sulekha Kulkarni.  All Rights Reserved.  Licensed under the MIT License.  See License.txt in the project root for license information.

﻿using System;
using System.IO;


namespace Daffodil.DatalogAnalysisFW.Common
{
    public static class ConfigParams
    {
        public static string AssemblyPath { get; set; }
        public static string DatalogDir { get; set; }
        public static string LogDir { get; set; }
        public static string Z3ExePath { get; set; }
        public static string StubsPath { get; set; }
        public static string AnalysesPath { get; set; }
        public static bool LoadSavedScope { get; set; }
        public static string SaveScopePath { get; set; }
        public static bool SuppressSystemExceptions { get; set; }
        public static int SystemExceptionsLimit { get; set; }
        public static string[] SystemExceptionsAllow { get; set; }
        public static string[] AppClassPrefixes { get; set; }
        public static string SourceRoot { get; set; }

        public static void LoadConfig(string filePath)
        {
            using (StreamReader sr = new StreamReader(filePath))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    try
                    {
                        if (line.StartsWith("AssemblyPath= "))
                        {
                            AssemblyPath = line.Split()[1];
                        }
                        else if (line.StartsWith("DatalogDir= "))
                        {
                            DatalogDir = line.Split()[1];
                        }
                        else if (line.StartsWith("LogDir= "))
                        {
                            LogDir = line.Split()[1];
                        }
                        else if (line.StartsWith("Z3ExePath= "))
                        {
                            Z3ExePath = line.Split()[1];
                        }
                        else if (line.StartsWith("StubsPath= "))
                        {
                            StubsPath = line.Split()[1];
                        }
                        else if (line.StartsWith("AnalysesPath= "))
                        {
                            AnalysesPath = line.Split()[1];
                        }
                        else if (line.StartsWith("SaveScopePath= "))
                        {
                            SaveScopePath = line.Split()[1];
                        }
                        else if (line.StartsWith("LoadSavedScope= "))
                        {
                            string boolStr = line.Split()[1];
                            if (boolStr == "true")
                                LoadSavedScope = true;
                            else
                                LoadSavedScope = false;
                        }
                        else if (line.StartsWith("SuppressSystemExceptions= "))
                        {
                            string boolStr = line.Split()[1];
                            if (boolStr == "true")
                                SuppressSystemExceptions = true;
                            else
                                SuppressSystemExceptions = false;
                        }
                        else if (line.StartsWith("SystemExceptionsLimit= "))
                        {
                            string intStr = line.Split()[1];
                            SystemExceptionsLimit = Int32.Parse(intStr);
                        }
                        else if (line.StartsWith("SystemExceptionsAllow= "))
                        {
                            string allowedCl = line.Split()[1];
                            SystemExceptionsAllow = allowedCl.Split(':');
                        }
                        else if (line.StartsWith("AppClassPrefixes= "))
                        {
                            string appCl = line.Split()[1];
                            AppClassPrefixes = appCl.Split(':');
                        }
                        else if (line.StartsWith("SourceRoot= "))
                        {
                            SourceRoot = line.Split()[1];
                        }
                    } catch (Exception e)
                    {
                        Console.WriteLine("Got Exception: {0}", e.Message);
                    }
                }
            }
        }

        public static void StoreConfig(string filePath)
        {
            using (StreamWriter sw = new StreamWriter(filePath))
            {
                sw.WriteLine("AssemblyPath= {0}", AssemblyPath);
                sw.WriteLine("DatalogDir= {0}", DatalogDir);
                sw.WriteLine("LogDir= {0}", LogDir);
                sw.WriteLine("Z3ExePath= {0}", Z3ExePath);
                sw.WriteLine("StubsPath= {0}", StubsPath);
                sw.WriteLine("AnalysesPath= {0}", AnalysesPath);
                sw.WriteLine("SaveScopePath= {0}", SaveScopePath);
                sw.WriteLine("LoadSavedScope= {0}", LoadSavedScope ? "true" : "false");
                sw.WriteLine("SuppressSystemExceptions= {0}", SuppressSystemExceptions ? "true" : "false");
                sw.WriteLine("SystemExceptionsLimit= {0}", SystemExceptionsLimit.ToString());
                sw.WriteLine("SystemExceptionsAllow= {0}", String.Join(":", SystemExceptionsAllow));
                sw.WriteLine("AppClassPrefixes= {0}", String.Join(":", AppClassPrefixes));
                sw.WriteLine("SourceRoot= {0}", SourceRoot);
            }
        }
    }
}
