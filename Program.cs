using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using evraz.objtask; 

namespace evraz
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("  Тестовое задание компании \"ЕвразТехника\"");
            Console.WriteLine("  Исполнитель Владимир Шаршов");
            Console.WriteLine("********************************************");

            while (RunTask.strComd == "next")
            {
                RunTask.RunTaskExtAsync();

                if (RunTask.strComd == "next")
                    RunTask.strComd = Console.ReadLine();

            }
        }
    }
}
