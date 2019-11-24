using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Simple.Wpf.FSharp.Repl.UI.Extensions
{
    internal static class AsyncExtension
    {
        public static Task<string> ReadLineAsync(this StreamReader streamReader, CancellationToken token)
        {
            return Task.Factory.StartNew(() => streamReader.ReadLine(), token);
        }

        public static Task<int> ReadAsync(this StreamReader streamReader, CancellationToken token)
        {
            return Task.Factory.StartNew(() => streamReader.Read(), token);
        }
    }
}