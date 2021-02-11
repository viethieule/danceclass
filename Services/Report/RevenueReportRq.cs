using Google.Apis.Auth.OAuth2;
using System;
using System.Collections.Generic;

namespace Services.Report
{
    public class RevenueReportRq
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public UserCredential Credential { get; set; }
        public List<int> BranchIds { get; set; }
        public int OrderBy { get; set; }

        public enum OrderByValues
        {
            None = 0,
            Branch = 1
        }
    }
}