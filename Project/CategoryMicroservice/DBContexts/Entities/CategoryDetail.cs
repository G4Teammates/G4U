using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CategoryMicroservice.DBContexts.Entities
{
    [PrimaryKey(nameof(CategoryId), nameof(ProductId))]
    public class CategoryDetail
    {
        public Category? Category { get; set; }
        [ForeignKey(nameof(Category))]
        public virtual Guid CategoryId { get; set; }
        public virtual Guid ProductId { get; set; }

    }
}
