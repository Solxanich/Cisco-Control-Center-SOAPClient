using System;
using System.Collections.Generic;
using System.Linq;
using SoapClient.com.jasperwireless.api7;
using SoapClient.JasperBillingService;
using System.Diagnostics;

namespace SoapClient.ControlCenterWrappers
{
    public class GetTerminalUsageRequest
    {
        public string licenseKey { get; set; }
        public string version { get; set; }
        public string messageId { get; set; }
        public string iccid { get; set; }
        public DateTime cycleStartDate { get; set; }
        public bool cycleStartDateSpecified { get; set; }
    }

    public class GetTerminalUsageResponse
    {
        public string iccid { get; set; }
        public DateTime cycleStartDate { get; set; }
        public DateTime cycleEndDate { get; set; }
        public long totalDataUsage { get; set; }
        public long billableDataUsage { get; set; }
        public int totalSmsUsage { get; set; }
        public int billableSmsUsage { get; set; }
        public long totalVoiceUsage { get; set; }
        public long billableVoiceUsage { get; set; }
    }

    public class TerminalUsageInfo
    {
        public string Iccid { get; set; }
        public string Customer { get; set; }
        public string DeviceId { get; set; }
        public long DataUsageMB { get; set; }
        public int SmsUsage { get; set; }
        public long VoiceUsageSeconds { get; set; }
    }

    public class CustomerUsageSummary
    {
        public string CustomerName { get; set; }
        public int TerminalCount { get; set; }
        public long TotalDataUsageMB { get; set; }
        public int TotalSmsUsage { get; set; }
        public long TotalVoiceUsageSeconds { get; set; }
        public List<TerminalUsageInfo> Terminals { get; set; } = new List<TerminalUsageInfo>();
    }

    internal static class UsageAnalyzer
    {
        internal static void ProcessAccountUsageByCustomer(TerminalService service, BillingService billingService, string licenseKey, long accountId, DateTime cycleStartDate)
        {
            try
            {
                Trace.WriteLine($"=== Processing Usage for Account {accountId} - Cycle: {cycleStartDate:yyyy-MM-dd} ===");

                List<string> iccids = GetModifiedTerminals.GetModifiedTerminal(service, licenseKey);
                Trace.WriteLine($"Found {iccids.Count} terminals for account {accountId}");

                List<TerminalUsageInfo> terminalUsages = new List<TerminalUsageInfo>();

                foreach (string iccid in iccids)
                {
                    try
                    {
                        var usage = GetTerminalUsage(billingService, licenseKey, iccid, cycleStartDate);

                        var terminalDetails = GetTerminalDetails(service, licenseKey, iccid);

                        if (usage != null && terminalDetails != null)
                        {
                            terminalUsages.Add(new TerminalUsageInfo
                            {
                                Iccid = iccid,
                                Customer = terminalDetails.Customer ?? "Unknown",
                                DeviceId = terminalDetails.DeviceId ?? "Unknown",
                                DataUsageMB = (long)usage.billableDataVolume / (1024 * 1024),
                                SmsUsage = (int)usage.billableSMSVolume,
                                VoiceUsageSeconds = (long)usage.billableVoiceVolume
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine($"Error processing ICCID {iccid}: {ex.Message}");
                    }
                }

                var customerSummaries = GroupUsageByCustomer(terminalUsages);

                LogCustomerUsageSummaries(customerSummaries, cycleStartDate);
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Error in ProcessAccountUsageByCustomer: {ex.Message}");
                throw;
            }
        }


        private static JasperBillingService.GetTerminalUsageResponse GetTerminalUsage(BillingService billingService, string licenseKey, string iccid, DateTime cycleStartDate)
        {
            var request = new JasperBillingService.GetTerminalUsageRequest
            {
                licenseKey = licenseKey,
                version = "1.0",
                messageId = $"Usage_{iccid}_{cycleStartDate:yyyyMMdd}",
                iccid = iccid,
                cycleStartDate = cycleStartDate,
                cycleStartDateSpecified = true
            };

            try
            {
                var response = billingService.GetTerminalUsage(request);
                return response;
            }
            catch (System.Web.Services.Protocols.SoapException e)
            {
                if (e.Message.Contains("200200")) // No usage found
                {
                    Trace.WriteLine($"No usage data found for ICCID {iccid}");
                    return null;
                }
                Shared.LogException(e);
                return null;
            }
        }

        private static TerminalDetails GetTerminalDetails(TerminalService service, string licenseKey, string iccid)
        {
            GetTerminalDetailsRequest request = new GetTerminalDetailsRequest() //Probably need a different function since this one doesn't grab the Customer field
            {
                licenseKey = licenseKey,
                version = "1.0",
                messageId = $"Details_{iccid}",
                iccids = iccid
            };
            
            var response = service.GetTerminalDetails(request);
            return new TerminalDetails 
            { 
                Customer = response.customer, 
                DeviceId = response.deviceId 
            };
        }

        private static List<CustomerUsageSummary> GroupUsageByCustomer(List<TerminalUsageInfo> terminalUsages)
        {
            var customerGroups = terminalUsages
                .GroupBy(t => t.Customer)
                .Select(g => new CustomerUsageSummary
                {
                    CustomerName = g.Key,
                    TerminalCount = g.Count(),
                    TotalDataUsageMB = g.Sum(t => t.DataUsageMB),
                    TotalSmsUsage = g.Sum(t => t.SmsUsage),
                    TotalVoiceUsageSeconds = g.Sum(t => t.VoiceUsageSeconds),
                    Terminals = g.ToList()
                })
                .OrderByDescending(c => c.TotalDataUsageMB)
                .ToList();

            return customerGroups;
        }

        private static void LogCustomerUsageSummaries(List<CustomerUsageSummary> summaries, DateTime cycleStartDate)
        {
            //Still need to calculate $ amount based on the data used and rate plans, Need to add a function to compute Subscription Charge, wasnt able to find Subscription Charge
            Trace.WriteLine("");
            Trace.WriteLine("=== CUSTOMER USAGE SUMMARY ===");
            Trace.WriteLine($"Billing Cycle: {cycleStartDate:yyyy-MM-dd}");
            Trace.WriteLine("");

            decimal totalDataGB = 0;
            int totalTerminals = 0;

            foreach (var summary in summaries)
            {
                decimal dataGB = summary.TotalDataUsageMB / 1024.0m;
                totalDataGB += dataGB;
                totalTerminals += summary.TerminalCount;

                Trace.WriteLine($"Customer: {summary.CustomerName}");
                Trace.WriteLine($"  Terminals: {summary.TerminalCount}");
                Trace.WriteLine($"  Total Data: {dataGB:F2} GB ({summary.TotalDataUsageMB:N0} MB)");
                Trace.WriteLine($"  Total SMS: {summary.TotalSmsUsage:N0}");
                Trace.WriteLine($"  Total Voice: {summary.TotalVoiceUsageSeconds / 60.0:F1} minutes");
                Trace.WriteLine("");


                if (summary.Terminals.Count <= 10)
                {
                    foreach (var terminal in summary.Terminals.OrderByDescending(t => t.DataUsageMB))
                    {
                        Trace.WriteLine($"    {terminal.DeviceId} ({terminal.Iccid}): {terminal.DataUsageMB:N0} MB");
                    }
                    Trace.WriteLine("");
                }
                else
                {
                    Trace.WriteLine($"    ... {summary.TerminalCount} terminals (details omitted)");
                    Trace.WriteLine("");
                }
            }

            Trace.WriteLine("=== OVERALL TOTALS ===");
            Trace.WriteLine($"Total Customers: {summaries.Count}");
            Trace.WriteLine($"Total Terminals: {totalTerminals}");
            Trace.WriteLine($"Total Data Usage: {totalDataGB:F2} GB");
            Trace.WriteLine("");
        }
    }

    public class TerminalDetails
    {
        public string Customer { get; set; }
        public string DeviceId { get; set; }
    }
}
