using ExtensionMethods;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Automation;

namespace AutoCalcTest
{
    public class CalcTester
    {
        //Holds the button patterns for the calculator and accessed via character
        static Dictionary<char, InvokePattern> patterns = new Dictionary<char, InvokePattern>();
        static Dictionary<char, AutomationPropertyPair> keyValuePairs = new Dictionary<char, AutomationPropertyPair>()
        {
            {'1', new AutomationPropertyPair{ prop=AutomationElement.NameProperty, value="One" } },
            {'2', new AutomationPropertyPair{ prop=AutomationElement.NameProperty, value="Two" } },
            {'3', new AutomationPropertyPair{ prop=AutomationElement.NameProperty, value="Three" }},
            {'4', new AutomationPropertyPair{ prop=AutomationElement.NameProperty, value="Four" }},
            {'5', new AutomationPropertyPair{ prop=AutomationElement.NameProperty, value="Five" }},
            {'6', new AutomationPropertyPair{ prop=AutomationElement.NameProperty, value="Six" }},
            {'7', new AutomationPropertyPair{ prop=AutomationElement.NameProperty, value="Seven" }},
            {'8', new AutomationPropertyPair{ prop=AutomationElement.NameProperty, value= "Eight" }},
            {'9', new AutomationPropertyPair{ prop=AutomationElement.NameProperty, value="Nine" }},
            {'0', new AutomationPropertyPair{ prop=AutomationElement.NameProperty, value="Zero" }},
            {'+', new AutomationPropertyPair{ prop=AutomationElement.NameProperty, value="Plus" }},
            {'-', new AutomationPropertyPair{ prop=AutomationElement.NameProperty, value="Minus" }},
            {'*', new AutomationPropertyPair{ prop=AutomationElement.NameProperty, value="Multiply by" }},
            {'/', new AutomationPropertyPair{ prop=AutomationElement.NameProperty, value="Divide by" }},
            {'=', new AutomationPropertyPair{ prop=AutomationElement.NameProperty, value="Equals" }},
            {'c', new AutomationPropertyPair{ prop=AutomationElement.NameProperty, value="Clear" }},
            {'e', new AutomationPropertyPair{ prop=AutomationElement.AutomationIdProperty, value="Close" }}
        };

        static void InvokeElement(char c)
        {
            InvokePattern pattern = patterns[c];
            pattern.Invoke();
        }
        public bool TestCalc(string input, string expected)
        {
            bool bPass = false;
            try
            {
                Console.WriteLine("Calculator Automation Test\n");
                Console.WriteLine("Launching Windows Calc application");
                Process p = null;
                p = Process.Start("calc.exe");

                int ct = 0;
                do
                {
                    Console.WriteLine("Looking for calc process. . . ");
                    ++ct;
                    Thread.Sleep(10);
                }
                while (p == null && ct < 50);

                if (p == null)
                    throw new Exception("Failed to find calc process");
                else
                    Console.WriteLine("Found calc process");

                Console.WriteLine("Getting Desktop");
                AutomationElement aeDesktop = null;
                aeDesktop = AutomationElement.RootElement;
                if (aeDesktop == null)
                    throw new Exception("Unable to get Desktop");
                else
                    Console.WriteLine("Found Desktop\n");

                AutomationElement aeCalc = null;
                int numWaits = 0;
                do
                {
                    Console.WriteLine("Looking for calc main window. . . ");
                    aeCalc = aeDesktop.FindFirst(TreeScope.Children,
                        new PropertyCondition(AutomationElement.NameProperty, "Calculator"));
                    ++numWaits;
                    Thread.Sleep(200);
                }
                while (aeCalc == null && numWaits < 50);

                if (aeCalc == null)
                    throw new Exception("Failed to find calc main window");
                else
                    Console.WriteLine("Found calc main window");


                Console.WriteLine("Getting button controls");

                foreach (var button in keyValuePairs)
                {
                    var aeButton = aeCalc.FindByProperty(TreeScope.Descendants, button.Value.prop, button.Value.value);
                    patterns[button.Key] = (InvokePattern)aeButton.GetCurrentPattern(InvokePattern.Pattern);
                }

                AutomationElement resultsText = aeCalc.FindByProperty(TreeScope.Descendants, AutomationElement.AutomationIdProperty, "CalculatorResults");


                Console.WriteLine("Got button controls");

                //Invokes buttons on the calc app
                foreach (char c in input)
                {
                    InvokeElement(c);
                }

                //Gets results from calc
                string results = (string)Regex.Match(resultsText.Current.Name, @"\d+").Value;

                //Test if results are correct
                if (results == expected)
                {
                    Console.WriteLine("Test: Pass");
                    bPass = true;
                }
                else
                {
                    Console.WriteLine("Test: FAIL");
                }

                Console.WriteLine("Closing application in 5 seconds");
                //Thread.Sleep(5000);
                patterns['e'].Invoke();

                Console.WriteLine("End automation\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Fatal: " + ex.Message);
            }
            return bPass;
        }
    }

    public struct AutomationPropertyPair
    {
        public AutomationProperty prop;
        public string value;
    }
}


namespace ExtensionMethods
{
    public static class AutomationElementExtensions
    {
        public static AutomationElement FindByName(this AutomationElement parent, TreeScope scope, String name)
        {
            return FindByProperty(parent, scope, AutomationElement.NameProperty, name);
        }

        public static AutomationElement FindByProperty(this AutomationElement parent, TreeScope scope, AutomationProperty prop, String name)
        {
            return parent.FindFirst(scope, new PropertyCondition(prop, name));
        }
    }
}