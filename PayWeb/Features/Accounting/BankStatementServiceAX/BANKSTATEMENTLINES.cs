﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace CRM.Features.Accounting.BankStatementServiceAX
{
    public class BANKSTATEMENTHEAD
    {
        [XmlElement("BANKSTATEMENTLINES", typeof(BANKSTATEMENTLINES))]
        public BANKSTATEMENTLINES[] LINES { get; set; }
    }
    public class BANKSTATEMENTLINES
    {
        [XmlElement]
        public string JOURNALNAMEID { get; set; }
        [XmlElement]
        public string CURRENCYCODE { get; set; }
        [XmlElement]
        public string TRANSDATE { get; set; }
        [XmlElement]
        public string PAYMREFERENCE { get; set; }
        [XmlElement]
        public string LEDGERJOURNALTRANSTXT { get; set; }
        [XmlElement]
        public decimal AMOUNTCURDEBIT { get; set; }
        [XmlElement]
        public decimal AMOUNTCURCREDIT { get; set; }
        [XmlElement]
        public string ACCOUNTNUM { get; set; }

    }
}
