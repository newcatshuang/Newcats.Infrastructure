using GeneratorApp;

namespace HostApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");
        }
    }

    public class SendMessage : ISendMessage
    {
        public string Message { get; set; }
    }

    [DIAttribute]
    public interface ISendMessage
    {

    }
}
