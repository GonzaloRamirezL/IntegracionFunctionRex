using Common.Entity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common.ViewModels
{
    public class TransactionUserVM
    {
        public UserVM User { get; set; }
        public bool Add { get; set; }
        public bool Move { get; set; }
        public bool Update { get; set; }
        public bool Enable { get; set; }
        public bool Disable { get; set; }

        public List<LogEntity> LogEntities { get; set; }
    }
}
