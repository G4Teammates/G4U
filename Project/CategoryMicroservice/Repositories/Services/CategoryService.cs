using AutoMapper;
using Azure;
using CategoryMicroservice.DBContexts;
using CategoryMicroservice.DBContexts.Entities;
using CategoryMicroservice.Models;
using CategoryMicroservice.Models.DTO;
using CategoryMicroservice.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using X.PagedList.Extensions;

namespace CategoryMicroservice.Repositories.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly CategoryDbContext _db;
        private readonly IMapper _mapper;

        private readonly IMessage _message;
        private readonly IHelperService _helperService;
        public CategoryService(CategoryDbContext db, IMapper mapper, IMessage message, IHelperService helperService)
        {
            _db = db;
            _mapper = mapper;
            _message = message;
            _helperService = helperService;
        }

        /*public IEnumerable<Category> Categories => _db.Categories.ToList();*/

        public async Task<ResponseModel> CreateCategory(CreateCategoryModel Category)
        {
            ResponseModel response = new();
            try
            {
                // Kiểm tra xem danh mục đã tồn tại chưa
                var checkExist = await _db.Categories.AnyAsync(x => x.Name == Category.Name);
                
                if (checkExist)
                {
                    response.IsSuccess = false;
                    response.Message = "The Category already exists";
                }
                else
                {
                    var newCate = new CategoryModel
                    {
                        Name = Category.Name,
                        Type = Category.Type,
                        Description = Category.Description,
                        Status = Category.Status
                    };
                    var CateEnti = _mapper.Map<Category>(newCate);
                    _db.AddAsync(CateEnti);
                    _db.SaveChangesAsync();
                    response.Result = CateEnti;
                }
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }
            return response;
        }

        public async Task<string> DeleteCategory(string id)
        {
            var cate = await _db.Categories.FindAsync(id);
            string name = cate.Name;
            if (cate != null)
            {
                int maxRetryAttempts = 3; // Số lần thử lại tối đa
                int retryCount = 0; // Đếm số lần thử lại
                bool _canDelete = false; // Khởi tạo biến _canDelete
                bool isCompleted = false; // Biến đánh dấu hoàn thành

                while (retryCount < maxRetryAttempts && !isCompleted)
                {
                    _message.SendingMessage(name); // Gửi message

                    var tcs = new TaskCompletionSource<bool>(); // Tạo TaskCompletionSource

                    // Đăng ký sự kiện để gán giá trị
                    _message.OnCategoryResponseReceived += (response) =>
                    {
                        Console.WriteLine($"Received response: {response.CateName}, CanDelete: {response.CanDelete}");
                        _canDelete = response.CanDelete; // Gán giá trị vào biến

                        // Đánh dấu task đã hoàn thành
                        if (!tcs.Task.IsCompleted)
                        {
                            tcs.SetResult(_canDelete);
                            Console.WriteLine("SetResult done");
                        }
                    };

                    // Chờ cho sự kiện được kích hoạt hoặc timeout sau 5 giây
                    var timeoutTask = Task.Delay(5000); // Thời gian timeout là 5 giây
                    var completedTask = await Task.WhenAny(tcs.Task, timeoutTask);

                    // Nếu timeout xảy ra
                    if (completedTask == timeoutTask)
                    {
                        Console.WriteLine($"Attempt {retryCount + 1} failed due to timeout.");
                        retryCount++; // Tăng số lần thử lại

                        if (retryCount >= maxRetryAttempts)
                        {                        
                            return "The system encountered a problem while performing deletion"; // Trả về lỗi sau khi hết số lần thử lại
                        }

                        // Nếu chưa đạt giới hạn retry, tiếp tục vòng lặp
                        continue;
                    }

                    // Nếu có phản hồi, thoát khỏi vòng lặp
                    isCompleted = true;

                    // Xử lý kết quả khi có phản hồi từ RabbitMQ
                    if (_canDelete)
                    {
                        _db.Categories.Remove(cate);
                        await _db.SaveChangesAsync();
                        return "Deleted successfully";
                    }
                    else
                    {
                        return "Foreign key error: Cannot delete because there are related records.";
                    }
                }
            }
            else
            {
                return "Can't find category";
            }
            // Nếu vòng lặp kết thúc mà không trả về
            return "Unknown error occurred during deletion.";
        }

        public async Task<ResponseModel> GetAll(int page, int pageSize)
        {
            ResponseModel response = new();
            try
            {
                var Cate = await _db.Categories.ToListAsync();
                if (Cate != null)
                {
                    response.Result = _mapper.Map<ICollection<Category>>(Cate).ToPagedList(page, pageSize);
                }
                else
                {
                    response.IsSuccess = false;
                    response.Message = "Not found any Category";
                }
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }
            return response;
        }

        public async Task<ResponseModel> GetById(string id)
        {
            ResponseModel response = new();
            try
            {
                var Cate = await _db.Categories.FindAsync(id);
                if (Cate != null)
                {
                    response.Result = _mapper.Map<Category>(Cate);
                }
                else
                {
                    response.IsSuccess = false;
                    response.Message = "Not found any Category";
                }
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }
            return response;
        }

        public async Task<ResponseModel> Search(string searchstring, int page, int pageSize)
        {
            ResponseModel response = new();
            try
            {
                var Category = _db.Categories.AsQueryable();
                if (!string.IsNullOrEmpty(searchstring))
                {
                    var resultByName = Category.Where(x => x.Name.Contains(searchstring)).ToPagedList(page, pageSize);
                    response.Result = _mapper.Map<Category>(resultByName);
                }
                else
                {
                    response.IsSuccess = false;
                    response.Message = "Search string is null";
                }
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }
            return response;
        }

        public async Task<ResponseModel> UpdateCategrori(CategoryModel Categrori)
        {
            ResponseModel response = new();
            try
            {
                var upCate = await _db.Categories.FindAsync(Categrori.Id);
                if (upCate != null)
                {
                    // Kiểm tra xem danh mục đã tồn tại chưa
                    var checkExist = await _db.Categories.AnyAsync(x => x.Name == Categrori.Name);
                    if (checkExist)
                    {
                        response.IsSuccess = false;
                        response.Message = "The Category already exists";
                    }
                    else
                    {
                        upCate.Name = Categrori.Name;
                        upCate.Status = Categrori.Status;
                        upCate.Description = Categrori.Description;
                        upCate.Type = Categrori.Type;

                        var CateEnti = _mapper.Map<Category>(upCate);
                        _db.Update(CateEnti);
                        await _db.SaveChangesAsync();
                        response.Result = CateEnti;
                    }
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
