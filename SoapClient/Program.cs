using SoapClient.ControlCenterWrappers;
using System;
using System.Collections.Generic;
using CommandLine;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using log4net;
using log4net.Config;
using System.IO;

namespace SoapClient
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// 
        public static readonly ILog log = LogManager.GetLogger(typeof(Program));

        [STAThread]
        static void Main(string[] args)
        {
            XmlConfigurator.Configure(new FileInfo("LogConfig.xml"));
            log.Info("\n\nProgram start");

            var parsed = Parser.Default.ParseArguments<Options>(args);
            var options = parsed.Value;

            Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));
            Trace.AutoFlush = true;
            Trace.Indent();

            try
            {
                if (options.task == null)
                    throw new ArgumentNullException("Missing '-task' argument. Options include: 'provision'");
            }
            catch(Exception ex) {
                log.Info(ex.ToString());
            }

            var service = Shared.GetTerminalService(options.uname, options.apikey);

            if (options.task == "provision")
            {
                try
                {
                    if (options.ipfile is null || !options.ipfile.Contains(".csv"))
                        throw new ArgumentNullException("Missing '-ipfile' argument. Please provide a valid CSV file");
                    TaskRunner.SetIp(options.ipfile, service, options.license);
                }
                catch (Exception ex)
                {
                    log.Info(ex.ToString());
                }

            }
            
            //Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new Form1());
        }
    }
}
