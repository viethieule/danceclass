using Google.Apis.Auth.OAuth2;
using System;

namespace Services.Report
{
    public class RevenueReportRq
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public UserCredential Credential { get; set; }
    }
}