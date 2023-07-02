using System;

namespace PracticalTask.Interfaces
{
    public interface IMathService
    {
        TNum GetCloserNum<TNum>(TNum number, bool isMajor = false, params TNum[] range) where TNum : IComparable, IEquatable<TNum>;
        float GetCloserNum(float number, bool isMajor = false, params float[] range);
        bool IsNumericType(Type num);
        Tuple<float, float> LeastSquares(int x, int[] columns);
        TArray[] DifferentiationFunc<TArray>(TArray[] func, int difLevel = 1);
        TArray[] FillingArrayByFunc<TArray>(Func<float, TArray> func, int arraySize, float step = 1);
        float[] MovingAverage(float[] array, int length, int averageInterval);
    }
}