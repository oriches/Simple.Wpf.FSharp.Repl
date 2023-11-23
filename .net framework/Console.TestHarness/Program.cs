using System;
using System.Diagnostics;
using Simple.Wpf.FSharp.Repl.Core;

namespace Console.TestHarness
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            using (var engine = new ReplEngine())
            {
                engine.State.Subscribe(x => Debug.WriteLine("state = " + x));
                engine.Output.Subscribe(System.Console.Write);
                engine.Start("let answer = 42.00;;");

                while (true)
                {
                    var line = System.Console.ReadLine();

                    if (line == "q!") break;

                    engine.Execute(line);
                }
            }

            System.Console.WriteLine(@"Press ENTER to close...");
            System.Console.ReadLine();
        }
    }
}