using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;
using Autodesk.Revit.DB.Plumbing;

namespace HeatingPipeV5
{
    public class TeeData
    {
        public double[,] Value { get; set; }
        public double LocRes { get; set; }
        public bool IsStraight { get; set; }
        public string ElementName { get; set; }
        public PipeSystemType SystemType { get; set; }
        public double Diameter { get; set; }
        public double RelA { get; set; }
        public double RelQ { get; set; }
       
        public TeeData(string name,PipeSystemType pipeSystemType, bool isStraight, double diameter, double relA, double relQ )
        {
            ElementName = name;
            SystemType = pipeSystemType;
            Diameter = diameter;
            RelA = relA;
            RelQ = relQ;
            if (ElementName.Contains("RAUTITAN") || ElementName.Contains("PX"))
            {
                if (SystemType == PipeSystemType.SupplyHydronic && IsStraight == true)
                {
                    Value = new double[,]
                    {
                    {16,20,25,32,40,50,63 },
                    {1,0.9,1.1,0.9,1,0.5,0.4}
                    };

                }
                else if (SystemType == PipeSystemType.SupplyHydronic && IsStraight == false)
                {
                    Value = new double[,]
                    {
                    { 16,20,25,32,40,50,63 },
                    { 3.8,3.6,4.4,3.8,4.2,2.6,2.4}
                    };

                }
                else if (SystemType == PipeSystemType.ReturnHydronic && IsStraight == true)
                {
                    Value = new double[,]
                    {
                    {16,20,25,32,40,50,63 },
                    { 17.3,13.5,16.4,12.2,14.2,7.8,7.1}
                    };

                }
                else if (SystemType == PipeSystemType.ReturnHydronic && IsStraight == false)
                {
                    Value = new double[,]
                    {
                    {16,20,25,32,40,50,63 },
                    { 9,8,8.6,6.3,7.2,4.1,3.8}
                    };

                }
            }
            else if (ElementName.Contains("Исполнение") || ElementName.Contains("Оцинкованные"))
            {
                if (SystemType == PipeSystemType.SupplyHydronic && IsStraight == true)
                {
                    Value = new double[,]
                    {
                         {0,0.1,0.2,0.3,0.4,0.5,0.6,0.7,0.8,0.9,1 },
                         {1,90,25,12.5,1.75,5.4,4.15,3.3,2.8,2.55,2.3 },
                         {0.74,33,9.4,4.7,3,2,1.5,1.2,1.05,1,1 },
                         {0.66,23,6.88,3.52,2.25,1.6,1.25,1,0.9,0.9,0.9 },
                         {0.59,13.5,4.73,2.88,1.8,1.34,1.1,0.95,0.83,0.8,0.8 },
                         {0.52,10,3.3,1.88,1.3,1,0.83,0.72,0.67,0.65,0.65 },
                         {0.44,5.3,1.9,1.4,0.8,0.6,0.52,0.5,0.5,0.5,0.5 },
                         {0.3,2.1,0.935,0.542,0.4,0.305,0.25,0.215,0.2,0.2,0.2 }
                    };

                }
                else if (SystemType == PipeSystemType.SupplyHydronic && IsStraight == false)
                {
                    Value = new double[,]
                    {
                         {0,0.1,0.2,0.3,0.4,0.5,0.6,0.7,0.8,0.9,1 },
                         {1,100,29,14.5,9,6.3,4.75,3.8,3.1,2.66,2.3 }

                    };

                }
                else if (SystemType == PipeSystemType.ReturnHydronic && IsStraight == true)
                {
                   
                    Value = new double[,]
                   {
                      {0,0.1,0.2,0.3,0.4,0.5,0.6,0.7,0.8,0.9,1 },
                      {1,-65,-10,-2.66,0.625,2,2.3,2.3,2.3,2.3,2.3 },
                      {0.74,-25,-3.62,0,1,1.4,1.6,1.8,1.9,2,2 },
                      {0.66,-15.2,-1.29,0.55,1,1.2,1.4,1.55,1.7,1.73,1.73 },
                      {0.59,-8,0.3,0.8,1,1.1,1.2,1.3,1.4,1.4,1.5 },
                      {0.52,-5,0.4,0.9,1,1.1,1.2,1.25,1.3,1.32,1.34 },
                      {0.44,-2,0.5,0.9,1,1.1,1.2,1.2,1.2,1.2,1.2 },
                      {0.3,-0.4,0.6,0.9,1,1.65,1.1,1.1,1.1,1.1,1.1 }
                   };

                }
                else if (SystemType == PipeSystemType.ReturnHydronic && IsStraight == false)
                {
                    Value = new double[,]
                    {
                        {0,0.1,0.2,0.3,0.4,0.5,0.6,0.7,0.8,0.9,1 },
                        {1, 80,21.5,11,7,5,3.9,3.25,2.8,2.5,2.3 }
                    };

                }
            }    
           
            

           
        }

        public double GetLocRes()
        {

            if (ElementName.Contains("RAUTITAN") || ElementName.Contains("PX"))
            {
                int numRows = Value.GetLength(1);

                // Check for exact match
                for (int i = 0; i < numRows; i++)
                {
                    if (Value[0, i] == Diameter)
                    {
                        LocRes = Value[1, i];
                        return LocRes;
                    }
                }



            }
            else if (ElementName.Contains("Исполнение") || ElementName.Contains("Оцинкованные"))
            {
                double result = 0;
                List<int> indexA = new List<int>();
                List<int> indexB = new List<int>();
                List<int> indexC = new List<int>();

                // Поиск индексов для RelQ
                for (int j = 1; j < Value.GetLength(1); j++)
                {
                    if (RelQ > Value[0, j - 1] && RelQ <= Value[0, j])
                    {
                        indexB.Add(j);
                    }
                }

                // Поиск индексов для RelA
                for (int k = 1; k < Value.GetLength(0); k++)
                {
                    if (RelA > Value[k - 1, 1] && RelA < Value[k, 1])
                    {
                        indexC.Add(k);
                    }
                    else if (RelA <= Value[k, 1]) // если RelA меньше или равно
                    {
                        indexC.Add(k);
                        RelA = Math.Round(RelA, 0); // округляем RelA
                        break; // выходим из цикла, раз нашли подходящее значение
                    }
                }

                // Проверка индексов на наличие значений
                if (indexB.Count == 0 || indexC.Count == 0)
                {
                    return double.NaN; // если нет подходящих индексов, возвращаем NaN
                }

                // Интерполяция для найденных индексов
                for (int i = 0; i < indexC.Count; i++)
                {
                    int k = indexC[i];

                    for (int j = 0; j < indexB.Count; j++)
                    {
                        int b = indexB[j];

                        if (RelQ == Value[0, b])
                        {
                            return Value[k, b]; // возвращаем значение, если точное совпадение
                        }

                        double x0 = Value[0, b - 1];
                        double x1 = Value[0, b];
                        double y0 = Value[k - 1, b];
                        double y1 = Value[k, b];

                        // Линейная интерполяция
                        result = (y0 + (RelQ - x0) / (x1 - x0) * (y1 - y0));
                        return result; // возвращаем результат
                    }
                }

                // Обработка других случаев: интерполяция по двум переменным
                for (int k = indexC.Min(); k < indexC.Max(); k++)
                {
                    for (int j = indexB.Min(); j < indexB.Max(); j++)
                    {
                        double A1 = Value[k - 1, 1];
                        double A2 = Value[k, 1];
                        double A = RelA;
                        double B1 = Value[0, j - 1];
                        double B2 = Value[0, j];
                        double B = RelQ;

                        double C11 = Value[k - 1, j - 1];
                        double C12 = Value[k - 1, j];
                        double C21 = Value[k, j - 1];
                        double C22 = Value[k, j];

                        double res1 = (((B2 - B) / (B2 - B1) * C11) + (B - B1) / (B2 - B1) * C12) * ((A2 - A) / (A2 - A1));
                        double res2 = (((B2 - B) / (B2 - B1) * C21) + (B - B1) / (B2 - B1) * C22) * (A - A1) / (A2 - A1);
                        result = res1 + res2;
                        LocRes = result;
                        return LocRes; // возвращаем результирующее значение
                    }
                }


            }
            return LocRes;
        }
    }
}
