using System.Collections.Generic;

namespace ProjectSMP.Features.Bank.Paycheck
{
    public class PaycheckEntry
    {
        public string Time { get; set; } = "";
        public string From { get; set; } = "";
        public int Amount { get; set; }
    }

    public class PaycheckData
    {
        public int PaycheckTime { get; set; }
        public List<PaycheckEntry> PaycheckList { get; set; } = new();
    }
}