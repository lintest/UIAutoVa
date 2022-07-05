using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Automation;
using System.Text.Json;
using System.Text.Encodings.Web;
using System.Text.Unicode;

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
                Console.WriteLine("Usage: {0} out_file.txt", System.AppDomain.CurrentDomain.FriendlyName);
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
                writer = null;
            }
        }

        static String getControlType(AutomationElement element) {
            var prefix = "ControlType.";
            var name = element.Current.ControlType.ProgrammaticName;
            return name.StartsWith(prefix) ? name.Substring(prefix.Length) : name;
        }

        private static String GetEventInfo(object src, AutomationEventArgs args, String eventId) {
            AutomationElement element = src as AutomationElement;
            if (element == null) {
                return null;
            }
            try {
                int processId = 0;
                var rect = element.Current.BoundingRectangle;
                try { processId = element.Current.ProcessId; } catch { }
                using (Process process = Process.GetProcessById(processId)) {
                    var summary = new {
                        EventId = eventId,
                        Name = element.Current.Name,
                        Type = getControlType(element),
                        ProcessId = element.Current.ProcessId,
                        AutomationId = element.Current.AutomationId,
                        HelpText = element.Current.HelpText,
                        Rectangle = new {
                            Left = rect.Left != Double.NaN ? rect.Left : 0,
                            Top = rect.Top != Double.NaN ? rect.Top : 0,
                            Width = rect.Width != Double.NaN ? rect.Width : 0,
                            Height = rect.Height != Double.NaN ? rect.Height : 0,
                        },
                    };
                    var options = new JsonSerializerOptions {
                        Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic),
                        WriteIndented = false,
                    };
                    return JsonSerializer.Serialize(summary, options);
                }
            }
            catch (Exception e) {
                Console.WriteLine(e.Message);
                return null;
            }
        }

        private static void WriteEvent(object src, AutomationEventArgs args, String eventId) {
            var jsonString = GetEventInfo(src, args, eventId);
            if (!String.IsNullOrEmpty(jsonString)) {
                writer.WriteLine(jsonString + ",");
            }
        }

        private static void OnFocusChangedHandler(object src, AutomationFocusChangedEventArgs args) {
            WriteEvent(src, args, "FocusChanged");
        }

        private static void OnWindowOpened(object src, AutomationEventArgs args) {
            AutomationElement element = src as AutomationElement;
            if (element != null) {
                WriteEvent(src, args, "WindowOpened");
                var jsonString = GetEventInfo(src, args, "WindowClosed");
                AutomationEventHandler closeFunction = (object sender, AutomationEventArgs e) => {
                    writer.WriteLine(jsonString + ",");
                };
                Automation.AddAutomationEventHandler(WindowPattern.WindowClosedEvent, element, TreeScope.Element, closeFunction);
            }
        }

        private static void OnAutomationEventHandler(object sender, AutomationEventArgs args) {
            WriteEvent(sender, args, events[args.EventId]);
        }
    }
}