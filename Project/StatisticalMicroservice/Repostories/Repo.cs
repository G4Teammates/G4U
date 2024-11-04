using AutoMapper;
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


        public Repo(IConfiguration configuration, StatisticalDbcontext db, IMapper mapper)
        {
            _configuration = configuration;
            _db = db;
            _mapper = mapper;
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

    }
}
