using SoapClient.ControlCenterWrappers;
using System;
using System.Collections.Generic;
using CommandLine;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

namespace SoapClient
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            var parsed = Parser.Default.ParseArguments<Options>(args);
            var options = parsed.Value;

            Trace.Listeners.Add(new TextWriterTraceListener("soapclient.log"));
            Trace.AutoFlush = true;
            Trace.WriteLine($"Program Start - {DateTime.Now}");
            Trace.Indent();

            try
            {
                if (options.task == null)
                    throw new ArgumentNullException("Missing '-task' argument. Options include: 'provision'");

                var service = Shared.GetTerminalService(options.uname, options.apikey);

                if (options.task == "provision")
                {
                    if (options.ipfile is null || !options.ipfile.Contains(".csv"))
                        throw new ArgumentNullException("Missing '-ipfile' argument. Please provide a valid CSV file");

                    TaskRunner.SetIp(options.ipfile, service, options.license);
                }
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.Message);
                Environment.Exit(1);
            }
        }
    }
}
