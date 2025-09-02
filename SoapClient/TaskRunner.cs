using CsvHelper;
using SoapClient.com.jasperwireless.api7;
using SoapClient.ControlCenterWrappers;
using System;
using System.Globalization;
using System.IO;
using System.Diagnostics;
using SoapClient.JasperBillingService;
using System.ServiceModel;

namespace SoapClient
{
    internal static class TaskRunner
    {
        internal static void ProvisionSim(string filePath, TerminalService service, string licenseKey)
        {
            using (var reader = new StreamReader(filePath))
            {
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    var records = csv.GetRecords<ProvisioningArgs>();
                    foreach (var item in records)
                    {
                        System.DateTime currTime = System.DateTime.Now;

                        var sim = Shared.RemoveNonNumbersFromString(item.iccid);

                        if (!string.IsNullOrEmpty(item.ip))
                            DeviceInfoWrapper.SetNewIP(service, licenseKey, iccId: sim, ip: item.ip, pdpId: item.pdpid, apn: item.apn);

                        DeviceInfoWrapper.EditTerminal(service, licenseKey, iccid: sim, TerminalChangeType.customer, item.customer);
                        DeviceInfoWrapper.EditTerminal(service, licenseKey, iccid: sim, TerminalChangeType.deviceid, item.deviceid); // For Billing
                        DeviceInfoWrapper.EditTerminal(service, licenseKey, iccid: sim, TerminalChangeType.locationid, item.deviceid); // For Customer Accounts Reference
                        DeviceInfoWrapper.EditTerminal(service, licenseKey, iccid: sim, TerminalChangeType.projectstatus, $"Project-{DateTime.Now.Year}"); // For Billing O&M vs Projects
                        DeviceInfoWrapper.EditTerminal(service, licenseKey, iccid: sim, TerminalChangeType.onboarded, "Onboarded"); // For whether this SIM has been provisioned

                        // Delay between calls per Cisco SPEC
                        while (System.DateTime.Now < currTime.AddSeconds(30)) { }
                    }
                }
            }
        }

        
        internal static void GetInvoice(BillingService service, string licenseKey, long accountid, DateTime cycleStartDate)
        {
            var invoiceDetails = BillingServiceWrapper.GetInvoice(service, licenseKey, accountid, cycleStartDate);

            File.WriteAllText($"{Directory.GetCurrentDirectory()}/{cycleStartDate.ToString("yyyy-MMM")}_invoice.txt", invoiceDetails);
        }
        
    }
}