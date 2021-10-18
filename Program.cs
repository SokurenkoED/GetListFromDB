using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetListFromDB
{
    class Program
    {
        private static void GetDataFromNPP(string Path, ref List<string> StringArray)
        {
            try
            {
                Encoding ANSI = Encoding.GetEncoding(1251);
                using (StreamReader sr = new StreamReader(Path, ANSI))
                {
                    string Line;
                    int k = 0;
                    while ((Line = sr.ReadLine()) != null)
                    {
                        if (k > 3)
                        {
                            string[] StrArr = Line.Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
                            StringArray.Add($"{StrArr[0]}{"\t"}{StrArr[2]}{"\t"}{StrArr[3]}");
                        }
                        k++;
                    }
                }
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("Файл" + Path + " не был найден");
            }
        }

        private static void GetDataFromIC(string Path, ref List<string> StringArray)
        {
            try
            {
                using (StreamReader sr = new StreamReader(Path))
                {
                    string Line;
                    while ((Line = sr.ReadLine()) != null)
                    {
                        string[] ArrOfStr = Line.Split(new string[] { "\t" }, StringSplitOptions.RemoveEmptyEntries);
                        if (ArrOfStr.Length > 1)
                        {
                            if (StringArray.Any(w => w == ArrOfStr[1])) // Проверяю на повтор элемента
                            {
                                throw new Exception($"{"Элемент"} {ArrOfStr[1]} {"повторяется"}");
                            }
                            StringArray.Add(ArrOfStr[1]);
                        }
                        else
                        {
                            if (StringArray.Any(w => w == Line)) // Проверяю на повтор элемента
                            {
                                throw new Exception($"{"Элемент"} {Line} {"повторяется"}");
                            }
                            StringArray.Add(Line);
                        }
                        
                    }
                }
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("Файл" + Path + " не был найден");
            }
        }

        private static void WriteDataToIC(string WorkPath, List<string> StringArrayFromIC, ref List<string> StringArrayFromNPP)
        {
            using (StreamWriter sw = new StreamWriter(WorkPath, false, Encoding.Default))
            {
                IFormatProvider formatter = new NumberFormatInfo { NumberDecimalSeparator = "." };
                for (int i = 0; i < StringArrayFromIC.Count; i++)
                {
                    var result = StringArrayFromNPP.Where(tmp => tmp.IndexOf(StringArrayFromIC[i] + "\t") >= 0);
                    if (result.Count() == 0)
                    {
                        throw new Exception($"{"Элемент"} {StringArrayFromIC[i]} {"не найден"}");
                    }
                    double d;
                    foreach (var item in result)
                    {
                        string[] ArrOfStr = item.Split(new string[] { "\t" }, StringSplitOptions.RemoveEmptyEntries);
                        if (ArrOfStr[2] == "дост")
                        {
                            if (double.TryParse(ArrOfStr[1], NumberStyles.Number, formatter, out d))
                            {
                                sw.WriteLine($"{ArrOfStr[1]}{"\t"}{ArrOfStr[0]}{"\t"}{"("}{i + 1}{")"}");
                            }
                            else
                            {
                                sw.WriteLine($"{9999997}{"\t"}{ArrOfStr[0]}{"\t"}{"("}{i + 1}{")"}");
                            }
                        }
                        else
                        {
                            if (double.TryParse(ArrOfStr[1], NumberStyles.Number, formatter, out d))
                            {
                                if (d >= 0)
                                {
                                    sw.WriteLine($"{9999999}{"\t"}{ArrOfStr[0]}{"\t"}{"("}{i + 1}{")"}");
                                }
                                else
                                {
                                    sw.WriteLine($"{-9999999}{"\t"}{ArrOfStr[0]}{"\t"}{"("}{i + 1}{")"}");
                                }
                            }
                            else
                            {
                                sw.WriteLine($"{9999998}{"\t"}{ArrOfStr[0]}{"\t"}{"("}{i + 1}{")"}");
                            }
                        }
                    }
                }
            }
        }

        static void Main(string[] args)
        {
            Stopwatch sWatch = new Stopwatch();
            sWatch.Start();
            
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            

            string ReadPathFromNPP = "Data.txt";
            string WorkPath = "!List_IC.txt";

            List<string> StringArrayFromNPP = new List<string>();
            List<string> StringArrayFromIC = new List<string>();

            GetDataFromNPP(ReadPathFromNPP, ref StringArrayFromNPP);

            GetDataFromIC(WorkPath, ref StringArrayFromIC);

            WriteDataToIC(WorkPath, StringArrayFromIC,ref StringArrayFromNPP);
 
            sWatch.Stop();
            Console.WriteLine(sWatch.ElapsedMilliseconds.ToString());
        }
    }
}
