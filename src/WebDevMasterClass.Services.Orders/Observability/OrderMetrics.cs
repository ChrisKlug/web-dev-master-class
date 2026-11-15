using System.Diagnostics.Metrics;

namespace WebDevMasterClass.Services.Orders.Observability;

public class OrderMetrics
{
    public const string MeterName = "WebDevMasterClass.Services.Orders";
    private Counter<int> TotalOrdersCounter { get; }

    public OrderMetrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create(MeterName);

        TotalOrdersCounter = meter.CreateCounter<int>("orders", "Order", "Orders added");
    }

    public void AddOrder() => TotalOrdersCounter.Add(1);
}