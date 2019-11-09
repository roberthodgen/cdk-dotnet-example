using Amazon.CDK;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CdkHelloWorld
{
    class Program
    {
        static void Main(string[] args)
        {
            var app = new App(null);
            new CdkHelloWorldStack(app, "CdkHelloWorldStack", new StackProps());
            app.Synth();
        }
    }
}
