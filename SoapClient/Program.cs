using SoapClient.ControlCenterWrappers;
using System;
using System.Collections.Generic;
using CommandLine;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SoapClient
{
    internal static class Program
    {
        const string LicenseKey = "08976573-dee4-4acf-815f-fa5a6f6d69a6";

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            var parsed = Parser.Default.ParseArguments<Options>(args);
            var options = parsed.Value;

            if (options.task == null)
                throw new ArgumentNullException("Missing '-task' argument. Options include: 'provision'");

            var service = Shared.GetTerminalService(options.uname, options.apikey);

            if (options.task == "provision")
            {
                if (options.ipfile is null || !options.ipfile.Contains(".csv"))
                    throw new ArgumentNullException("Missing '-ipfile' argument. Please provide a valid CSV file");

                TaskRunner.SetIp(options.ipfile, service, LicenseKey);
            }

            //GetModifiedTerminals.GetModifiedTerminal(service, licenseKey: "");
            
            
            
            
            
            
            //Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new Form1());
        }
    }
}
