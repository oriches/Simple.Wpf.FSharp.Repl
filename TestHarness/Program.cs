namespace TestHarness
{
    using System;
    using Simple.Wpf.FSharp.Repl;

    class Program
    {
        static void Main(string[] args)
        {
            using (var engine = new ReplEngine())
            {
                engine.Output.Subscribe(Console.WriteLine);

                engine.Start("let answer = 42.00");

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
