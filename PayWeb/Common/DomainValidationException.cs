using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PayWebRepository.Common
{
    public class DomainValidationException: Exception
    {
        public string Field { get; protected set; }
        public DomainValidationException(string field, string message) : base(message)
        {
            Field = field;
        }
        public DomainValidationException(string message) : base(message)
        {

        }
    }
}
