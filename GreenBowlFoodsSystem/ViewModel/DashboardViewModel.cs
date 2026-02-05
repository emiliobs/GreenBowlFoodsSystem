using GreenBowlFoodsSystem.Models;
using System.Numerics;

namespace GreenBowlFoodsSystem.ViewModel;

public class DashboardViewModel
{
    //KPI CARDS (Top Row)

    public decimal TotalInventoryValue { get; set; } // $$$ Money in warehouse
    public int ActiveShipmentsCount { get; set; }    // Trucks on the road
    public int QualityIssuesToday { get; set; }      // X-Ray Fails (Last 24h)
    public int ExpiringSoonCount { get; set; }       // Ingredients expiring in 7 days

    //  CRITICAL ALERTS (Right Panel)
    // Products (Finished Goods) running low
    public List<FinishedProduct> LowStockProducts { get; set; } = new List<FinishedProduct>();

    // Ingredients (Raw Materials) running low or expiring
    public List<RawMaterial> CriticalLowStockMaterials { get; set; } = new List<RawMaterial>();

    // LIVE FEEDS (Lerft Panel)
    // Recent production activity
    public List<ProductionBatch> RecentBatches { get; set; } = new List<ProductionBatch>();

    // Recent logistics activity
    public List<Shipment> RecentShipments { get; set; } = new List<Shipment>();
}