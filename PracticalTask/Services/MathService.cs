using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CSharp.RuntimeBinder;
using PracticalTask.Interfaces;

namespace PracticalTask.Services
{
    public class MathService : IMathService
    {
        /// <summary>
        /// Обобщённый метод, вычисляющий ближайшие число на основе входных параметров.
        /// </summary>
        /// <param name="number">число, для которого производится поиск ближайшего числа</param>
        /// <param name="isMajor">выбор наибольшего число при равном диапозоне откланения.
        /// Например для числа 7 числа 5 и 9 одинакого близки, в случаии значения true метод вернёт 9.</param>
        /// <param name="range">список чисел для поиска ближайшего числа.</param>
        /// <typeparam name="TNum">тип числа может быть представлен любой базовой структурой числа.</typeparam>
        /// <returns>ближайшие число к входному из указаного диапозона.</returns>
        /// <exception cref="NotSupportedException">тип не являющийся числом.</exception>
        public TNum GetCloserNum<TNum>(TNum number, bool isMajor = false, params TNum[] range) where TNum: IComparable, IEquatable<TNum>
        {
            if (!IsNumericType(typeof(TNum)))
                throw new ArgumentException("Type " + typeof(TNum) + " is invalid");
            
            TNum[] array = new TNum[range.Length];
            TNum[] outNumbers = new TNum[2];
            
            int indexNum = 0;
            bool isInitLessNum = false;

            if (range.Contains(number))
                return number;

            for (int i = 0; i < range.Length; i++)
            {
                double num = (dynamic)range[i];
                
                if (double.IsNaN(num))
                    throw new ArithmeticException("number in range is NaN");
                
                array[i] = Math.Abs((dynamic)number - range[i]);

                if (i > 0 && array[i].CompareTo(array[indexNum]) == -1)
                {
                    indexNum = i;
                    outNumbers = new TNum[2];
                    outNumbers[0] = range[i];

                    isInitLessNum = false;
                }
                else if (i > 0 && array[i].CompareTo(array[indexNum]) == 0)
                {
                    if (!outNumbers.Contains(array[i]))
                        outNumbers[1] = range[i];

                    isInitLessNum = true;
                }
            }

            TNum biggerNum, lesserNum;

            if (!isInitLessNum)
                return outNumbers[0];

            if (outNumbers[0].CompareTo(outNumbers[1]) == -1)
            {
                lesserNum = outNumbers[0];
                biggerNum = outNumbers[1];
            }
            else
            {
                lesserNum = outNumbers[1];
                biggerNum = outNumbers[0];
            }
            
            return isMajor ? biggerNum : lesserNum;
        }
        
        /// <summary>
        /// Типизированый метод, вычисляющий ближайшие число на основе входных параметров.
        /// </summary>
        /// <param name="number">число, для которого производится поиск ближайшего числа</param>
        /// <param name="isMajor">выбор наибольшего число при равном диапозоне откланения.
        /// Например, для числа 7 числа 5 и 9 одинакого близки, в случаие значения true метод вернёт 9.</param>
        /// <param name="range">список чисел для поиска ближайшего числа.</param>
        /// <returns>ближайшие число к входному из указаного диапозона</returns>
        public float GetCloserNum(float number, bool isMajor = false, params float[] range)
        {
            float[] inputArray = range.OrderBy(x => x).ToArray();
            float[] array = new float[range.Length];
            float[] outNumbers = new float[2];
            
            int indexNum = 0;
            bool isInitLessNum = false;

            if (inputArray.Contains(number))
                return number;

            for (int i = 0; i < inputArray.Length; i++)
            {
                array[i] = Math.Abs(number - inputArray[i]);

                if (i > 0 && array[i] < array[indexNum])
                {
                    indexNum = i;
                    outNumbers = new float[2];
                    outNumbers[0] = inputArray[i];

                    isInitLessNum = false;
                }
                else if (i > 0 && array[i] == array[indexNum])
                {
                    if (!outNumbers.Contains(array[i]))
                        outNumbers[1] = inputArray[i];

                    isInitLessNum = true;
                }
            }

            float biggerNum, lesserNum;

            if (!isInitLessNum)
                return outNumbers[0];

            if (outNumbers[0] < outNumbers[1])
            {
                lesserNum = outNumbers[0];
                biggerNum = outNumbers[1];
            }
            else
            {
                lesserNum = outNumbers[1];
                biggerNum = outNumbers[0];
            }
            
            return isMajor ? biggerNum : lesserNum;
        }

        /// <summary>
        /// Проверка является ли объект числом.
        /// </summary>
        /// <param name="num">тип проверяемого объекта.</param>
        /// <returns>true, если входный тип, является числом.</returns>
        public bool IsNumericType(Type num)
        {
            HashSet<Type> numericTypes = new HashSet<Type>
            {
                typeof(int),
                typeof(uint),
                typeof(double),
                typeof(ushort),
                typeof(byte),
                typeof(long),
                typeof(ulong),
                typeof(decimal),
                typeof(float)
            };
            
            return numericTypes.Contains(num);
        }
        
        /// <summary>
        /// Проверка объекта на NaN.
        /// </summary>
        /// <param name="obj">проверяемый объек.т</param>
        /// <returns>true, если входной объект явялется NaN, в противном случае false.</returns>
        public bool IsNaN(dynamic obj)
        {
            float dub;

            try
            {
                dub = (float)obj;
                return float.IsNaN(dub);
            }
            catch (RuntimeBinderException)
            {
            }

            return false;
        }

        /// <summary>
        /// Метод наименьших квадратов.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="columns"></param>
        /// <returns>коэффициенты функции (y = ax + b) - a и b.</returns>
        public Tuple<float, float> LeastSquares(int x, int[] columns)
        {
            float sum1 = 0;
            float sum2 = 0;
            float sum3 = 0;

            float sum4 = 0;

            float coefA = 0 , coefB = 0;

            for (int i = 0; i < columns.Length - 1; i++)
            {
                sum1 += i * columns[i];
            }

            for (int i = 0; i < columns.Length - 1; i++)
            {
                sum2 += i;
            }

            for (int i = 0; i < columns.Length - 1; i++)
            {
                sum3 += columns[i];
            }


            for (int i = 0; i < columns.Length - 1; i++)
            {
                sum4 += i * i;
            }

            coefA = ((x + 1) * sum1 - sum2 * sum3) / ((x + 1) * sum4 - sum2 * sum2);
            coefB = (sum3 * sum4 - sum2 * sum1) / ((x + 1) * sum4 - sum2 * sum2);

            return new Tuple<float, float>(coefA, coefB);
        }

        /// <summary>
        /// Обобщёный метод, для дифференцирования функции.
        /// </summary>
        /// <param name="func">дифференцируемая функция.</param>
        /// <typeparam name="TArray">тип числа может быть представлен любой базовой структурой числа.</typeparam>
        /// <returns>дифференцированный массив, представляющий набор точке (x, y).</returns>
        public TArray[] DifferentiationFunc<TArray>(TArray[] func, int difLevel = 1)
        {
            TArray[] difFunc = new TArray[func.Length];

            for (int i = difLevel; i < func.Length - difLevel; i++)
            {
                difFunc[i - difLevel] = ((dynamic)func[i + difLevel] - func[i - difLevel]) / difLevel * 2;
            }

            return difFunc;
        }
        
        /// <summary>
        /// Типизированый метод, для дифференцирования функции.
        /// </summary>
        /// <param name="func">дифференцируемая функция.</param>
        /// <returns>дифференцированный массив, представляющий набор точке (x, y).</returns>
        public float[] DifferentiationFunc(float[] func, int difLevel = 1)
        {
            float[] difFunc = new float[func.Length];

            for (int i = difLevel; i < func.Length - difLevel; i++)
            {
                difFunc[i - difLevel] = (func[i + difLevel] - func[i - difLevel]) / difLevel * 2;
            }

            return difFunc;
        }

        /// <summary>
        /// Заполняет массив на основе данных функции.
        /// </summary>
        /// <param name="func">функция для заполнения массива.</param>
        /// <param name="arraySize">размер массива.</param>
        /// <param name="xStep">величина на которую меняется x.</param>
        /// <typeparam name="TArray">тип числа может быть представлен любой базовой структурой числа.</typeparam>
        /// <returns>заполненый массив.</returns>
        public TArray[] FillingArrayByFunc<TArray>(Func<float, TArray> func, int arraySize, float xStep = 1)
        {
            if (!IsNumericType(typeof(TArray)))
                throw new ArgumentException("Type " + typeof(TArray) + " is invalid");
            
            TArray[] array = new TArray[arraySize];
            float x = 0;
            
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = func(x);
                x += xStep;
            }

            return array;
        }

        public float[] MovingAverage(float[] array, int mLenghth, int averageInterval)
        {
            int index = 0;

            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] > 0)
                {
                    index = i;
                    break;
                }
            }

            float[] result = new float[mLenghth - averageInterval - index];

            for (int i = index; i < result.Length + index; i++ )
            {
                for (int j = i + 1; j < i + averageInterval; j++ )
                {
                    array[i] += array[j];
                }
                result[i - index] = array[i] / averageInterval;
            }
            
            return result;
        }
    }
}