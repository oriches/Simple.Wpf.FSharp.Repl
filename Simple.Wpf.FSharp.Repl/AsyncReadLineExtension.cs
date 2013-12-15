namespace Simple.Wpf.FSharp.Repl
{
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    internal static class AsyncReadLineExtension
    {
        public static Task<string> ReadLineAsync(this StreamReader streamReader, CancellationToken token)
        {
            return Task.Factory.StartNew(() => streamReader.ReadLine(), token);
        }
    }
}