// Copyright (c) Sulekha Kulkarni.  All Rights Reserved.  Licensed under the MIT License.  See License.txt in the project root for license information.

﻿using System;


namespace T14
{
    public class T14
    {
        static void Main(string[] args)
        {
            try
            {
                Foo();
            }
            catch(ArgumentException ae)
            {
                Console.WriteLine("argument exc {0}", ae.Message);
                try
                {
                    throw new Exception();
                }
                catch (Exception e)
                {

                }
            }
            catch(FieldAccessException fae)
            {
                Console.WriteLine("field access exc {0}", fae.Message);
            }
        }

        static void Foo()
        {
            try
            {
                Bar bar = new Bar();
                throw (bar.fae);
            }
            finally
            {
                throw (new ArgumentException());
            }
        }
    }

    public class Bar
    {
        public FieldAccessException fae;

        public Bar()
        {
            fae = new FieldAccessException();
        }
    }
}
