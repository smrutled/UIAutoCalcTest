using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using AutoCalcTest;

namespace UnitTestUIAutoCalc
{
    [TestClass]
    public class CalcUITests
    {
        [TestMethod]
        public void AdditionTest()
        {
            CalcTester calcTester = new CalcTester();
            Assert.IsTrue(calcTester.TestCalc("1+1=", "2"));
        }
    }
}
