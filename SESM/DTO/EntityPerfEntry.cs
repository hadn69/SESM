using System;

namespace SESM.DTO
{
    public class EntityPerfEntry
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }

        public int RamUsage { get; set; }
        public int? RamUsagePeak { get; set; }
        public int? RamUsageTrough { get; set; }
        public int? RamUsageQ1 { get; set; }
        public int? RamUsageQ3 { get; set; }

        public int CPUUsage { get; set; }
        public int? CPUUsagePeak { get; set; }
        public int? CPUUsageTrough { get; set; }
        public int? CPUUsageQ1 { get; set; }
        public int? CPUUsageQ3 { get; set; }
    }
}