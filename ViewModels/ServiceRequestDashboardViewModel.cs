// ViewModels/ServiceRequestDashboardViewModel.cs
using System;
using System.Collections.Generic;
using CasaHeights.Models;  // Add this for ServiceRequest

namespace CasaHeights.ViewModels
{
    public class ServiceRequestDashboardViewModel
    {
        public int NewRequestsCount { get; set; }
        public int InProgressCount { get; set; }
        public int CompletedTodayCount { get; set; }
        public int EmergencyCount { get; set; }
        public List<ServiceRequest> RecentRequests { get; set; } = new();
        public List<RequestTypeStats> RequestsByType { get; set; } = new();
        public List<RequestStatusStats> RequestsByStatus { get; set; } = new();
    }

    public class RequestTypeStats
    {
        public string Type { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class RequestStatusStats
    {
        public string Status { get; set; } = string.Empty;
        public int Count { get; set; }
    }
}
