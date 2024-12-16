using AutoMapper;
using Azure;
using CategoryMicroservice.DBContexts;
using CategoryMicroservice.DBContexts.Entities;
using CategoryMicroservice.DBContexts.Enum;
using CategoryMicroservice.Models;
using CategoryMicroservice.Models.DTO;
using CategoryMicroservice.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
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

        public async Task<bool> CheckCategorysByCategoryNameAsync(ICollection<CategoryNameModel> categories)
        {
            // Lấy danh sách tên các danh mục cần kiểm tra
            var categoryNames = categories.Select(c => c.CategoryName.ToLower()).ToList();

            // Lấy danh sách các tên danh mục có trong cơ sở dữ liệu (không phân biệt chữ hoa, chữ thường)
            var existingCategoryNames = await _db.Categories
                .Where(c => categoryNames.Contains(c.Name.ToLower()))
                .Select(c => c.Name.ToLower())
                .ToListAsync();

            // Kiểm tra xem tất cả tên danh mục có tồn tại trong cơ sở dữ liệu không
            bool allCategoriesExist = categoryNames.All(name => existingCategoryNames.Contains(name));

            return allCategoriesExist;
        }

        /*public IEnumerable<Category> Categories => _db.Categories.ToList();*/

        public async Task<ResponseModel> CreateCategory(CreateCategoryModel Category)
        {
            ResponseModel response = new();
            try
            {
                if(!IsContentAppropriate(Category.Name) || !IsContentAppropriate(Category.Description))
                {
                    response.IsSuccess = false;
                    response.Message = "The Content is not for community";
                    return response;
                }
                else
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
                    /*_message.SendingMessage(name);*/ // Gửi message

                    _message.SendingMessage(name, "delete_category", "delete_category_queue", "delete_category_queue", ExchangeType.Direct, true, false, false, false);// Gửi message

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
                    var resultByName = Category.Where(x => x.Name.ToLower().Contains(searchstring.ToLower())).ToPagedList(page, pageSize);
                    response.Result = _mapper.Map<List<Category>>(resultByName);
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
                if (!IsContentAppropriate(Categrori.Name) || !IsContentAppropriate(Categrori.Description))
                {
                    response.IsSuccess = false;
                    response.Message = "The Content is not for community";
                    return response;
                }
                else
                {
                    var upCate = await _db.Categories.FindAsync(Categrori.Id);
                    if (upCate != null)
                    {
                        // Kiểm tra xem danh mục đã tồn tại chưa
                        var checkExist = await _db.Categories.AnyAsync(x => x.Name == Categrori.Name && x.Id != Categrori.Id);
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
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }
            return response;
        }

        public async Task<ResponseModel> GetCateByStatus(int Status)
        {
            ResponseModel response = new();
            try
            {
                var Cates = await _db.Categories.Where(p => p.Status == (CategoryStatus)Status).ToListAsync();

                if (Cates.Count == 0)
                {
                    response.IsSuccess = false;
                    response.Message = "This Status doesn't have any Cate";
                    return response;
                }

                response.Message = "Get Cates Success";
                response.Result = _mapper.Map<ICollection<Category>>(Cates);
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }
            return response;
        }
        public async Task<ResponseModel> GetCateByType(int Type)
        {
            ResponseModel response = new();
            try
            {
                var Cates = await _db.Categories.Where(p => p.Type == (CategoryType)Type).ToListAsync();

                if (Cates.Count == 0)
                {
                    response.IsSuccess = false;
                    response.Message = "This Type doesn't have any Cate";
                    return response;
                }

                response.Message = "Get Cates Success";
                response.Result = _mapper.Map<ICollection<Category>>(Cates);
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }
            return response;
        }

        #region method
        private bool IsContentAppropriate(string content)
        {
            // Kiểm tra xem content có chứa từ bị cấm hay không
            foreach (var bannedWord in _bannedWords)
            {
                if (content.Contains(bannedWord, StringComparison.OrdinalIgnoreCase))
                {
                    return false; // Nội dung không phù hợp
                }
            }
            return true; // Nội dung phù hợp
        }
        private readonly List<string> _bannedWords = new List<string>
        {
            // Tiếng Việt - Nội dung không phù hợp và các biến thể lách luật
            "chết tiệt", "chếttttt", "đồ ngu", "ngu ngốc", "nguuu", "khốn nạn", "khónnnn", "đĩ", "đĩiiiiii", "mẹ mày", "mẹeeeee",
            "cút đi", "cúttttt", "xấu xa", "xấuuuu", "vô học", "vôooo", "biến đi", "biếnnnn", "đồ chó", "chóooo", "khốn kiếp",
            "kiếppppp", "bố láo", "láooooo", "cái l*n", "vãi l*n", "mẹ kiếp", "đ**", "d*m", "chửi thề", "đồ phản bội", "phản boiiii",
            "phá hoại", "hoạiiii", "phản động", "phảnnn", "vô dụng", "dụnggggg", "mất dạy", "dạyyyy", "cặn bã", "cặnnnnn",
            "thối nát", "náttttt", "biến thái", "biếnnnn", "vô liêm sỉ", "hèn hạ", "ngốc nghếch", "ngu si", "thô tục", "tụcccc",
            "đầu bò", "đần độn", "dốtttt", "thần kinh", "đầu gấu", "đồ ngớ ngẩn", "tởm lợm", "chửi tục", "cút xéo", "ngớ ngẩn",
            "tởm", "ăn hại", "ăn cắp", "vô lại", "đê tiện", "xấu xí", "mất nết", "vô tích sự", "vô giáo dục", "thô lỗ", "phản bội",
            "đụ", "đụ má", "đụ mẹ", "đụmmmm", "dụ nhau", "cu", "cu to", "cặc", "cặcccc", "c*k", "cc", "chịch", "chịch nhau", "chịcc",
            "nứng", "nứng nà", "nứng ơi", "dâm đãng", "d*m đãng", "d*m dục", "địt", "địtttt", "đjt", "dm", "dm bạn", "ch*ch","lồn",
            "lồnnn", "vãi lồn", "cái lồn", "l*n", "loz", "lozzzz", "l*nz", "l**z", "lol", "lồllll", "cặc", "cặkkkk", "cạkkkk", "cặk",
            "cặt", "cậtttt", "c*c", "c*k", "cc", "cặccc", "cắcccc", "cặccccc", "cứt", "cức", "cứtttt", "cứcccc", "đổ cứt", "đổ cức",
            "óc chó", "óc ch*o", "óc chooo", "óc c*o", "chó óc", "c*n l*n", "l*l", "đồ loz", "thằng lol", "đồ lol", "ngu lol", "đồ c*o",
            "ăn lol", "đồ l*n", "loằnn", "ngu loz", "vô loz", "ph* loz", "lốnnnn",


            // Tiếng Anh - Offensive and inappropriate words and variations
            "idiot", "idiotttttt", "stupid", "stupidddd", "moron", "moroNnnn", "jerk", "jerkkkk", "dumb", "dumBBB", "loser",
            "looooser", "shut up", "shuttupppp", "trash", "trasshhhhh", "fool", "fooool", "freak", "freeeak", "pervert", "pErverTttt",
            "slut", "sluTtt", "bitch", "biatch", "whore", "wh0r3", "pussy", "pussssyyy", "douche", "douchee", "retard", "retaaard",
            "scumbag", "sCumbagg", "twat", "twatttt", "dirtbag", "dirtttt", "degenerate", "dEgeneRate", "hypocrite", "hypooo",
            "worthless", "worTHLessss", "scum", "SCumm", "dumbbell", "dUmBBBell", "retard", "retaRRRd", "slob", "sloBBb",
            "creep", "creeeeeep", "loser", "looooser", "psychopath", "PSYchooooo", "trash", "trasshhhhh", "cringe", "cRingEee",

            // Thêm từ chửi thề phổ biến bằng tiếng Anh
            "fuck", "f*ck", "f***", "f**k", "fuuuck", "fuuuccckkk", "fking", "fkng", "fck", "sh*t", "shiiiit", "shyt", "shttt",
            "bullshit", "b*llshit", "bsh*t", "motherfucker", "m*therf*cker", "mf", "mfker", "screw you", "scrw you", "scrwy",
            "asshole", "assh*le", "a$$hole", "a$$", "arsehole", "dickhead", "d*ckhead", "dikhed", "d*uche", "douchebag", "doucheb*g",
            "wtf", "wtfff", "wtfuu", "damn", "d*mn", "d4mn", "hell", "he11", "helll", "h3ll", "jackass", "j4ckass", "jerk", "biatch",
            "b*tch", "b1tch", "beyotch", "beeotch", "idiot", "dumbass", "dumbf*ck", "dummkopf",
    
            // Tiếng Tây Ban Nha - Palabras ofensivas y inapropiadas con variaciones
            "idiota", "idiotaaaa", "estúpido", "estuPIDoooo", "imbécil", "imbecilLLLLL", "basura", "baSURaaa", "mierda", "mierdddddaaa",
            "puta", "putaaaaaa", "carajo", "cArAjoOOO", "tonto", "toooNTOO", "pervertido", "pervEERTido", "sucio", "SUcioooo", "malnacido",
            "malnACido", "pendejo", "pendEjjjjO", "miserable", "MIsERableeeee", "patán", "patAnNNN", "idiotez", "idioteZZZZ", "vago",
            "vagooo", "asno", "ASnooooo", "estúpida", "estuPIDa", "imbeciloide", "imbeciLoIDe", "cabeza hueca", "caBeZA hUeCA",
            "idiotez", "idioTeZZ", "inepto", "inePTTTo", "hipócrita", "HIPoCritaA", "maleducado", "malEDUcaDo", "sucia", "suCiaaaa",
    
            // Tiếng Pháp - Mots offensants et inappropriés et leurs variations
            "idiot", "IDIOTTTT", "imbécile", "IMBECiLeee", "connard", "CONNardDD", "salaud", "SAlAUDddd", "ordure", "OrDureEEE",
            "merde", "MERRRRDDE", "putain", "PUTainNNN", "crétin", "CRETiiiiNNN", "débile", "dEBILEee", "abruti", "ABRUtiiiiii",
            "goujat", "goujATttt", "dégénéré", "DEGENEREEEEE", "salopard", "saloPARDddd", "connNNNN", "imbécile heureux",
            "IMBECILEEEEE", "branleur", "branleEEEur", "impoli", "IMPolIeee", "inculte", "INculteeeee", "imbu de lui-même",
            "imbUUU", "fainéant", "fAINNEEANTtt", "paresseux", "PAREsSEuxxx", "épais", "EPaaaiss", "salopard", "saloPARDD",
    
            // Tiếng Đức - Beleidigende Wörter mit Varianten
            "idiot", "IDIOTTtt", "dummkopf", "DUMMKOPFff", "arschloch", "ARSCHloCH", "schlampe", "SCHLAMpppeee", "verdammt",
            "verDAMMMT", "scheiße", "SCHHHHHeisse", "hure", "HUUURE", "miststück", "MiSTStuckkk", "dumm", "DUMMMMM",
            "blödmann", "BLÖDMANNN", "verrückt", "VERRUCKTTT", "krank", "KRRRANK", "ekelhaft", "EKELHAFtTT", "pervers",
            "pERverseee", "idiot", "IDIOTEEE", "krüppel", "KRUPPEL", "grobian", "GROOOOBIAN", "unmensch", "UNMENSCHHH",
    
            // Tiếng Nhật - 不適切な言葉とそのバリエーション
            "馬鹿", "バカaaaa", "くそ", "くそおおお", "アホ", "あほほほほ", "死ね", "しねえええ", "気持ち悪い", "気持ちわる",
            "最低", "さいてい", "嫌い", "きらい", "消えろ", "きえろお", "野郎", "やろううう", "バカたれ", "たれええ",
            "ゴミ", "ごみいいい", "アホンダラ", "あほんだら", "サイテー", "さいてええ", "うざい", "うざあああ", "やかましい",
            "やかまああ", "詐欺", "さぎいいい", "馬鹿げた", "ばかげたああ", "不愉快", "ふゆかいい", "くたばれ", "くたばれえ",
            "馬鹿馬鹿しい", "ばかばかしいいい"
        };
        #endregion
    }
}
