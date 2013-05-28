using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Windows.Automation;

namespace AutoCalcTest
{
    class Program
    {
       static AutomationElement getAEByName(AutomationElement parent, TreeScope scope, String name)
        {
            return parent.FindFirst(scope, new PropertyCondition(AutomationElement.NameProperty, name));
        }
        
        static void Main(string[] args)
        {
            int[] input = { 4, 6 };
            
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

                AutomationElement aecalc = null;
                int numWaits = 0;
                do
                {
                    Console.WriteLine("Looking for calc main window. . . ");
                    aecalc = aeDesktop.FindFirst(TreeScope.Children,
                        new PropertyCondition(AutomationElement.NameProperty, "Calculator"));
                    ++numWaits;
                    Thread.Sleep(200);
                }
                while (aecalc == null && numWaits < 50);

                if (aecalc == null)
                    throw new Exception("Failed to find calc main window");
                else
                    Console.WriteLine("Found calc main window");

                
                /*Get all buttons
                AutomationElementCollection aeAllButtons = null;
                aeAllButtons = aecalc.FindAll(TreeScope.Descendants,
            new PropertyCondition(AutomationElement.ControlTypeProperty,
              ControlType.Button));
                if (aeAllButtons == null)
                    throw new Exception("No buttons collection");
                else
                    Console.WriteLine("Got buttons collection");
                 */

                Console.WriteLine("Getting button controls");
                
                //Operator buttons
                AutomationElement addButton =getAEByName(aecalc,TreeScope.Descendants,"Add");
                AutomationElement subButton = getAEByName(aecalc, TreeScope.Descendants,"Subtract");
                AutomationElement equalsButton = getAEByName(aecalc, TreeScope.Descendants, "Equals");
                AutomationElement multiplyButton = getAEByName(aecalc, TreeScope.Descendants, "Multiply");
                AutomationElement divideButton = getAEByName(aecalc, TreeScope.Descendants, "Divide");
                AutomationElement clearButton = getAEByName(aecalc, TreeScope.Descendants, "Clear");
                AutomationElement resultsText = aecalc.FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.AutomationIdProperty, "150"));

                AutomationElement closeButton = getAEByName(aecalc, TreeScope.Descendants, "Close");

                //Get all number buttons
                AutomationElement[] numberButtons = new AutomationElement[10];
                InvokePattern[] iNumberButtons = new InvokePattern[10];
                numberButtons[0] = aecalc.FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.AutomationIdProperty, "130"));
                iNumberButtons[0] = (InvokePattern)numberButtons[0].GetCurrentPattern(InvokePattern.Pattern);
                for(int i=1; i<10; i++)
                {
                    numberButtons[i]= getAEByName(aecalc, TreeScope.Descendants, i.ToString());
                    iNumberButtons[i] = (InvokePattern)numberButtons[i].GetCurrentPattern(InvokePattern.Pattern);
                }

                //Setting up invoke patterns
                InvokePattern iAddButton = (InvokePattern)addButton.GetCurrentPattern(InvokePattern.Pattern);
                InvokePattern iSubButton = (InvokePattern)subButton.GetCurrentPattern(InvokePattern.Pattern);
                InvokePattern iEqualsButton = (InvokePattern)equalsButton.GetCurrentPattern(InvokePattern.Pattern);
                InvokePattern iMultiplyButton = (InvokePattern)multiplyButton.GetCurrentPattern(InvokePattern.Pattern);
                InvokePattern iDivideButton = (InvokePattern)divideButton.GetCurrentPattern(InvokePattern.Pattern);
                InvokePattern iClearButton = (InvokePattern)clearButton.GetCurrentPattern(InvokePattern.Pattern);
                InvokePattern iCloseButton = (InvokePattern)closeButton.GetCurrentPattern(InvokePattern.Pattern);

                Console.WriteLine("Got button controls");

                //Invokes buttons on the calc app
                iNumberButtons[input[0]].Invoke();
                iAddButton.Invoke();
                iNumberButtons[input[1]].Invoke();
                iEqualsButton.Invoke();
                
                //Gets results from calc
                string results =(string)resultsText.Current.Name;

                

                //Test if results are correct
                if (results == (input[0]+input[1]).ToString())
                {
                    Console.WriteLine("Test: Pass");
                }
                else
                {
                    Console.WriteLine("Test: FAIL");
                }

                Console.WriteLine("Closing application in 5 seconds");
                Thread.Sleep(5000);
                iCloseButton.Invoke();

                Console.WriteLine("End automation\n");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Fatal: " + ex.Message);
                Console.ReadLine();
            }

        }

    }
}
