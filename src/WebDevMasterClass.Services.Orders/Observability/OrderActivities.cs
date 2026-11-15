using System.Diagnostics;
using WebDevMasterClass.Services.Orders.Services;

namespace WebDevMasterClass.Services.Orders.Observability;

public static class OrderActivities
{
    public const string ActivitySourceName = "OrdersService";
    private static readonly ActivitySource ActivitySource = new(ActivitySourceName);

    public static Activity? Create()
    {
        var activity = ActivitySource.StartActivity($"{ActivitySourceName}.AddOrder");
        activity?.SetTag("ServiceName", nameof(OrdersService));
        return activity;
    }
}