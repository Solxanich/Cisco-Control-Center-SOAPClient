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
                    throw new ArgumentNullException("Missing '-task' argument. Options include: 'provision', 'invoice'");

                var service = Shared.GetTerminalService(options.uname, options.apikey);

                if (options.task == "provision")
                {
                    if (options.ipfile is null || !options.ipfile.Contains(".csv"))
                        throw new ArgumentNullException("Missing '-ipfile' argument. Please provide a valid CSV file");

                    TaskRunner.ProvisionSim(options.ipfile, service, options.license);
                }
                else if (options.task == "invoice")
                {
                    if (options.accountid == 0)
                        throw new ArgumentNullException("Missing '-accountid' argument");

                    if (string.IsNullOrEmpty(options.cyclestart) || string.IsNullOrEmpty(options.cycleend))
                        throw new ArgumentNullException("Missing cycle dates");

                    DateTime startDate = DateTime.Parse(options.cyclestart);
                    DateTime endDate = DateTime.Parse(options.cycleend);


                    var billingService = Shared.GetBillingService(options.uname, options.apikey);
                    InvoiceManager.GetInvoice(billingService, options.license, options.accountid, startDate);
                }
                else if (options.task == "usage")
                {
                    if (options.accountid == 0)
                        throw new ArgumentNullException("Missing '-accountid' argument");

                    if (string.IsNullOrEmpty(options.cyclemonth))
                        throw new ArgumentNullException("Missing '-cyclemonth' argument (format: YYYY-MM)");

                    if (!DateTime.TryParse($"{options.cyclemonth}-01", out DateTime cycleStart))
                        throw new ArgumentException("Invalid cycle month format. Use YYYY-MM (e.g., 2024-01)");

                    var TerminalService = Shared.GetTerminalService(options.uname, options.apikey);
                    var billingService = Shared.GetBillingService(options.uname, options.apikey);
                    UsageAnalyzer.ProcessAccountUsageByCustomer(service, billingService, options.license, options.accountid, cycleStart);
                }

            }
            catch (Exception e)
            {
                Trace.WriteLine(e.Message);
                Environment.Exit(1);
            }
        }

        private static void HandleProvisionTask(Options options, SoapClient.com.jasperwireless.api7.TerminalService service)
        {
            if (options.ipfile is null || !options.ipfile.Contains(".csv"))
                throw new ArgumentNullException("Missing '-ipfile' argument. Please provide a valid CSV file");

            TaskRunner.ProvisionSim(options.ipfile, service, options.license);
        }
    }
}