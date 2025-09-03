using CsvHelper;
using SoapClient.com.jasperwireless.api7;
using SoapClient.ControlCenterWrappers;
using System;
using System.Globalization;
using System.IO;
using System.Diagnostics;
using SoapClient.JasperBillingService;
using System.ServiceModel;
using System.Collections.Generic;
using System.Web.Services.Description;

namespace SoapClient
{
    internal static class TaskRunner
    {
        internal static List<string> iccIdCache = new List<string>();
        internal static DateTime currentBillingCycleCached = DateTime.MinValue;

        internal static void ProvisionSim(string filePath, TerminalService deviceInfoService, string licenseKey)
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
                            DeviceInfoWrapper.SetNewIP(deviceInfoService, licenseKey, iccId: sim, ip: item.ip, pdpId: item.pdpid, apn: item.apn);

                        DeviceInfoWrapper.EditTerminal(deviceInfoService, licenseKey, iccid: sim, TerminalChangeType.customer, item.customer);
                        DeviceInfoWrapper.EditTerminal(deviceInfoService, licenseKey, iccid: sim, TerminalChangeType.deviceid, item.deviceid); // For Billing
                        DeviceInfoWrapper.EditTerminal(deviceInfoService, licenseKey, iccid: sim, TerminalChangeType.locationid, item.deviceid); // For Customer Accounts Reference
                        DeviceInfoWrapper.EditTerminal(deviceInfoService, licenseKey, iccid: sim, TerminalChangeType.projectstatus, $"Project-{DateTime.Now.Year}"); // For Billing O&M vs Projects
                        DeviceInfoWrapper.EditTerminal(deviceInfoService, licenseKey, iccid: sim, TerminalChangeType.onboarded, "Onboarded"); // For whether this SIM has been provisioned

                        // Delay between calls per Cisco SPEC
                        while (System.DateTime.Now < currTime.AddSeconds(30)) { }
                    }
                }
            }
        }

        
        internal static void GetInvoiceTxt(BillingService billingService, string licenseKey, long accountid, DateTime cycleStartDate)
        {
            var invoiceDetails = BillingServiceWrapper.GetInvoice(billingService, licenseKey, accountid, cycleStartDate);

            File.WriteAllText($"{Directory.GetCurrentDirectory()}/{cycleStartDate.ToString("yyyy-MMM")}_invoice.txt", invoiceDetails);
        }
        
        // NOTE: This does not work well for historicals; SIM cards can be moved. Only use for the most recent two billing cycles
        internal static void UpdateAllCaches(TerminalService deviceInfoService, BillingService billingService, string licenseKey, long accountId, DateTime cycleStartDate)
        {
            Trace.WriteLine($"=== Processing Usage for Account {accountId} - Cycle: {cycleStartDate:yyyy-MM-dd} ===");
            currentBillingCycleCached = cycleStartDate;

            iccIdCache = DeviceInfoWrapper.GetActiveIccIdList(deviceInfoService, licenseKey);
            Trace.WriteLine($"Found {iccIdCache.Count} terminals for account {accountId}");

            UpdateAllBillingCaches(billingService, licenseKey, cycleStartDate);
            UpdateAllDeviceInfoCaches(deviceInfoService, licenseKey);
        }

        private static void UpdateAllBillingCaches(BillingService billingService, string licenseKey, DateTime cycleStartDate)
        {
            foreach (var item in iccIdCache)
            {
                BillingServiceWrapper.UpdateBillingInfoForIccId(billingService, licenseKey, item, cycleStartDate);
            }   
        }

        private static void UpdateAllDeviceInfoCaches(TerminalService deviceInfoService, string licenseKey)
        {
            const int maxPageSize = 50;

            for (int i = 0; i < iccIdCache.Count; i += maxPageSize)
            {
                DeviceInfoWrapper.UpdateDetailsForIccIdList(deviceInfoService, licenseKey, iccIdCache.GetRange(i, Math.Min(maxPageSize, iccIdCache.Count - i)).ToArray());
            }
        }
    }
}