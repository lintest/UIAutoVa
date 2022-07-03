using System;
using System.Diagnostics;
using System.Windows.Automation;
using System.Text.Json;

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
            AutomationElement element = src as AutomationElement;
            if (element != null)
            {
                try
                {
                    int processId = element.Current.ProcessId;
                    using (Process process = Process.GetProcessById(processId))
                    {
                        var summary = new
                        {
                            EventId = "OnFocusChanged",
                            Name = element.Current.Name,
                            ProcessId = element.Current.ProcessId,
                            AutomationId = element.Current.AutomationId,
                        };
                        var options = new JsonSerializerOptions { WriteIndented = false };
                        string jsonString = JsonSerializer.Serialize(summary, options);
                        Console.WriteLine(jsonString + ",");
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("{{Error: \"{0}\"}}", e.Message);
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
                    var processName = process.ProcessName;
                    Console.WriteLine("  Open Window: Name: {0}, Id: {1}, Process: {2}", name, processId, processName);
                    AutomationEventHandler closeFunction = (object sender, AutomationEventArgs e) => {
                        Console.WriteLine("  Close Window: Name: {0}, ProcessId: {1}, Process: {2}", name, processId, processName);
                    };
                    Automation.AddAutomationEventHandler(WindowPattern.WindowClosedEvent, element, TreeScope.Element, closeFunction);
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