using LinkMicroservice.DBContexts.Enum;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace LinkMicroservice.DBContexts.Entities
{
    public class Link
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public required string ProviderName { get; set; }
        public required string Url { get; set; }
        public LinkType Type { get; set; }
        public LinkStatus Status { get; set; }

        public DateTime CreateAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdateAt { get; set; }

        public Guid ProductId { get; set; }
    }
}
