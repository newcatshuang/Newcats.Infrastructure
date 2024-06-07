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

    [DependencyInjectionAttribute(LifetimeEnum.Scoped)]
    public class SendMessage : ISendMessage
    {
        public string Message { get; set; }
    }

    public interface ISendMessage
    {

    }
}
