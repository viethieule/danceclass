using DataAccess.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Entities
{
    public class FieldChangeLog : EntityBase
    {
        public int Id { get; set; }
        public string Entity { get; set; }
        public string EntityId { get; set; }
        public string ChangeLog { get; set; }
        public string Action { get; set; }
    }

    public class FieldChangeLogDetail
    {
        public string Field { get; set; }
        public string PreviousValue { get; set; }
        public string UpdatedValue { get; set; }
    }
}
