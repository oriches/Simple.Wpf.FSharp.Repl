namespace Console.TestHarness
{
    using System;
    using System.Diagnostics;
    using Simple.Wpf.FSharp.Repl;

    class Program
    {
        static void Main(string[] args)
        {
            using (var engine = new ReplEngine())
            {
                engine.State.Subscribe(x => Debug.WriteLine("state = " + x.ToString()));
                engine.Output.Subscribe(Console.Write);
                engine.Start("let answer = 42.00;;");

                while (true)
                {
                    var line = Console.ReadLine();

                    if (line == "q!")
                    {
                        break;
                    }

                    engine.Execute(line);
                }
            }

            Console.WriteLine("Press ENTER to close...");
            Console.ReadLine();
        }
    }
}
