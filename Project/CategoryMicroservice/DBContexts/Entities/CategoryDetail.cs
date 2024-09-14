using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MongoDB.Bson.Serialization.Attributes;

namespace CategoryMicroservice.DBContexts.Entities
{
    public class CategoryDetail
    {
        [BsonElement("productId")]
        public virtual Guid ProductId { get; set; }
    }
}
