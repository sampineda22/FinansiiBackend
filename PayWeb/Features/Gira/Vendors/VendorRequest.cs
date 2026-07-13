namespace CRM.Features.Gira.Vendors
{
    public class VendorRequest
    {
        public string RequesterCode { get; set; }
        public string Description { get; set; }
        public string VendorName { get; set; }
        public string RTN { get; set; }
        public byte[]? InvoiceImage { get; set; }
    }
}
