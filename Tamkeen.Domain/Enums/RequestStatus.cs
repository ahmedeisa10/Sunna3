namespace Tamkeen.Domain.Enums
{
    
    public enum RequestStatus
    {
        Pending = 0,   // View for vendors
        vendorAccepted = 1,
        Resolved = 2,
        Closed = 3,
        ManagerReview = 4,   // Waiting for manager approval >> default status when ticket created
        Rejected = 5,   // manager rject >> still exist but rejected
    }
}
