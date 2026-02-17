using GreenBowlFoodsSystem.Models;
using Microsoft.AspNetCore.Identity;
using System.Numerics;

namespace GreenBowlFoodsSystem.ViewModel;

/* * DashboardViewModel - Green Bowl Foods System
 * This model aggregates data from multiple modules (Sales, Production, Inventory)
 * to provide a high-level overview of the facility's operational status.
 */

public class DashboardViewModel
{
    //  CARDS (Top Row Metrics)

    // Total monetary value of all finished goods currently stored in the warehouse
    public decimal TotalInventoryValue { get; set; }

    // Total accumulated revenue calculated from all "Paid" invoices in the system
    public decimal TotalRevenue { get; set; }

    // Counter for shipments that are currently in progress or not yet delivered
    public int ActiveShipmentsCount { get; set; }

    // Count of X-Ray inspection failures recorded within the last 24-hour window (Critical Control Point)
    public int QualityIssuesToday { get; set; }

    // Number of raw material lots that will reach their expiration date within the next 7 days
    public int ExpiringSoonCount { get; set; }

    // CRITICAL ALERTS (Inventory Warning Lists)

    // Collection of Finished Products where the available quantity has fallen below the safety stock threshold
    public List<FinishedProduct> LowStockProducts { get; set; } = new List<FinishedProduct>();

    // Collection of Raw Materials that are either below minimum stock levels or nearing expiration
    public List<RawMaterial> CriticalLowStockMaterials { get; set; } = new List<RawMaterial>();

    //LIVE FEEDS (Operational Activity Logs)

    // List of the most recent production runs, including their status (In Progress, Completed, or Hold)
    public List<ProductionBatch> RecentBatches { get; set; } = new List<ProductionBatch>();

    // List of the most recent outgoing shipments, used to track real-time distribution activity
    public List<Shipment> RecentShipments { get; set; } = new List<Shipment>();
}