using GreenBowlFoodsSystem.Models;
using System.Numerics;

namespace GreenBowlFoodsSystem.ViewModel;

public class DashboardViewModel
{
    // Counters for the cards above
    public int ActiveBatchesCount { get; set; }

    public int QAHoldCount { get; set; }
    public int PlannedCount { get; set; }

    // Ingentory alert

    public int LowStockProductCount { get; set; }

    // List of recent batches to display mini-table
    public List<ProductionBatch> RecentBatches { get; set; } = new List<ProductionBatch>();
}