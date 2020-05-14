// Copyright (c) Sulekha Kulkarni.  All Rights Reserved.  Licensed under the MIT License.  See License.txt in the project root for license information.

﻿using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace MNTest
{
    class MNTest
    {
        static void Main(string[] args)
        {
            Foo foo = new Foo();
            foo.DoBar();
        }
    }

    class Foo
    {
        public int DoBar()
        {
            string s = "hello";
            Task<int> res = DoAsyncWork(s);
            res.Wait();
            return res.Result;
        }

        public async Task<int> DoAsyncWork(string strParam)
        {
            int res1;
            try
            {
                res1 = await MyTask1(strParam);
            }
            catch (Exception e)
            {
                throw e;
            }
            return res1;
        }

        public Task<int> MyTask1(string sp)
        {
            Task<int> itsk;
            int val = 0;
            itsk = Task.Run(() =>
            {
                val = Process(val, sp);
                return val;
            });
            return itsk;
        }

        public int Process(int val, string s)
        {
            int curr = val;
            curr += s.Length;
            if (curr > 10) throw new Exception("Value beyond upper limit.");
            return curr;
        }
    }
 }
