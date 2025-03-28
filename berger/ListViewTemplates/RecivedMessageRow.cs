using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace berger.ListViewTemplates
{
    public class RecivedMessageRow
    {
        public int Id { get; set; }
        public string RecivedMessage { get; set; }
        public bool ErrorFlag { get; set; }
    }
}
