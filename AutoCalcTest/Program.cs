using System;

namespace AutoCalcTest
{
    class Program
    {
       

        static void Main(string[] args)
        {
            Console.WriteLine("Enter input:");
            string input = Console.ReadLine();
            Console.WriteLine("Enter expected results:");
            string expected = Console.ReadLine();

            CalcTester tester = new CalcTester();
            tester.TestCalc(input, expected);
        }

    }
}