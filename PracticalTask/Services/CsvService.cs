using System;
using System.IO;
using System.Text;

namespace PracticalTask.Services
{
    public class CsvService
    {
        public void Export<TArray>(string path, TArray[] array)
        {
            StringBuilder sb = new StringBuilder();
            
            for (int i = 0; i < array.Length - 1; i++)
            {
                sb.AppendLine(string.Format("{0};{1}", i, array[i]));
            }

            try
            {
                File.WriteAllText(path, sb.ToString());
            }
            catch (IOException e)
            {
                Console.WriteLine("Файл {0} занят другим процессом. Сохранение не удалось.", path);
            }
        }
        
        public void Export<TArray>(string path, TArray[,] array)
        {
            StringBuilder sb = new StringBuilder();
            
            for (int i = 0; i < array.GetLength(1); i++)
            {
                for (int j = 0; j < array.GetLength(0) ; j++)
                {
                    sb.Append(string.Format("{0};{1}; ;", i, array[j, i]));
                }

                sb.AppendLine();
            }
            
            try
            {
                File.WriteAllText(path, sb.ToString());
            }
            catch (IOException e)
            {
                Console.WriteLine("Файл {0} занят другим процессом. Сохранение не удалось.", path);
            }
        }

        public void Import()
        {
            throw new NotImplementedException();
        }
    }
}