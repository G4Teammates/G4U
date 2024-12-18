﻿using AutoMapper;
using CloudinaryDotNet;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Azure;
using StatisticalMicroservice.DBContexts;
using StatisticalMicroservice.DBContexts.Entities;
using StatisticalMicroservice.Model;
using StatisticalMicroservice.Model.DTO;
using StatisticalMicroservice.Model.Message;
using StatisticalMicroservice.Models.DTO;
using System.Reflection.Metadata.Ecma335;
using X.PagedList.Extensions;

namespace StatisticalMicroservice.Repostories
{
    public class Repo : IRepo
    {
        #region declaration and initialization
        private readonly StatisticalDbcontext _db;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly IMessage _message;


        public Repo(IConfiguration configuration, StatisticalDbcontext db, IMapper mapper, IMessage message)
        {
            _configuration = configuration;
            _db = db;
            _mapper = mapper;
            _message = message;
        }
        #endregion
        public async Task<ResponseDTO> GetAll(int page, int pageSize)
        {
            ResponseDTO response = new();
            try
            {
                var Stas = await _db.Statistical.ToListAsync();
                if (Stas != null)
                {
                    response.Result = _mapper.Map<ICollection<Statistical>>(Stas).ToPagedList(page, pageSize);
                }
                else
                {
                    response.IsSuccess = false;
                    response.Message = "Not found any Statisticals";
                }
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }
            return response;
        }

        public async Task<ResponseDTO> CreateStastistical(CreateStatisticalModel Stastistical)
        {
            ResponseDTO response = new();
            try
            {
                var existingStatistical = await _db.Statistical.FirstOrDefaultAsync(s => s.CreateAt.Date == Stastistical.CreateAt.Date);
                if(existingStatistical != null)
                {
                    var updatemodel= new StatisticalModel
                    {
                        TotalWebsite = Stastistical.TotalWebsite,
                        CreateAt = Stastistical.CreateAt,
                        UpdatedAt = Stastistical.CreateAt
                    };
                    var update = await UpdateStastistical(updatemodel);
                    response.Result = update.Result;
                    return response;
                }

                // Nếu chưa tồn tại, tạo bản ghi mới
                var newStatistical = new StatisticalModel
                {
                    TotalWebsite = Stastistical.TotalWebsite,
                    CreateAt = Stastistical.CreateAt,
                    UpdatedAt = DateTime.Now
                };

                var mappedStatistical = _mapper.Map<Statistical>(newStatistical);
                await _db.Statistical.AddAsync(mappedStatistical);
                response.Result = mappedStatistical;

                await _db.SaveChangesAsync();
                response.IsSuccess = true;
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }
            return response;
        }

        public async Task<ResponseDTO> UpdateStastistical(StatisticalModel Stastistical)
        {
            ResponseDTO response = new();
            try
            {
                var upStas = await _db.Statistical.FirstOrDefaultAsync(s => s.CreateAt.Date == Stastistical.CreateAt.Date);
                if (upStas != null)
                {

                    upStas.TotalWebsite.TotalRevenue = Stastistical.TotalWebsite.TotalRevenue;
                    upStas.TotalWebsite.TotalViews = Stastistical.TotalWebsite.TotalViews;
                    upStas.TotalWebsite.TotalProducts = Stastistical.TotalWebsite.TotalProducts;
                    upStas.TotalWebsite.TotalSolds = Stastistical.TotalWebsite.TotalSolds;
                    upStas.TotalWebsite.TotalUsers = Stastistical.TotalWebsite.TotalUsers;
                    upStas.CreateAt=Stastistical.CreateAt;
                    upStas.UpdatedAt = DateTime.Now;
                     
                    var Stastisticali = _mapper.Map<Statistical>(upStas);
                    _db.Update(Stastisticali);
                    await _db.SaveChangesAsync();
                    response.Result = Stastisticali;
                } else
                {
                    response.Message = "Date is not in database";
                    return response;
                }
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }
            return response;
        }

        public async Task<ResponseDTO> UpdateStastisticalProduct(ProductResponse productResponse)
        {
            ResponseDTO response = new();
            try
            {
                // Kiểm tra nếu `updateAt` là ngày hợp lệ
                if (DateTime.TryParse(productResponse.updateAt.ToString(), out DateTime searchDate))
                {
                    var updateAtInUtc = DateTime.SpecifyKind(productResponse.updateAt, DateTimeKind.Utc);
                    // Tìm tài liệu với ngày tạo tương ứng
                    var upStas = await _db.Statistical.FirstOrDefaultAsync(x =>
                        x.CreateAt.Year == searchDate.Year &&
                        x.CreateAt.Month == searchDate.Month &&
                        x.CreateAt.Day == searchDate.Day);

                    if (upStas != null)
                    {
                        // Cập nhật các trường nếu tài liệu đã tồn tại
                        upStas.TotalWebsite.TotalProducts = productResponse.totalProducts;
                        upStas.TotalWebsite.TotalSolds = productResponse.totalSolds;
                        upStas.TotalWebsite.TotalViews = productResponse.totalViews;
                        upStas.UpdatedAt = updateAtInUtc;
                        /*upStas.CreateAt = productResponse.updateAt;*/

                        _db.Update(upStas);
                        await _db.SaveChangesAsync();
                        response.Result = upStas;
                    }
                    else
                    {
                        // Tìm tài liệu gần nhất theo `CreateAt`
                        var latestStas = await _db.Statistical
                            .OrderByDescending(x => x.CreateAt)
                            .FirstOrDefaultAsync();


                        // Tạo tài liệu mới
                        var newStas = new StatisticalModel
                        {
                            CreateAt = updateAtInUtc,
                            UpdatedAt = updateAtInUtc,
                            TotalWebsite = new TotalWebsiteInfoModel
                            {
                                TotalProducts = productResponse.totalProducts,
                                TotalSolds = productResponse.totalSolds,
                                TotalViews = productResponse.totalViews,
                                // Lấy dữ liệu từ tài liệu gần nhất nếu có
                                TotalRevenue = latestStas?.TotalWebsite.TotalRevenue ?? 0,
                                TotalUsers = latestStas?.TotalWebsite.TotalUsers ?? 0,
                            },
                        };

                        // Thêm tài liệu mới vào cơ sở dữ liệu
                        await _db.Statistical.AddAsync(_mapper.Map<Statistical>(newStas));
                        await _db.SaveChangesAsync();
                        response.Result = newStas;
                    }
                }
                else
                {
                    response.Message = "Invalid date format in productResponse.updateAt";
                    response.IsSuccess = false;
                }
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }
            return response;
        }

        public async Task<ResponseDTO> UpdateStastisticalUser(UserResponse userResponse)
        {
            ResponseDTO response = new();
            try
            {
                // Kiểm tra nếu `updateAt` là ngày hợp lệ
                if (DateTime.TryParse(userResponse.updateAt.ToString(), out DateTime searchDate))
                {
                    var updateAtInUtc = DateTime.SpecifyKind(userResponse.updateAt, DateTimeKind.Utc);
                    // Tìm tài liệu với ngày tạo tương ứng
                    var upStas = await _db.Statistical.FirstOrDefaultAsync(x =>
                        x.CreateAt.Year == searchDate.Year &&
                        x.CreateAt.Month == searchDate.Month &&
                        x.CreateAt.Day == searchDate.Day);

                    if (upStas != null)
                    {
                        // Cập nhật các trường nếu tài liệu đã tồn tại
                        upStas.TotalWebsite.TotalUsers = userResponse.totalUsers;
                        upStas.UpdatedAt = updateAtInUtc;
                        /*upStas.CreateAt = userResponse.updateAt;*/
                        _db.Update(upStas);
                        await _db.SaveChangesAsync();
                        response.Result = upStas;
                    }
                    else
                    {
                        // Tìm tài liệu gần nhất theo `CreateAt`
                        var latestStas = await _db.Statistical
                            .OrderByDescending(x => x.CreateAt)
                            .FirstOrDefaultAsync();

                        // Tạo tài liệu mới
                        var newStas = new StatisticalModel
                        {
                            CreateAt = updateAtInUtc,
                            UpdatedAt = updateAtInUtc,
                            TotalWebsite = new TotalWebsiteInfoModel
                            {
                                TotalProducts = latestStas?.TotalWebsite.TotalProducts ?? 0,
                                TotalSolds = latestStas?.TotalWebsite.TotalSolds ?? 0,
                                TotalViews = latestStas?.TotalWebsite.TotalViews ?? 0,
                                // Lấy dữ liệu từ tài liệu gần nhất nếu có
                                TotalRevenue = latestStas?.TotalWebsite.TotalRevenue ?? 0,
                                TotalUsers = userResponse.totalUsers
                            },
                        };

                        // Thêm tài liệu mới vào cơ sở dữ liệu
                        await _db.Statistical.AddAsync(_mapper.Map<Statistical>(newStas));
                        await _db.SaveChangesAsync();
                        response.Result = newStas;
                    }
                }
                else
                {
                    response.Message = "Invalid date format in productResponse.updateAt";
                    response.IsSuccess = false;
                }
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }
            return response;
        }

        public async Task<ResponseDTO> UpdateStastisticalOder(OrderResponse orderResponse)
        {
            ResponseDTO response = new();
            try
            {
                // Kiểm tra nếu `updateAt` là ngày hợp lệ
                if (DateTime.TryParse(orderResponse.updateAt.ToString(), out DateTime searchDate))
                {
                    var updateAtInUtc = DateTime.SpecifyKind(orderResponse.updateAt, DateTimeKind.Utc);
                    // Tìm tài liệu với ngày tạo tương ứng
                    var upStas = await _db.Statistical.FirstOrDefaultAsync(x =>
                        x.CreateAt.Year == searchDate.Year &&
                        x.CreateAt.Month == searchDate.Month &&
                        x.CreateAt.Day == searchDate.Day);

                    if (upStas != null)
                    {
                        // Cập nhật các trường nếu tài liệu đã tồn tại
                        upStas.TotalWebsite.TotalRevenue = (int)orderResponse.totalRevenue;
                        upStas.UpdatedAt = updateAtInUtc;
                        /*upStas.CreateAt = orderResponse.updateAt;*/
                        _db.Update(upStas);
                        await _db.SaveChangesAsync();
                        response.Result = upStas;
                    }
                    else
                    {
                        // Tìm tài liệu gần nhất theo `CreateAt`
                        var latestStas = await _db.Statistical
                            .OrderByDescending(x => x.CreateAt)
                            .FirstOrDefaultAsync();
                        
                        // Tạo tài liệu mới
                        var newStas = new StatisticalModel
                        {
                            CreateAt = updateAtInUtc,
                            UpdatedAt = updateAtInUtc,
                            TotalWebsite = new TotalWebsiteInfoModel
                            {
                                TotalProducts = latestStas?.TotalWebsite.TotalProducts ?? 0,
                                TotalSolds = latestStas?.TotalWebsite.TotalSolds ?? 0,
                                TotalViews = latestStas?.TotalWebsite.TotalViews ?? 0,
                                // Lấy dữ liệu từ tài liệu gần nhất nếu có
                                TotalRevenue = (int)orderResponse.totalRevenue,
                                TotalUsers = latestStas?.TotalWebsite.TotalRevenue ?? 0
                            },
                        };

                        // Thêm tài liệu mới vào cơ sở dữ liệu
                        await _db.Statistical.AddAsync(_mapper.Map<Statistical>(newStas));
                        await _db.SaveChangesAsync();
                        response.Result = newStas;
                    }
                }
                else
                {
                    response.Message = "Invalid date format in productResponse.updateAt";
                    response.IsSuccess = false;
                }
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }
            return response;
        }

        public async Task<ResponseDTO> GetStastisticalByUser(TotalGroupByUserRequest totalGroupByUserRequest)
        {
            ResponseDTO response = new();
            try
            {
                // Đồng thời thực hiện cả OrderData và ProductData
                var orderDataTask = OrderData(totalGroupByUserRequest);
                var productDataTask = ProductData(totalGroupByUserRequest);

                // Chờ cả hai task hoàn thành
                await Task.WhenAll(orderDataTask, productDataTask);

                var orderdata = await orderDataTask;
                var productdata = await productDataTask;

                var total = new TotalGroupByUserResponse()
                {
                    orderdata = orderdata,
                    productdata = productdata
                };

                if (orderdata != null && productdata != null)
                {
                    response.IsSuccess = true;
                    response.Result = total;
                }
                else
                {
                    response.IsSuccess = false;
                    response.Message = "Data missing.";
                }
            }
            catch (Exception ex)
            {
                // Xử lý lỗi nếu cần
                response.IsSuccess = false;
                response.Message = $"An error occurred: {ex.Message}";
            }

            return response;
        }

        private async Task<OrderGroupByUserData> OrderData(TotalGroupByUserRequest totalGroupByUserRequest)
        {
            var response = new OrderGroupByUserData();
            try
            {
                int maxRetryAttempts = 3;
                int retryCount = 0;
                bool isCompleted = false; // Biến đánh dấu hoàn thành
                while (retryCount < maxRetryAttempts && !isCompleted)
                {
                    _message.SendingMessageStastisticalGroupByUser(totalGroupByUserRequest);

                    var orderTcs = new TaskCompletionSource<bool>();

                    // Đăng ký sự kiện nhận dữ liệu order
                    _message.OnOrderResponseReceived += (orderResponse) =>
                    {
                        Console.WriteLine($"Received order response: {orderResponse.Revenue}");
                        response.Revenue = orderResponse.Revenue;
                        // Đánh dấu task đã hoàn thành
                        if (!orderTcs.Task.IsCompleted)
                        {
                            orderTcs.SetResult(true);
                            Console.WriteLine("SetResult done");
                        }
                    };

                    // Tạo task hẹn giờ timeout
                    var timeoutTask = Task.Delay(5000);
                    var completedTask = await Task.WhenAny (orderTcs.Task, timeoutTask);

                    if (completedTask == timeoutTask)
                    {
                        Console.WriteLine($"Attempt {retryCount + 1} failed due to timeout.");
                        retryCount++;

                        if (retryCount >= maxRetryAttempts)
                        {
                           
                            return null;
                        }
                        continue;
                    }
                    isCompleted = true;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return response;
        }
        private async Task<ProductGroupByUserData> ProductData(TotalGroupByUserRequest totalGroupByUserRequest)
        {
            var response = new ProductGroupByUserData();
            try
            {
                int maxRetryAttempts = 3;
                int retryCount = 0;
                bool isCompleted = false; // Biến đánh dấu hoàn thành
                while (retryCount < maxRetryAttempts && !isCompleted)
                {
                    _message.SendingMessageStastisticalGroupByUser(totalGroupByUserRequest);

                    var orderTcs = new TaskCompletionSource<bool>();

                    // Đăng ký sự kiện nhận dữ liệu order
                    _message.OnProductResponseReceived += (orderResponse) =>
                    {
                        Console.WriteLine($"Received product response: {orderResponse.Views}, {orderResponse.Products}, {orderResponse.Solds}");
                        response.Views = orderResponse.Views;
                        response.Solds = orderResponse.Solds;
                        response.Products = orderResponse.Products;
                        if (!orderTcs.Task.IsCompleted)
                        {
                            orderTcs.SetResult(true);  // Đánh dấu khi nhận dữ liệu order
                        }
                    };

                    // Tạo task hẹn giờ timeout
                    var timeoutTask = Task.Delay(5000);
                    var completedTask = await Task.WhenAny(orderTcs.Task, timeoutTask);

                    if (completedTask == timeoutTask)
                    {
                        Console.WriteLine($"Attempt {retryCount + 1} failed due to timeout.");
                        retryCount++;

                        if (retryCount >= maxRetryAttempts)
                        {

                            return null;
                        }
                        continue;
                    }
                    isCompleted = true;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return response;
        }
    }
}
