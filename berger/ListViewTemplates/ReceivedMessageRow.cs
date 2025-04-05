using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace berger.ListViewTemplates
{
    public class ReceivedMessageRow
    {
        public int Id { get; set; }
        public string ReceivedMessage { get; set; }
        public string BergerCode { get; set; }
        public bool ErrorFlag { get; set; }
        public DateTime ReceivedDate { get; set; }
    }
}
