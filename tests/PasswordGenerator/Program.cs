using System.Text;

namespace PasswordGenerator
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("==========================密码生成器[Newcats-2024/08/28]==========================");
            Console.Write("请输入需要的密码位数：");
            string inputLength = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(inputLength))
                inputLength = "16";
            if (!int.TryParse(inputLength, out int length))
            {
                length = 16;
            }

            for (int i = 0; i < 10; i++)
            {
                string password = GetRandomKey(length);
                Console.WriteLine(password);
                Console.WriteLine();
            }

            Console.ReadKey();
        }

        /// <summary>
        /// 获取指定长度的随机数字/字母/特殊字符组成的字符串(Salt)
        /// </summary>
        /// <param name="length">字符串长度</param>
        private static string GetRandomKey(int length)
        {
            char[] arrChar = new char[]{
           'a','b','d','c','e','f','g','h','i','j','k','l','m','n','p','r','q','s','t','u','v','w','z','y','x',
           '0','1','2','3','4','5','6','7','8','9',
           'A','B','C','D','E','F','G','H','I','J','K','L','M','N','Q','P','R','T','S','V','U','W','X','Y','Z',
           '!','@','#','$','%','^','&','*','(',')','_','+','=','|','.',',','/'
          };

            StringBuilder num = new();

            for (int i = 0; i < length; i++)
            {
                num.Append(arrChar[Random.Shared.Next(0, arrChar.Length)].ToString());
            }
            return num.ToString();
        }
    }
}