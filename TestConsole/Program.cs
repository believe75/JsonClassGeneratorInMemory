﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var test = new ClassGenerator.ClassGenerator();
            test.ClassName = "TestProcess";
            var t = test.Generate();


        }
    }
}
