namespace NepalMediHub.Models;

/// <summary>High-level product grouping used for navigation, SEO routes and recommendations.</summary>
public enum ProductType
{
    Medicine = 0,
    MedicalEquipment = 1,
    HealthProduct = 2
}

/// <summary>Lifecycle status of a customer order.</summary>
public enum OrderStatus
{
    Pending = 0,
    Confirmed = 1,
    Processing = 2,
    Shipped = 3,
    Delivered = 4,
    Cancelled = 5
}

/// <summary>Status of the payment associated with an order.</summary>
public enum PaymentStatus
{
    Pending = 0,
    Successful = 1,
    Failed = 2
}

/// <summary>Verification status for an uploaded prescription (future pharmacist workflow).</summary>
public enum PrescriptionStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2
}
