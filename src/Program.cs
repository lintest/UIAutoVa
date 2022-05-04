using System.Windows.Automation;
using System.Diagnostics;
using System;

namespace FocusChanged
{
    class Program
    {
        static void Main(string[] args)
        {
            Automation.AddAutomationFocusChangedEventHandler(OnFocusChangedHandler);
            Automation.AddAutomationEventHandler(WindowPattern.WindowOpenedEvent, AutomationElement.RootElement, TreeScope.Subtree, OnWindowOpened);
            Console.WriteLine("Monitoring... Hit enter to end.");
            Console.ReadLine();
        }

        private static void OnFocusChangedHandler(object src, AutomationFocusChangedEventArgs args)
        {
            Console.WriteLine("Focus changed!");
            AutomationElement element = src as AutomationElement;
            if (element != null)
            {
                try
                {
                    string name = element.Current.Name;
                    string id = element.Current.AutomationId;
                    int processId = element.Current.ProcessId;
                    using (Process process = Process.GetProcessById(processId))
                    {
                        Console.WriteLine("  Name: {0}, Id: {1}, Process: {2}", name, id, process.ProcessName);
                    }
                }
                catch
                { 
                }
            }
        }

        private static void OnWindowOpened(object src, AutomationEventArgs args)
        {
            AutomationElement element = src as AutomationElement;
            if (element != null)
            {
                string name = element.Current.Name;
                string id = element.Current.AutomationId;
                int processId = element.Current.ProcessId;
                using (Process process = Process.GetProcessById(processId))
                {
                    Console.WriteLine("  Open Window: Name: {0}, Id: {1}, Process: {2}", name, id, process.ProcessName);
                }
                AutomationEventHandler closeFunction = (object sender, AutomationEventArgs e) => { Console.WriteLine("  Close Window"); };
                Automation.AddAutomationEventHandler(WindowPattern.WindowClosedEvent, element, TreeScope.Element, OnWindowClosed);
            }
        }

        private static void OnWindowClosed(object src, AutomationEventArgs args)
        {
            Console.WriteLine("  Close Window");
            AutomationElement element = src as AutomationElement;
            if (element != null)
            {
                string name = element.Current.Name;
                string id = element.Current.AutomationId;
                int processId = element.Current.ProcessId;
                using (Process process = Process.GetProcessById(processId))
                {
                    Console.WriteLine("  Close Window: Name: {0}, Id: {1}, Process: {2}", name, id, process.ProcessName);
                }
            }
        }

        private static void OnAutomationEventHandler(object src, AutomationFocusChangedEventArgs args)
        {
            Console.WriteLine("Focus changed!");
            AutomationElement element = src as AutomationElement;
            if (element != null)
            {
                string name = element.Current.Name;
                string id = element.Current.AutomationId;
                int processId = element.Current.ProcessId;
                using (Process process = Process.GetProcessById(processId))
                {
                    Console.WriteLine("  Name: {0}, Id: {1}, Process: {2}", name, id, process.ProcessName);
                }
            }
        }
    }
}