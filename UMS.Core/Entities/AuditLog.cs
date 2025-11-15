using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UMS.Core.Entities
{
    [Table("AUDIT_LOG",Schema ="dbo")]
    public class AuditLog:EntityBase
    {
        public string RowId { get; set; }
        public string DbTable { get; set; }
        public string FieldChanges { get; set; }
        public string UserAction { get; set; }
        public string PerformedFromIp { get; set; }
    }
}
