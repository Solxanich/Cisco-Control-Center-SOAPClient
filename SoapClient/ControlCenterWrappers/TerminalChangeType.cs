using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoapClient.ControlCenterWrappers
{

    // See Terminal Change type under 'Simple Types' documentation
    internal enum TerminalChangeType
    {
        custom1 = 17, // The 'Location ID' Account Custom 1 which is modifiable by customer
        deviceid = 1,
        customer = 6,
        rateplan = 4,
        status = 3,
    }
}
