using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.Distributions;

namespace evraz.tempobj
{
    /// <summary>
    /// Исходные условия: 
    ///     Общая дистанция складывается из нескольких кругов
    ///     заложено 3 круга
    /// </summary>


    // Тип транспСредства
    public enum Enm_typeTransp { not_data, truck, car, motorcycle, motorcycleSidecar }

    /// <summary>
    /// Размещения данных по событию прокола шины
    /// </summary>
    public struct Struct_EventAccident
    {
        public int numCircl;    // на каком круге произойдет событие прокола шины
        public bool eventExist; // true событие детерминировано Такой участник обречен !!!
        public int timeKwant;   // квант времени на устранение прокола шины
    }

    // Базовый класс для всех видов транспорта
    public abstract class Transport
    {

    #region ********* статические поля класса **********
        // Кол-во кругов -> для упрощения значение из прогрКода
        static readonly int _Quantity_of_circles = 3;
        public static int Quantity_of_circles { get { return _Quantity_of_circles; } }

        // Размер периметра круга в км
        private static int _CirclePerimeterSize = 40;
        public static int CirclePerimeterSize {
            get { return _CirclePerimeterSize; }
            set { _CirclePerimeterSize = value; } }

        // Значение вероятности прокола шины по умолчанию 
        public static double probOccurEvent_def = 0.05;

        #endregion


        #region ******* Свойства объектов ******


        public static Struct_EventAccident Get_EvenAccident(Transport argTransp)
        {
            Struct_EventAccident ev = new Struct_EventAccident();
            ev.eventExist = false;

            Exponential obExp = new Exponential(3);
            for (var i = 1; i < 4; i++)
            {
                var val_objExp = Math.Round(obExp.Sample(), 3);
                if (argTransp.ProbOccurEvent > val_objExp)
                {
                    ev.eventExist = true;
                    ev.timeKwant = argTransp.Dist * 3000 / (int)argTransp.StatedSpeed;
                    ev.numCircl = i;
                    break;
                }
            }
            
            return ev; 
        }
                    
        public Struct_EventAccident EvenAccident { get; set; }


        public TimeSpan time { get; set; }
        public void SetTime(TimeSpan startTime)
        {
            time = DateTime.Now.TimeOfDay - startTime;
        }

        public int Dist { get { return CirclePerimeterSize; } }

        public string Mes { get; set; }


        // index of object
        public string Indexobj { get; set; }


        // Заявленная скорость на марше км/час
        public int StatedSpeed { get; set; }

        // вероятность прокола шины
        private double _probOccurEvent = probOccurEvent_def; // оптимальное значение
        public double ProbOccurEvent {
            get { return _probOccurEvent; }
            set { if (value < 0.07 && value > 0.01) _probOccurEvent = value; } }


        public static Enm_typeTransp convStr_into_typeTransp(string arg)
        {
            Enm_typeTransp res = Enm_typeTransp.not_data;

            switch (arg)
            {
                case "Truck":
                    res = Enm_typeTransp.truck;
                    break;
                case "Car":
                    res = Enm_typeTransp.car;
                    break;
                case "Motorcycle":
                    res = Enm_typeTransp.motorcycle;
                    break;
                case "MotorcycleSidecar":
                    res = Enm_typeTransp.motorcycleSidecar;
                    break;
            }

            return res; 
        }


        // типТранспортного средства
        public Enm_typeTransp TypeTransp { get; set; }


        // Вес 1000 кг.
        public double Weight { get; set; }

        // Время на замену шины -> условная величина равная (Vmax/Sdist) * 5 
        // т.е. пятиКратное значение от времени прохода ВСЕЙ дистанции на максСкорости
        //private double _RepairTime;
        public int RepairTime { get; set; }
       
        
        // Квант времени для прохода одного круга 
        public int KwantTime { get; set; }


    #endregion


    #region ****** свойства модели из MathNet.Numerics.Distributions *******

        // Распределение по Гауссу. Объект используется для каждого транспСредства
        // отдельно
        public Normal NormalDistr { get; set; }


        // Экспоненциальное распределение
        // для определения вероятности прокола шины 
        // привязка на 3 круга. Где произойдет -> расчетноеЗначение по ProbOccurEvent
        private static Exponential _ExpDistr = new Exponential(3);
        public static Exponential ExpDistr { get { return _ExpDistr; } }

    #endregion

    }

    // структура для инициализации объектов транспорта 
    public struct InitObjTransp
    {
        public Enm_typeTransp TypeTransp;   // тип транспСредства

        public double ProbOccurEvent;       // значение вероятности прокола шины (0,05)
        public double Weight;               // вес грузоваика, легковушки 
        public int StatedSpeed;             // заявленная скрость км/час

        public int Number_pass;             // кол-во пассажиров для легковушки
        public int Sidecar;                 // Признак наличия коляски 
        public string NameNode;             // идентификатор xmlNode 
        public string indexobj;             // Строковый идентификатор объекта
    }

    /// <summary>
    /// Класс грузовика
    /// </summary>
    public class Truck: Transport
    { 
        public Truck(InitObjTransp parmTransp)
        {            
            ProbOccurEvent  = parmTransp.ProbOccurEvent;
            TypeTransp      = parmTransp.TypeTransp;
            Weight          = parmTransp.Weight > 0 ? parmTransp.Weight: 30000;
            StatedSpeed     = parmTransp.StatedSpeed > 0 ? parmTransp.StatedSpeed : 50 ;
            Indexobj        = parmTransp.indexobj ;

            // --------------------- 
            KwantTime       = CirclePerimeterSize * 3000 / (int)this.StatedSpeed;
            RepairTime      = (CirclePerimeterSize * 5) / StatedSpeed;

            NormalDistr     = new Normal(StatedSpeed, (StatedSpeed * 0.1));

            EvenAccident = Transport.Get_EvenAccident(this);
        }

    }


    /// <summary>
    /// Класс легковушки
    /// </summary>
    public class Car : Transport
    {
        private int _Number_pass;
        public int Number_pass {
            get { return _Number_pass; }
            set {
                if (value > 0 && value < 5 )
                    _Number_pass = value;
                else
                    _Number_pass = 1; 
            } }

        public Car(InitObjTransp parmTransp)
        {

            ProbOccurEvent  = parmTransp.ProbOccurEvent;
            TypeTransp      = parmTransp.TypeTransp;
            Weight          = parmTransp.Weight;
            StatedSpeed     = parmTransp.StatedSpeed;
            Number_pass     = parmTransp.Number_pass;
            Indexobj        = parmTransp.indexobj;

            // --------------------- 
            KwantTime       = CirclePerimeterSize * 3000 / (int)this.StatedSpeed;
            RepairTime      = (CirclePerimeterSize * 5) / StatedSpeed;

            NormalDistr     = new Normal(StatedSpeed, (StatedSpeed * 0.1));

            EvenAccident = Transport.Get_EvenAccident(this);
        }
        
    }


    /// <summary>
    /// Класс Мотоцикла
    /// Наличие/отсутствие коляски определяется свойством Sidecar
    /// </summary>
    public class Motorcycle : Transport
    {
        public bool Sidecar { get; set; }

        public Motorcycle(InitObjTransp parmTransp)
        {
            ProbOccurEvent  = parmTransp.ProbOccurEvent;
            TypeTransp      = parmTransp.TypeTransp;
            Weight          = parmTransp.Weight;
            StatedSpeed     = parmTransp.StatedSpeed;
            Indexobj        = parmTransp.indexobj;

            // --------------------- 
            KwantTime       = CirclePerimeterSize * 3000 / (int)this.StatedSpeed;
            RepairTime      = (CirclePerimeterSize * 5) / StatedSpeed;
            Sidecar         = parmTransp.Sidecar> 0 ;

            NormalDistr     = new Normal(StatedSpeed, (StatedSpeed * 0.1));

            EvenAccident = Transport.Get_EvenAccident(this);
        }

        
    }


}
