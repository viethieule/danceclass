using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Enums
{
    public enum RegistrationStatus
    {
        [Description("Đăng ký")]
        Registered,
        [Description("Tới lớp")]
        Attended,
        [Description("Nghỉ")]
        Off
    }
}
