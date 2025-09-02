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
        internal static void PrintTotalUsageByCustomer()
        {
            var terminalUsages = new List<TerminalUsageInfo>();

            foreach (var iccId in TaskRunner.iccIdCache)
            {
                var billingData = BillingServiceWrapper.DeviceBillableUsage.deviceBillingCache[iccId];
                var deviceInfo = DeviceInfoWrapper.TerminalDetails.deviceDetailsCache[iccId];

                var terminalUsage = new TerminalUsageInfo()
                {
                    Customer = deviceInfo.Customer,
                    Iccid = iccId,
                    DeviceId = $"{deviceInfo.GisLocation}::{deviceInfo.Comments}",
                    DataUsageMB = billingData.billableDataUsage,
                    SmsUsage = billingData.billableSmsUsage,
                    VoiceUsageSeconds = billingData.billableVoiceUsage
                };

                terminalUsages.Add(terminalUsage);
            }

            var customerUsages = GetTotalUsageByCustomer(terminalUsages);

            LogCustomerUsageSummaries(customerUsages, TaskRunner.currentBillingCycleCached);
        }

        private static List<CustomerUsageSummary> GetTotalUsageByCustomer(List<TerminalUsageInfo> terminalUsages)
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
}