using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PracticalTask.Services;

namespace PracticalTask.Tests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void GetCloserNumFloat()
        {
            MathService mathService = new MathService();

            float value = 9.36f;
            float[] range = 
            {
                2.15f, 2.91f, 3.53f, 4.54f, 9.07f, 15.11f, 1.09f, 7.77f, 130.03f, -8.81f, 6.39f, -91.00f, 9.91f, -9.35f,
                10.21f, 3.96f, 11.63f, 5.54f, -9.36f, float.Epsilon, float.MaxValue, float.MinValue,
                float.NegativeInfinity, float.PositiveInfinity
            };

            var actual = mathService.GetCloserNum(value, false, range);

            Assert.AreEqual(9.07f, actual);
        }
    }
}
