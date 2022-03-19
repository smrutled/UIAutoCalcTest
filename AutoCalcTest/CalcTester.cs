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
        static Dictionary<char, AutomationElement> aeObjects = new Dictionary<char, AutomationElement>();
        static Dictionary<char, AutomationPropertyPair> oldCalcKeyValuePairs = new Dictionary<char, AutomationPropertyPair>()
        {
            {'1', new AutomationPropertyPair{ prop=AutomationElement.NameProperty, value="1" } },
            {'2', new AutomationPropertyPair{ prop=AutomationElement.NameProperty, value="2" } },
            {'3', new AutomationPropertyPair{ prop=AutomationElement.NameProperty, value="3" }},
            {'4', new AutomationPropertyPair{ prop=AutomationElement.NameProperty, value="4" }},
            {'5', new AutomationPropertyPair{ prop=AutomationElement.NameProperty, value="5" }},
            {'6', new AutomationPropertyPair{ prop=AutomationElement.NameProperty, value="6" }},
            {'7', new AutomationPropertyPair{ prop=AutomationElement.NameProperty, value="7" }},
            {'8', new AutomationPropertyPair{ prop=AutomationElement.NameProperty, value="8" }},
            {'9', new AutomationPropertyPair{ prop=AutomationElement.NameProperty, value="9" }},
            {'0', new AutomationPropertyPair{ prop=AutomationElement.AutomationIdProperty, value="130" }},
            {'+', new AutomationPropertyPair{ prop=AutomationElement.NameProperty, value="Add" }},
            {'-', new AutomationPropertyPair{ prop=AutomationElement.NameProperty, value="Subtract" }},
            {'*', new AutomationPropertyPair{ prop=AutomationElement.NameProperty, value="Multiply" }},
            {'/', new AutomationPropertyPair{ prop=AutomationElement.NameProperty, value="Divide" }},
            {'=', new AutomationPropertyPair{ prop=AutomationElement.NameProperty, value="Equals" }},
            {'c', new AutomationPropertyPair{ prop=AutomationElement.NameProperty, value="Clear" }},
            {'e', new AutomationPropertyPair{ prop=AutomationElement.AutomationIdProperty, value="Close" }},
            {'R', new AutomationPropertyPair{ prop=AutomationElement.AutomationIdProperty, value="150" }}
        };
        static Dictionary<char, AutomationPropertyPair> UWPCalcKeyValuePairs = new Dictionary<char, AutomationPropertyPair>()
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
            {'e', new AutomationPropertyPair{ prop=AutomationElement.AutomationIdProperty, value="Close" }},
            {'R', new AutomationPropertyPair{ prop=AutomationElement.AutomationIdProperty, value="CalculatorResults" }}
        };

        static void InvokeElement(char c)
        {
            InvokePattern pattern = patterns[c];
            pattern.Invoke();
        }
        public bool TestCalc(string input, string expected)
        {
            bool bPass = false;
            Process p = null;
            try
            {
                Console.WriteLine("Calculator Automation Test\n");
                Console.WriteLine("Launching Windows Calc application");
               
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



                Dictionary<char, AutomationPropertyPair> keyValuePairs;
                //Determine Calc version
                if (aeCalc.Current.ClassName == "ApplicationFrameWindow")
                    keyValuePairs = UWPCalcKeyValuePairs;
                else if (aeCalc.Current.ClassName == "CalcFrame")
                    keyValuePairs = oldCalcKeyValuePairs;
                else
                {
                    Console.WriteLine("Error: Unknown version of calc");
                    return false;
                }

                Console.WriteLine("Getting controls");
                bool bAllButtons = true;
                foreach (var keypair in keyValuePairs)
                {
                    Console.WriteLine("Getting control " + keypair.Value.value);
                    var aeObject = aeCalc.FindByProperty(TreeScope.Descendants, keypair.Value.prop, keypair.Value.value);
                    if (aeObject != null)
                    {
                        Console.WriteLine("Found control " + aeObject.Current.Name + " " + aeObject.Current.ClassName);
                        aeObjects[keypair.Key]= aeObject;
                        if (aeObject.TryGetCurrentPattern(InvokePattern.Pattern, out object pattern))
                            patterns[keypair.Key] = (InvokePattern)pattern;
                    }
                    else
                    {
                        bAllButtons = false;
                        Console.WriteLine("Button " + keypair.Value.value + " not found!!");
                    }
                }
                if(!bAllButtons)
                {
                    Console.WriteLine("Error: Missing buttons");
                    return false;
                }

                AutomationElement resultsText = aeObjects['R'];


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
            if(p!=null)
                p.Close();
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