using Newcats.DependencyInjection;

namespace HostApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");
        }
    }

    [DependencyInjection]
    public class SendMessage : ISendMessage
    {
        public string Message { get; set; }
    }

    public interface ISendMessage
    {

    }
}
