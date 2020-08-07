using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using evraz.tempobj;


namespace evraz.objtask
{
    /// <summary>
    /// Класс управляющий параллельной обработкой Task участников пробега
    /// </summary>
    public class RunTask
    {
        // список struct InitObjTransp для инициализации списка объектов Transport
        static List<InitObjTransp> lstStructTransp = Init_list_transp.Load_data_from_file();

        // Список объектов Transport 
        static List<Transport> lstTransport = new List<Transport>();


        /// <summary>
        /// Вывод на консоль данных об участниках 
        /// Список участников создается из InitList_transp.Load_data_from_file
        /// В список попадают только те Node, которые прописаны в списке lstNode
        /// Все другие Node игнорируются !!! 
        /// Атрибуты каждого Node используются для инициализации свойств,
        /// которые также игнорируются, если не проходят через switch()...
        /// </summary>
        public static void PrintData_transp()
        {
            lstStructTransp = Init_list_transp.Load_data_from_file();  // загрузка данных участников пробега 


            Console.WriteLine($"      Дистанция три круга по {Transport.CirclePerimeterSize} км.");
            Console.WriteLine();
            Console.WriteLine("       Участники пробега");
            int num = 1;

            #region Отображение списка участников пробега
                foreach (var item in lstStructTransp)
                {
                    string sPrint = string.Empty;
                    string sTransp = string.Empty;
                    switch (item.NameNode)
                    {
                        case "Truck":
                            sTransp = "Грузовик";
                            sPrint = $"{item.indexobj,15} {sTransp,-20} заявлСкор:{item.StatedSpeed,3} весГруза:{item.Weight} вероятнПроколаШины:{item.ProbOccurEvent}";
                            break;
                        case "Car":
                            sTransp = "ЛегкАвто";
                            sPrint = $"{item.indexobj,15} {sTransp,-20} заявлСкор:{item.StatedSpeed,3} кол-во пассажиров:{item.Number_pass} вероятнПроколаШины:{item.ProbOccurEvent}";
                            break;

                        case "Motorcycle":
                            if (item.Sidecar > 0)
                                sTransp = "Мотоцикл с коляской";
                            else
                                sTransp = "Мотоцикл";
                            
                            sPrint = $"{item.indexobj,15} {sTransp,-20} заявлСкор:{item.StatedSpeed,3} вероятнПроколаШины:{item.ProbOccurEvent}";
                            break;
                    }

                    Console.WriteLine($"  {num++}. {sPrint}");
                }

            #endregion
            
        }

        /// <summary>
        /// Заполнение списка транспорта из файла tempobj.config_transp.xml
        /// в формате struct InitObjTransp
        /// </summary>
        static void InitList_transp()
        {
            foreach (InitObjTransp tr in lstStructTransp)
            {
                Transport ob = null;

                switch (tr.NameNode)
                {
                    case "Truck":
                        ob = new Truck(tr);
                        break;
                    case "Car":
                        ob = new Car(tr);
                        break;
                    case "Motorcycle":
                        ob = new Motorcycle(tr);
                        break;
                }


                lstTransport.Add(ob);
               
            }
        }

        /// <summary>        
        /// используется из Program.Main для управления консолью 
        /// значения:
        /// start  команда старта участников пробега
        /// next   команда начать новый старт
        /// ----------------------------------------
        /// иначе -> выход из консоли
        /// </summary>
        public static string strComd = "next";  // управление консолью  

        /// <summary>
        /// Процедура-диспетчер параллельного запуска Task по всем участникам транспорта
        /// </summary>
        /// <returns></returns>
        public static async Task RunTaskExtAsync()
        {

            lstStructTransp.Clear();
            lstTransport.Clear();

            PrintData_transp();  // Заполнение списка участников проекта из *.xml
            if (lstStructTransp.Count == 0)            
                Console.WriteLine("Нет данных по участникам пробега: выход -> любая клавиша+ENTER");            
            else
            {
                InitList_transp();   // инициализация списка трасп. участников пробега 

                Console.WriteLine("---------------------------------------------");
                Console.WriteLine("  Все готово для старта.");
                Console.WriteLine("  Команда для старта: start  выход: любой символ+ENTER");
                Console.Write("   Начать старт: ");

                string sComd = Console.ReadLine();   // считывание команды начать старт:start  выход:ЛюбаяКлавиша
                if (sComd != "start")
                    strComd = "stop";
                else
                {
                    //Стартовый заголовок
                    Console.WriteLine("");
                    Console.WriteLine("---------------------------------------------");
                    Console.WriteLine("               Старт");
                    Console.WriteLine("---------------------------------------------");

                    #region Инициализация локальных переменных

                    List<Task> lst = new List<Task>();          // список задач для параллельного запуска
                    List<string> lstFinish = new List<string>();// список участников дошедших до финиша 
                    List<string> lstErr = new List<string>();   // Список участников выбывших из пробега

                    TimeSpan time = DateTime.Now.TimeOfDay;     // Общий таймер времени 
                    int numFinish;

                    Task res = null;
                    Action<string> rezProgr = s => Console.WriteLine($"{s}"); // Сообщение о ходе работ 

                    #endregion

                    // Форматирование строки для финиша
                    void prResult(Transport arg)
                    {
                        arg.SetTime(time);
                        arg.Mes += $" время:{arg.time,-10:mm\\:ss\\:fff}";

                        lstFinish.Add(arg.Mes);
                    }

                    // Цикл параллельного запуска Task
                    foreach (var tr in lstTransport)
                        lst.Add(Run_testasync.RunTestExt_Async(tr, rezProgr));

                    #region Цикл обработки и анализ данных Task
                    while (lst.Count > 0)
                    {
                        try
                        {
                            res = await Task.WhenAny(lst);

                            // выборка данных по Task
                            foreach (Task<Transport> item in lst)
                            {
                                if (item == res)
                                {
                                    var ob = item.Result;
                                    prResult((Transport)ob);
                                }
                            }

                            lst.Remove(res);

                        }
                        catch (Exception exp)
                        {
                            string sErr = exp.InnerException.Message;
                            lstErr.Add($"{sErr}");      // данные выбывших с дистанции 
                            Console.WriteLine($" {sErr}");
                            lst.Remove(res);
                        }
                    }
                    #endregion

                    #region Блок отображения результатов прохода участниками 
                    Console.WriteLine();
                    Console.WriteLine("Сошли с дистанции:");

                    numFinish = 1;
                    foreach (var item in lstErr)
                        Console.WriteLine($" {numFinish++} {item} ");

                    Console.WriteLine();
                    Console.WriteLine("        Дошли до старта:");

                    numFinish = 1;
                    foreach (var s in lstFinish)
                        Console.WriteLine($"{numFinish++} {s}");

                    // Замыкающее сообщение: остановить/продолжить
                    Console.WriteLine();
                    Console.WriteLine("  Начать новый старт: next     выход:любойСимвол+ENTER");
                    Console.Write("  Введите команду: ");
                    #endregion
                }
            }
        }
        
    }
    
}
