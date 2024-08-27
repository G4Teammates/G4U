using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Category.DBContexts.Entities
{
    public class CategoryDetail
    {
        public Guid CategoryId { get; set; }
        public Guid ProductId { get; set; }
    }
}
