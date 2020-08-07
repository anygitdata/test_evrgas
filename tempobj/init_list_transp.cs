using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;


namespace evraz.tempobj
{   
    public class Init_list_transp
    {
        public static List<InitObjTransp> Load_data_from_file()
        {
            // Базовый список строковых идентификатор транспорта
            List<string> lstNode = new List<string> { "Truck", "Car", "Motorcycle" };
            List<InitObjTransp> _listTranp = new List<InitObjTransp>();

            // Загрузить данные транспорта
            XmlDocument doc = new XmlDocument();

            string sFile = System.IO.Directory.GetCurrentDirectory() + "\\tempobj\\config_transp.xml";

            doc.Load(sFile);
            XmlNode nodes = doc.SelectSingleNode("//lst_transp");   // Узел, содержащий транспорт участников
            double probOccurEvent_def = Transport.probOccurEvent_def;

            int CirclePerimeterSize = int.Parse(nodes.Attributes[0].Value);
            Transport.CirclePerimeterSize = CirclePerimeterSize; 

            foreach (XmlNode n in nodes.ChildNodes)
            {                
                InitObjTransp obj = new InitObjTransp();

                try
                {
                    if (lstNode.Contains(n.Name))  // Только те, которые находятся в базовомСписке lstNode
                    {
                        foreach (XmlAttribute att in n.Attributes)
                        {
                            switch (att.Name)
                            {
                                case "StatedSpeed":
                                    obj.StatedSpeed = int.Parse(att.Value);
                                    break;
                                case "ProbOccurEvent":
                                    obj.ProbOccurEvent = double.Parse(att.Value);
                                    break;
                                case "Weight":
                                    obj.Weight = int.Parse(att.Value);
                                    break;
                                case "Number_pass":
                                    obj.Number_pass = int.Parse(att.Value);
                                    break;
                                case "Sidecar":
                                    obj.Sidecar = int.Parse(att.Value);
                                    break;
                                case "indexobj":
                                    obj.indexobj = att.Value;
                                    break;
                            }
                        }

                        obj.NameNode = n.Name;


                        if (obj.Sidecar > 0 && n.Name == "Motorcycle")  // учет наличия коляски мотоцикла 
                            obj.TypeTransp = Transport.convStr_into_typeTransp(obj.NameNode + "Sidecar");
                        else
                            obj.TypeTransp = Transport.convStr_into_typeTransp(obj.NameNode);

                        // Верификация базовых параметров
                        if (obj.ProbOccurEvent == 0) obj.ProbOccurEvent = probOccurEvent_def;

                        if (obj.NameNode == "Truck")   // Вес груза 
                        {
                            if (obj.Weight == 0) obj.Weight = 1000;
                        }

                        if (obj.NameNode == "Car")  // кол-во пассажиров в легкАвто
                        {
                            if (obj.Number_pass == 0) obj.Number_pass = 1;
                        }

                        if (obj.StatedSpeed == 0) obj.StatedSpeed = 70; // заявленная скорость


                        _listTranp.Add(obj);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка загрузки атрибутов для транспорта {n.Name}");
                }

            }


            return _listTranp;
        }




    }
}
