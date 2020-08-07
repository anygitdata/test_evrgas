using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.Distributions;
using evraz.tempobj;

namespace evraz.objtask
{
    /// <summary>
    /// Класс обработки асинхронных Task
    /// </summary>
    public class Run_testasync
    {
        public static async Task<Transport> RunTestExt_Async(Transport arg, Action<string> procMesProgr)
        {
            int dist = arg.Dist; // Размер периметра круга 

            double maxSpeed = arg.StatedSpeed;   // заявленная скорость на марше для транспСредства
            Normal obSpeed = null;
            double curSpeed;
            int timeKwant;
            int index = 1;

            for (var data = index; data <= 3; data++)
            {
                // Принудительный вызов Exception для демонстрации обработки события 
                if (arg.Indexobj == "trunct_02" && index==3)
                    throw new Exception($"{arg.Indexobj}: Поломка транспорта");


                obSpeed = new Normal(maxSpeed, (maxSpeed * 0.1));
                curSpeed = obSpeed.Sample();

                timeKwant = dist * 3000 / (int)curSpeed;
                                
                // Проверка реализации вероятности прокола шины
                if (arg.EvenAccident.eventExist && arg.EvenAccident.numCircl == index)
                {
                    timeKwant = arg.EvenAccident.timeKwant;
                    Console.WriteLine($"  {arg.Indexobj} повреждение шины");
                }

                await Task.Run(() => Task.Delay(timeKwant));

                string sProgr = $"     {arg.Indexobj,15} прошел {index++} круг; срСкор:{(int)maxSpeed,3} фактСкор:{(int)curSpeed,3}";

                procMesProgr(sProgr);
            }


            arg.Mes = $"{arg.Indexobj,15} срСкор:{arg.StatedSpeed,3}";

            return arg;
        }
    }
}
