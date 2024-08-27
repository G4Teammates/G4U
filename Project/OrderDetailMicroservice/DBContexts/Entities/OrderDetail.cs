using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace OrderDetail.DBContexts.Entities
{
    public class OrderDetail
    {
        public string? DisplayName { get; set; }
        public string? Avatar { get; set; }
        public required int Status { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
