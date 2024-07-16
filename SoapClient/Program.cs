using CommandLine;
using SoapClient.ControlCenterWrappers;
using System;
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

            Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));
            Trace.AutoFlush = true;
            Trace.Indent();

            if (options.task == null)
                throw new ArgumentNullException("Missing '-task' argument. Options include: 'provision'");

            var service = Shared.GetTerminalService(options.uname, options.apikey);

            if (options.task == "provision")
            {
                if (options.ipfile is null || !options.ipfile.Contains(".csv"))
                    throw new ArgumentNullException("Missing '-ipfile' argument. Please provide a valid CSV file");

                TaskRunner.SetIp(options.ipfile, service, options.license);
            }

            //Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new Form1());
        }
    }
}
