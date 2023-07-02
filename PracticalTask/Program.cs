using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using _AVM.Library.Core;
using PracticalTask.Enums;
using PracticalTask.Interfaces;
using PracticalTask.Services;

namespace PracticalTask
{
    class Program
    {
        private static readonly IMathService MathService = new MathService();
        private static readonly CsvService CsvService = new CsvService();

        #region Parameters

        private static readonly int Percent = 3; //default is 3.
        private static readonly int FilterSensitivity = 2; //default is 12.

        #endregion
        
        //Загрузка ROI (5.1).
        private static string _inputImage = string.Empty;
        
        private static void Main()
        {
            do
            {
                Console.WriteLine("Enter the path to the image");
                _inputImage = Console.ReadLine();
            } while (!File.Exists(_inputImage));

            Image._8bit originalImage = new Image._8bit(_inputImage);
            ImageService redactImage = new ImageService(originalImage, MathService);

            #region [Определение текущей ориентации острого края на изображении (5.2)]
            
            var orientationInputData = redactImage.GetEdgeOrientation();
            
            switch (orientationInputData)
            {
                case OrientationEnum.RightUp:
                    redactImage.Rotate(270);
                    break;
                case OrientationEnum.RightDown:
                    redactImage.Rotate(90);
                    redactImage.FlipVertical();
                    break;
                case OrientationEnum.LeftUp:
                    redactImage.Rotate(90);
                    redactImage.FlipHorizontal();
                    break;
                case OrientationEnum.LeftDown:
                    redactImage.Rotate(90);
                    break;
                case OrientationEnum.TopRight:
                    redactImage.FlipHorizontal();
                    break;
                case OrientationEnum.TopLeft:
                    break;
                case OrientationEnum.BottomRight:
                    redactImage.Rotate(180);
                    break;
                case OrientationEnum.BottomLeft:
                    redactImage.FlipVertical();
                    break;
            }

             
            originalImage = redactImage.GetImageCopy();

            #endregion

            #region [Построение диаграммы яркости, вычисление динамического диапазона (5.3)]
            
            var histogram = redactImage.Histogram;
            int percent = histogram.Max() / 100 * Percent;
            int startRange = -1, endRange = -1;

            for (int start = 1, end = histogram.Length - 2; 
                 !(startRange != -1 && endRange != -1);
                 start++, end--)
            {
                if (histogram[start - 1] <= percent && histogram[start] >= percent)
                {
                    int closerNum = start > 0
                        ? MathService.GetCloserNum(percent, false, histogram[start], histogram[start - 1])
                        : histogram[start];
                    
                    startRange = closerNum >= percent ? start : start - 1;
                    start--;
                }

                if (histogram[end + 1] <= percent && histogram[end] >= percent)
                {
                    int closerNum = end < histogram.Length - 1
                        ? MathService.GetCloserNum(percent, false, histogram[end], histogram[end + 1])
                        : histogram[end];
                    
                    endRange = closerNum >= percent ? end : end + 1;
                    end++;
                }
            }
            
            #endregion

            #region [Бинаризация изображения (5.4)]
            
            redactImage.Binarize(startRange, endRange, 60);
            
            if (!redactImage.IsBinarized)
            {
                Console.WriteLine("Ошибка бинаризации.\nНажмите любую клавишу для завршения программы.");
                redactImage.Save("BinarizedError.bmp");
                return;
            }
            
            #endregion

            #region [Фильтрация изображения (5.5)]
            
            for (int i = FilterSensitivity; i > 0; i /= 2)
            {
                int sensitivity = i <= 3 ? 75 : 65;
                redactImage.CleaningNoise(i, percentSensitivity:sensitivity);
            }
            
            #endregion

            #region [Метод наименьших квадратов (5.6)]
            
            int[] countBlackByColumns = new int[redactImage.Size.Width];

            for (int x = 0; x < redactImage.Size.Width; x++)
            {
                for (int y = 0; y < redactImage.Size.Height; y++)
                {
                    if (redactImage[x,y] == 0)
                    {
                        countBlackByColumns[x]++;
                    }
                }
            }

            var leastSquaresRes = MathService.LeastSquares(countBlackByColumns.Length - 1, countBlackByColumns);
            
            #endregion

            #region [Рисуем функцию y = ax + b]
            
            Func<int, int> functionLine = x => (int) (-leastSquaresRes.Item1 * x + leastSquaresRes.Item2);
            redactImage.DrawFunction(functionLine, 255);

            for (int x = 0; x < redactImage.Size.Width; x++)
            {
                //int y = (int)(leastSquaresRes.Item1 * x + leastSquaresRes.Item2);
                //originalImage[x, y] = 255;
            }

            //originalImage.Save("redact.bmp");
            
            #endregion

            #region [Продолжаем работать с входным изображением. Определение субдискретизованных функций края ESFm (5.7)]
            
            int xp = (int)(1 / Math.Abs(leastSquaresRes.Item1)); //Xp
            int amountEsf = countBlackByColumns.Length / xp; //M
            int sizeEsf = redactImage.Size.Height * xp;
            
            byte[,] esfFunctions = new byte[amountEsf, sizeEsf];

            for (int y = 0; y < originalImage.Size.Height; y++)
            {
                for (int x = 0; x < amountEsf * xp; x++)
                {
                    int esfIndex = (x / xp);

                    int xOffset = esfIndex * xp;

                    int yOffset = y * xp;

                    int esfValueIndex = x - xOffset + yOffset;

                    esfFunctions[esfIndex, esfValueIndex] = originalImage[x, y];
                }
            }

            #endregion

            #region [Получение суммарной функции ESF (5.8)]
            
            float[] sumEsf = new float[sizeEsf - (amountEsf * xp)];

            for (int i = amountEsf * xp; i < sumEsf.Length; i++)
            {
                for (int j = 1; j < amountEsf + 1; j++)
                {
                    sumEsf[i] += esfFunctions[j - 1, i - (j - 1) * xp];
                }
            }
            
            sumEsf = MathService.MovingAverage(sumEsf, sumEsf.Length , xp);

            #endregion

            #region [Получение LSF путём дифференцирования ESF (5.9)]
            
            float[] lsf = MathService.DifferentiationFunc(sumEsf, xp / 2);
            
            var lsfMaxAmplitude = float.MinValue;
            var lsfMaxAmplitudeIndex = 0;

            for (int i = 0; i < lsf.Length; i++)
            {
                if (Math.Abs(lsf[i]) > lsfMaxAmplitude)
                {
                    lsfMaxAmplitude = lsf[i];
                    lsfMaxAmplitudeIndex = i;
                }
            }
            
            var lsfMin60 = (lsfMaxAmplitude / 100) * 30;
            int n1 = -1, n2 = 11600;

            for (int i = lsfMaxAmplitudeIndex; i > lsf.Length / 4; i--)
            {
                var firstPoint = Math.Abs(lsf[i]);
                var secondPoint = Math.Abs(lsf[i - 1]);

                if (lsfMin60 <= firstPoint && lsfMin60 >= secondPoint)
                {
                    n1 = i;
                    break;
                }
            }

            for (int i = lsfMaxAmplitudeIndex; i < (lsf.Length / 2) + lsf.Length / 4; i++)
            {
                var firstPoint = Math.Abs(lsf[i]);
                var secondPoint = Math.Abs(lsf[i - 1]);

                if (lsfMin60 <= firstPoint && lsfMin60 >= secondPoint)
                {
                    n2 = i;
                    break;
                }
            }

            var thresholdLeft = lsfMaxAmplitudeIndex - (n2 - n1) * 4;
            var thresholdRight = lsfMaxAmplitudeIndex + (n2 - n1) * 4;

            float[] lsfThreshold = new float[thresholdRight - thresholdLeft];

            for (int i = thresholdLeft; i < thresholdRight; i++)
            {
                lsfThreshold[i - thresholdLeft] = lsf[i];
            }

            #endregion

            #region [Построение окна Хемминга W(y), сглаживание функции LSF (5.10)]
            
            float[] hammingWindow = MathService.FillingArrayByFunc(
                x =>(float) (0.54 + 0.46 * Math.Cos(2 * Math.PI * (x - lsfThreshold.Length / 2.0) /
                                                    lsfThreshold.Length)), lsfThreshold.Length );
            
            float[] lsfW = MathService.FillingArrayByFunc(x => hammingWindow[(int) x] * lsfThreshold[(int) x], hammingWindow.Length);

            #endregion

            #region [Расчёт exp - SFR(k), получение ЧКХ (MTF) (5.11)]
            
            float[] mtf = new float[100];

            for (int k = 1; k <= 100; k++)
            {
                float step = k / (float)xp;

                float num = (float)(1 / Math.Sin(Math.PI * step / hammingWindow.Length));
                float d = Math.Min(num, 10);

                float sum1 = 0, sum2 = 0;

                for (int y = 1; y < lsfW.Length; y++)
                {
                    sum1 += lsfW[y] * (float)Math.Exp(-2 * Math.PI * step * y / lsfW.Length);
                    sum2 += lsfW[y];
                }
                
                mtf[k - 1] = d * sum1 / sum2;
            }
            
            
            #endregion

            #region [Save]
            
            CsvService.Export("SumESF.csv", sumEsf);
            Console.WriteLine("SumESF.csv has been exported");
            CsvService.Export("LSFThreshold.csv", lsfThreshold);
            Console.WriteLine("LSFThreshold.csv has been exported");
            CsvService.Export("MTF.csv", mtf);
            Console.WriteLine("MTF.csv has been exported");

            CsvService.Export("LSFThresholdw.csv", lsfW);
            Console.WriteLine("LSFThresholdw.csv has been exported");

            string redactImageSaveName = _inputImage.Remove(0, _inputImage.LastIndexOf('\\') + 1);
            redactImageSaveName = redactImageSaveName.Insert(redactImageSaveName.IndexOf('.'), "_changed");
            
            redactImage.Save(redactImageSaveName);
            originalImage.Save("original_source.bmp");
            
            #endregion

            #region [Excel export]

            Console.WriteLine("Done");
            Console.WriteLine("Do you wanna draw graphs in excel? [Y/n]?");
            char answer = Console.ReadKey().KeyChar;

            if (answer == 'Y' || answer == 'y')
            {
                ProcessStartInfo excelProcess = new ProcessStartInfo();
                string currentDir = Directory.GetCurrentDirectory();
                excelProcess.FileName = Path.Combine(currentDir, "MtfExcel.exe");
                excelProcess.Arguments = Path.Combine(currentDir, "SumESF.csv");
                excelProcess.Arguments += " " + Path.Combine(currentDir, "LSFThreshold.csv");
                excelProcess.Arguments += " " + Path.Combine(currentDir, "MTF.csv");
                excelProcess.Arguments += " " + Path.Combine(currentDir, "LSFThresholdw.csv");
                Process.Start(excelProcess);
            }

            Console.WriteLine("\nEnter any key to exit...");
            Console.ReadKey();
            
            #endregion
        }
        
    }
}
