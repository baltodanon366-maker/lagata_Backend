using MongoDB.Driver;
using LicoreriaAPI.Domain.Models.MongoDB;

namespace LicoreriaAPI.Infrastructure.Data.MongoDB;

/// <summary>
/// Helper para acceder a las colecciones de MongoDB
/// </summary>
public static class MongoCollections
{
    public const string Notifications = "notifications";
    public const string NotificationTemplates = "notification_templates";
    public const string NotificationHistory = "notification_history";
    public const string UserLogs = "user_logs";
    public const string SystemLogs = "system_logs";
    public const string AuditTrail = "audit_trail";
    public const string SalesMetrics = "sales_metrics";
    public const string InventoryMetrics = "inventory_metrics";
    public const string DashboardMetrics = "dashboard_metrics";
    public const string DocumentMetadata = "document_metadata";
    public const string MobileSyncData = "mobile_sync_data";
    public const string ReportCache = "report_cache";
    public const string EtlJobs = "etl_jobs";

    /// <summary>
    /// Obtiene una colección tipada de MongoDB
    /// </summary>
    public static IMongoCollection<T> GetCollection<T>(MongoDbContext context, string collectionName)
    {
        return context.Database.GetCollection<T>(collectionName);
    }

    // Helpers específicos para cada tipo de colección
    public static IMongoCollection<Notification> NotificationsCollection(MongoDbContext context)
        => GetCollection<Notification>(context, Notifications);

    public static IMongoCollection<UserLog> UserLogsCollection(MongoDbContext context)
        => GetCollection<UserLog>(context, UserLogs);

    public static IMongoCollection<SalesMetric> SalesMetricsCollection(MongoDbContext context)
        => GetCollection<SalesMetric>(context, SalesMetrics);
}

