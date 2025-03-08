using PAS.Models;

namespace PAS.Services
{
    public class PurchaseOrderState
    {
        public PurchaseOrder CurrentPurchaseOrder { get; set; }
        public PurchaseOrderItem CurrentItem { get; set; }
    }

}
