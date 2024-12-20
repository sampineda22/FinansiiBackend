using System.Xml.Serialization;

namespace CRM.Features.Accounting.CD
{
    public class CDHEADER
    {
        [XmlElement("CDLINES", typeof(CDLINES))]
        public CDLINES[] LINES { get; set; }
    }
    public class CDLINES
    {
        [XmlElement]
        public string CERTIFICATENUMBER { get; set; }
        [XmlElement]
        public string LEDGERDIMENSION { get; set; }
        [XmlElement]
        public string OFFSETLEDGERDIMENSION { get; set; }
        [XmlElement]
        public string TRANSDATE { get; set; }
        [XmlElement]
        public string JOURNALDATE { get; set; }
        [XmlElement]
        public string LEDGERJOURNALTRANSTXT { get; set; }
        [XmlElement]
        public string CURRENCYCODE { get; set; }
        [XmlElement]
        public decimal AMOUNTCURDEBIT { get; set; }
        [XmlElement]
        public decimal AMOUNTCURCREDIT { get; set; }
    }
}
