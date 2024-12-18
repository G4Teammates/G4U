﻿using ProductMicroservice.DBContexts.Entities;
using ProductMicroservice.Models.DTO;
using ProductMicroservice.Models;

namespace ProductMicroservice.Repostories.Helper
{
    public interface IHelper
    {
        Task<string> ScanFileForVirus(IFormFile file);
        Task<string> UploadImageCloudinary(IFormFile file);
        Task<string> UploadFileToGoogleDrive(IFormFile file);
        Task<Products> CreateProduct(CreateProductModel Product, List<LinkModel> linkModel, string username);
        Task<Products> UpdateProduct(UpdateProductModel Product);
        public bool IsContentAppropriate(string content);

        Task<Products> CreateProductClone(CreateProductModel Product, List<LinkModel> linkModel, string username);
        Task<Products> UpdateProductClone(UpdateProductModel Product);
    }
}
