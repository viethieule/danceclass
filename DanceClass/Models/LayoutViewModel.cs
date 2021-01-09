using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DanceClass.Models
{
    public class LayoutViewModel
    {
        public SelectedLeftMenuItem? SelectedLeftMenuItem { get; set; }
        public SelectedLeftMenuSubItem? SelectedLeftMenuSubItem { get; set; }
    }

    public enum SelectedLeftMenuItem
    {
        Schedule,
        Members,
        Create,
        Report
    }

    public enum SelectedLeftMenuSubItem
    {
        Create_Receptionist,
        Create_Member,
        Report_Revenue
    }
}