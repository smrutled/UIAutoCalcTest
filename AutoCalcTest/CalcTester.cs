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
        static Dictionary<char, Condition> oldCalcKeyValuePairs = new Dictionary<char, Condition>()
        {
            {'1', new PropertyCondition(AutomationElement.AutomationIdProperty,"131") },
            {'2', new PropertyCondition(AutomationElement.NameProperty,"2") },
            {'3', new PropertyCondition(AutomationElement.NameProperty, "3" )},
            {'4', new PropertyCondition(AutomationElement.NameProperty, "4" )},
            {'5', new PropertyCondition(AutomationElement.NameProperty, "5" )},
            {'6', new PropertyCondition(AutomationElement.NameProperty, "6" )},
            {'7', new PropertyCondition(AutomationElement.NameProperty, "7" )},
            {'8', new PropertyCondition(AutomationElement.NameProperty, "8" )},
            {'9', new PropertyCondition(AutomationElement.NameProperty, "9" )},
            {'0', new PropertyCondition(AutomationElement.AutomationIdProperty, "130" )},
            {'+', new PropertyCondition(AutomationElement.NameProperty, "Add" )},
            {'-', new PropertyCondition(AutomationElement.NameProperty, "Subtract" )},
            {'*', new PropertyCondition(AutomationElement.NameProperty, "Multiply" )},
            {'/', new PropertyCondition(AutomationElement.NameProperty, "Divide" )},
            {'=', new PropertyCondition(AutomationElement.NameProperty, "Equals" )},
            {'c', new PropertyCondition(AutomationElement.NameProperty, "Clear" )},
            {'e', new PropertyCondition(AutomationElement.AutomationIdProperty, "Close" )},
            {'R', new PropertyCondition(AutomationElement.AutomationIdProperty, "150" )}
        };
        static Dictionary<char, Condition> UWPCalcKeyValuePairs = new Dictionary<char, Condition>()
        {
            {'1', new PropertyCondition(AutomationElement.NameProperty, "One" ) },
            {'2', new PropertyCondition(AutomationElement.NameProperty, "Two" )},
            { '3', new PropertyCondition(AutomationElement.NameProperty, "Three")},
            { '4', new PropertyCondition(AutomationElement.NameProperty, "Four")},
            { '5', new PropertyCondition(AutomationElement.NameProperty, "Five")},
            { '6', new PropertyCondition(AutomationElement.NameProperty, "Six")},
            { '7', new PropertyCondition(AutomationElement.NameProperty, "Seven")},
            { '8', new PropertyCondition(AutomationElement.NameProperty, "Eight")},
            { '9', new PropertyCondition(AutomationElement.NameProperty, "Nine")},
            { '0', new PropertyCondition(AutomationElement.NameProperty, "Zero")},
            { '+', new PropertyCondition(AutomationElement.NameProperty, "Plus")},
            { '-', new PropertyCondition(AutomationElement.NameProperty, "Minus")},
            { '*', new PropertyCondition(AutomationElement.NameProperty, "Multiply by")},
            { '/', new PropertyCondition(AutomationElement.NameProperty, "Divide by")},
            { '=', new PropertyCondition(AutomationElement.NameProperty, "Equals")},
            { 'c', new PropertyCondition(AutomationElement.NameProperty, "Clear")},
            { 'e', new PropertyCondition(AutomationElement.AutomationIdProperty, "Close")},
            { 'R', new PropertyCondition(AutomationElement.AutomationIdProperty, "CalculatorResults")}
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



                Dictionary<char, Condition> keyValuePairs;
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
                    Console.WriteLine("Getting control " + keypair.Key);
                    var aeObject = aeCalc.FindFirst(TreeScope.Descendants, keypair.Value);
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
                        Console.WriteLine("Control " + keypair.Key + " not found!!");
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