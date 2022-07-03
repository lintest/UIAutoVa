using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Automation;
using System.Text.Json;
using System.Windows;

namespace FocusChanged {

    class Program {

        private static readonly Dictionary<AutomationEvent, String> events = new Dictionary<AutomationEvent, String> {
            { SelectionItemPattern.ElementSelectedEvent, "ElementSelected" },
            { AutomationElement.MenuOpenedEvent, "MenuOpened" },
            { AutomationElement.MenuClosedEvent, "MenuClosed" },
            { InvokePattern.InvokedEvent, "Invoked" },
        };

        static StreamWriter writer;

        static void Main(string[] args) {
            if (args.Length < 1) {
                Console.WriteLine("Set the output file name");
                return;
            }
            using (var outputFile = new StreamWriter(args[0])) {
                writer = outputFile;
                Automation.AddAutomationFocusChangedEventHandler(OnFocusChangedHandler);
                Automation.AddAutomationEventHandler(WindowPattern.WindowOpenedEvent, AutomationElement.RootElement, TreeScope.Subtree, OnWindowOpened);
                foreach (var item in events) {
                    Automation.AddAutomationEventHandler(item.Key, AutomationElement.RootElement, TreeScope.Subtree, OnAutomationEventHandler);
                }
                Console.WriteLine("Monitoring... Hit enter to end.");
                Console.ReadLine();
            }
        }

        private static void WriteEvent(object src, String eventId) {
            AutomationElement element = src as AutomationElement;
            if (element != null) {
                AutomationElement.AutomationElementInformation current;
                try { 
                    current = element.Current; 
                } catch {
                    return;
                }
                try {
                    int processId = 0;
                    var rect = current.BoundingRectangle; 
                    try { processId = current.ProcessId; } catch { }
                    using (Process process = Process.GetProcessById(processId)) {
                        var summary = new {
                            EventId = eventId,
                            Name = current.Name,
                            ProcessId = current.ProcessId,
                            AutomationId = current.AutomationId,
                            Rectangle = new {
                                Left = rect.Left != Double.NaN ? rect.Left : 0,
                                Top = rect.Top != Double.NaN ? rect.Top : 0,
                                Width = rect.Width != Double.NaN ? rect.Width : 0,
                                Height = rect.Height != Double.NaN ? rect.Height : 0,
                            },
                        };
                        var options = new JsonSerializerOptions { WriteIndented = false };
                        string jsonString = JsonSerializer.Serialize(summary, options);
                        writer.WriteLine(jsonString + ",");
                    }
                }
                catch (Exception e) {
                    writer.WriteLine("{{Error: \"{0}\"}}", e.Message);
                }
            }
        }

        private static void OnFocusChangedHandler(object src, AutomationFocusChangedEventArgs args) {
            WriteEvent(src, "FocusChanged");
        }

        private static void OnWindowOpened(object src, AutomationEventArgs args) {
            AutomationElement element = src as AutomationElement;
            if (element != null) {
                WriteEvent(src, "WindowOpened");
                try {
                    int processId = element.Current.ProcessId;
                    using (Process process = Process.GetProcessById(processId)) {
                        var summary = new {
                            EventId = "WindowClosed",
                            Name = element.Current.Name,
                            ProcessId = element.Current.ProcessId,
                            AutomationId = element.Current.AutomationId,
                        };
                        AutomationEventHandler closeFunction = (object sender, AutomationEventArgs e) => {
                            var options = new JsonSerializerOptions { WriteIndented = false };
                            string jsonString = JsonSerializer.Serialize(summary, options);
                            writer.WriteLine(jsonString + ",");
                        };
                        Automation.AddAutomationEventHandler(WindowPattern.WindowClosedEvent, element, TreeScope.Element, closeFunction);
                    }
                }
                catch (Exception e) {
                    writer.WriteLine("{{Error: \"{0}\"}}", e.Message);
                }
            }
        }

        private static void OnAutomationEventHandler(object sender, AutomationEventArgs args) {
            WriteEvent(sender, events[args.EventId]);
        }
    }
}