using AutoMapper;
using CategoryMicroservice.DBContexts;
using CategoryMicroservice.DBContexts.Entities;
using CategoryMicroservice.Models;
using CategoryMicroservice.Models.DTO;
using CategoryMicroservice.Repositories.Interfaces;

namespace CategoryMicroservice.Repositories.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly CategoryDbContext _db;
        private readonly IMapper _mapper;

        private readonly IMessage _message;
        public CategoryService(CategoryDbContext db, IMapper mapper, IMessage message)
        {
            _db = db;
            _mapper = mapper;
            _message = message;
        }

        public IEnumerable<Category> Categories => _db.Categories.ToList();

        public Category CreateCategory(CreateCategoryModel Category)
        {
            var newCate = new CategoryModel
            {
                Name = Category.Name,
                Type = Category.Type,
                Description = Category.Description,
                Status = Category.Status
            };
            var CateEnti = _mapper.Map<Category>(newCate);
            _db.Add(CateEnti);
            _db.SaveChanges();
            return CateEnti;
        }

        public async Task<string> DeleteCategory(string id)
        {
            var cate = await GetById(id);
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


        public async Task<Category> GetById(string id) => _db.Categories.Find(id);

        public IEnumerable<Category> Search(string searchstring)
        {
            var Category = _db.Categories.AsQueryable();
            if (!string.IsNullOrEmpty(searchstring))
            {
                // Tìm kiếm theo tên sản phẩm
                var resultByName = Category.Where(x => x.Name.Contains(searchstring));
                if (resultByName.Any())
                {
                    return resultByName;
                }
            }

            // Nếu không có kết quả nào khớp với điều kiện tìm kiếm, trả về danh sách trống
            return new List<Category>();
        }

        public async Task<Category> UpdateCategrori(CategoryModel Categrori)
        {
            var upCate  = await GetById(Categrori.Id);    
             if(upCate != null)
            {
                upCate.Name = Categrori.Name;
                upCate.Status = Categrori.Status;
                upCate.Description = Categrori.Description; 
                upCate.Type = Categrori.Type;
            }
             var cateEnti = _mapper.Map<Category>(upCate);
            _db.Update(cateEnti);
            await _db.SaveChangesAsync();
            return cateEnti;
        }
    }
}
