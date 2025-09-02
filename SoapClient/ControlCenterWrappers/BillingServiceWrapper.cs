using SoapClient.JasperBillingService;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using static SoapClient.ControlCenterWrappers.DeviceInfoWrapper;

namespace SoapClient.ControlCenterWrappers
{
    internal static class BillingServiceWrapper
    {
        internal static string GetInvoice(BillingService service, string licenseKey, long accountId, DateTime cycleStartDate)
        {
            GetInvoiceRequest request = new GetInvoiceRequest()
            {
                licenseKey = licenseKey,
                version = "1.0",
                messageId = $"Invoice_{accountId}_{cycleStartDate:yyyyMMdd}",
                accountId = accountId,
                cycleStartDate = cycleStartDate,
                cycleStartDateSpecified = true,
            };

            try
            {
                return GetInvoiceDetailAsString(service.GetInvoice(request));
            }
            catch (FaultException e)
            {
                Trace.WriteLine($"SOAP Fault: {e.Message}");
                throw e;
            }
            catch (Exception e)
            {
                Trace.WriteLine($"Error: {e.Message}");
                throw e;
            }
        }

        private static string GetInvoiceDetailAsString(GetInvoiceResponse response)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("=== Invoice Details ===");
            sb.AppendLine($"Account ID: {response.accountId}");
            sb.AppendLine($"Invoice ID: {response.invoiceId}");
            sb.AppendLine($"Currency: {response.currency}");
            sb.AppendLine($"Invoice Date: {response.invoiceDate:yyyy-MM-dd}");
            sb.AppendLine($"Due Date: {response.dueDate:yyyy-MM-dd}");
            sb.AppendLine($"Billing Cycle: {response.cycleStartDate:yyyy-MM-dd} to {response.cycleEndDate:yyyy-MM-dd}");
            sb.AppendLine($"Total Terminals: {response.totalTerminals}");
            sb.AppendLine("");
            sb.AppendLine("=== Charges ===");
            sb.AppendLine($"Subscription Charge: {response.subscriptionCharge} {response.currency}");
            sb.AppendLine($"Data Volume: {response.dataVolume} MB");
            sb.AppendLine($"Data Overage Charge: {response.overageCharge} {response.currency}");
            sb.AppendLine($"SMS Volume: {response.smsVolume}");
            sb.AppendLine($"SMS Charge: {response.smsCharge} {response.currency}");
            sb.AppendLine($"Voice Volume: {response.voiceVolume} sec");
            sb.AppendLine($"Voice Charge: {response.voiceCharge} {response.currency}");
            sb.AppendLine($"Other Charges: {response.otherCharge} {response.currency}");
            sb.AppendLine($"Activation Charges: {response.activationCharge} {response.currency}");
            sb.AppendLine($"Discount Applied: {response.discountApplied} {response.currency}");
            sb.AppendLine("");
            sb.AppendLine($"Total Charge: {response.totalCharge} {response.currency}");
            sb.AppendLine("");

            return sb.ToString();
        }

        public struct DeviceBillableUsage
        {
            public DateTime cycleStartDate { get; private set; }
            public long billableDataUsage { get; private set; }
            public int billableSmsUsage { get; private set; }
            public int billableVoiceUsage { get; private set; }
            public string ratePlan { get; private set; }

            public DeviceBillableUsage(GetTerminalUsageResponse response)
            {
                cycleStartDate = response.cycleStartDate;
                ratePlan = response.ratePlan;
                billableDataUsage = (long)Math.Ceiling(response.billableDataVolume / (1024 * 1024));
                billableSmsUsage = (int)Math.Ceiling(response.billableSMSVolume);
                billableVoiceUsage = (int)Math.Ceiling(response.billableVoiceVolume);
            }

            internal static Dictionary<string, DeviceBillableUsage> deviceBillingCache = new Dictionary<string, DeviceBillableUsage>();
        }

        internal static void UpdateBillingInfoForIccId(BillingService billingService, string licenseKey, string iccid, DateTime cycleStartDate)
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
                DeviceBillableUsage.deviceBillingCache[iccid] = new DeviceBillableUsage(billingService.GetTerminalUsage(request));
            }
            catch (System.Web.Services.Protocols.SoapException e)
            {
                if (e.Message.Contains("200200")) // No usage found
                {
                    Trace.WriteLine($"No usage data found for ICCID {iccid}");
                }
                Shared.LogException(e);
            }
        }
    }
}
