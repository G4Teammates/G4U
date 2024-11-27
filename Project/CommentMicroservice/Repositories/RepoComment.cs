using AutoMapper;
using Azure;
using CommentMicroservice.DBContexts;
using CommentMicroservice.DBContexts.Entities;
using CommentMicroservice.Models;
using CommentMicroservice.Models.DTO;
using CommentMicroservice.Models.Message;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using X.PagedList.Extensions;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CommentMicroservice.Repositories
{
    public class RepoComment : IRepoComment
    {
        private readonly CommentDbContext _db;
        private readonly IMapper _mapper;
        private readonly IMessage _message;
        public RepoComment(CommentDbContext db, IMapper mapper, IMessage message)
        {
            _db = db;
            _mapper = mapper;
            _message = message;
        }
        public async Task<ResponseModel> GetAll(int page, int pageSize)
        {
            ResponseModel response = new();
            try
            {
                var Comm = await _db.Comments.ToListAsync();
                if (Comm != null)
                {
                    response.Result = _mapper.Map<ICollection<Comment>>(Comm).ToPagedList(page, pageSize);
                }
                else
                {
                    response.IsSuccess = false;
                    response.Message = "Not found any Comment";
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
                var Comm = await _db.Comments.FindAsync(id);
                if (Comm != null)
                {
                    response.Result = _mapper.Map<Comment>(Comm);
                }
                else
                {
                    response.IsSuccess = false;
                    response.Message = "Not found any Commemt";
                }
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }
            return response;
        }
        public async Task<ResponseModel> GetListById(string id, int page, int pageSize)
        {
            ResponseModel response = new();
            try
            {
                // Lấy danh sách tất cả các comment từ database
                var allComments = await _db.Comments.ToListAsync();

                // Tìm tất cả các comment con, cháu, và sâu hơn nữa
                var commentsHierarchy = GetCommentsHierarchy(allComments, id);

                if (commentsHierarchy.Any())
                {
                    // Map kết quả và phân trang
                    response.Result = _mapper.Map<ICollection<Comment>>(commentsHierarchy).ToPagedList(page, pageSize);
                }
                else
                {
                    response.IsSuccess = false;
                    response.Message = "Not found any Comment";
                }
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }
            return response;
        }

        public async Task<ResponseModel> CreateComment(string userId, CreateCommentDTO Comment)
        {
            #region declaration
            ResponseModel response = new();
            int maxRetryAttempts = 3; // Số lần thử lại tối đa
            int retryCount = 0; // Đếm số lần thử lại
            bool _canCreate = false; // Khởi tạo biến _canDelete
            bool isCompleted = false; // Biến đánh dấu hoàn thành
            #endregion

            while (retryCount < maxRetryAttempts && !isCompleted)
            {
                #region send message
                var data = new CheckPurchasedSending
                {
                    UserId = userId,
                    ProductId = Comment.ProductId
                };
                _message.SendingMessageCheckPurchased(data); // Gửi message
                #endregion

                #region wait for response and process response from rabbitmq
                var tcs = new TaskCompletionSource<bool>(); // Tạo TaskCompletionSource

                // Đăng ký sự kiện để gán giá trị
                _message.OnResponseReceived += (response) =>
                {
                    Console.WriteLine($"Received response: IsPurchased: {response.IsPurchased}");
                    _canCreate = response.IsPurchased; // Gán giá trị vào biến

                    // Đánh dấu task đã hoàn thành
                    if (!tcs.Task.IsCompleted)
                    {
                        tcs.SetResult(_canCreate);
                        Console.WriteLine("SetResult done");
                    }
                };
                #endregion

                #region handle time out
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
                        response.IsSuccess = false;
                        response.Message = "The system encountered a problem while performing deletion";
                        return response; // Trả về lỗi sau khi hết số lần thử lại
                    }

                    // Nếu chưa đạt giới hạn retry, tiếp tục vòng lặp
                    continue;
                }

                // Nếu có phản hồi, thoát khỏi vòng lặp
                isCompleted = true;
                #endregion

                #region Process results when there is a response from RabbitMQ
                if (_canCreate)
                {
                    #region Create Comment
                    try
                    {
                        // Kiểm duyệt nội dung
                        if (!IsContentAppropriate(Comment.Content))
                        {
                            response.IsSuccess = false;
                            response.Message = "The Content is not for community";
                        }
                        else
                        {
                            var newComm = new CommentModel
                            {
                                Content = Comment.Content,
                                NumberOfLikes = 0,
                                NumberOfDisLikes = 0,
                                UserLikes = new List<UserLikesModel>(),
                                UserDisLikes = new List<UserDisLikesModel>(),
                                UserName = Comment.UserName,
                                Status = Comment.Status,
                                ProductId = Comment.ProductId,
                                ParentId = Comment.ParentId,
                                CreatedAt = DateTime.Now,
                                UpdatedAt = DateTime.Now
                            };
                            var CommEnti = _mapper.Map<Comment>(newComm);
                            _db.AddAsync(CommEnti);
                            _db.SaveChangesAsync();
                            response.Result = CommEnti;
                        }
                    }
                    catch (Exception ex)
                    {
                        response.IsSuccess = false;
                        response.Message = ex.Message;
                    }
                    return response;
                    #endregion
                }
                else
                {
                    response.IsSuccess = false;
                    response.Message = "Error: You have not purchased this game yet.";
                    return response; // Trả về lỗi sau khi hết số lần thử lại
                }
                #endregion
            }
            // Nếu vòng lặp kết thúc mà không trả về
            response.IsSuccess = false;
            response.Message = "Unknown error occurred during deletion.";
            return response; // Trả về lỗi sau khi hết số lần thử lại
        }

        public async Task<ResponseModel> UpdateComment(CommentModel Comment)
        {
            ResponseModel response = new();
            try
            {
                var upComm = await _db.Comments.FindAsync(Comment.Id);
                if (upComm != null)
                {

                    upComm.Content = Comment.Content;
                    upComm.NumberOfLikes = Comment.NumberOfLikes;
                    upComm.NumberOfDisLikes = Comment.NumberOfDisLikes;
                    upComm.UserLikes = _mapper.Map<ICollection<UserLikes>>(Comment.UserLikes);
                    upComm.UserDisLikes = _mapper.Map<ICollection<UserDisLikes>>(Comment.UserDisLikes);
                    upComm.UserName = Comment.UserName;
                    upComm.Status = Comment.Status;
                    upComm.ProductId = Comment.ProductId;
                    upComm.ParentId = Comment.ParentId;
                    upComm.CreatedAt = Comment.CreatedAt;
                    upComm.UpdatedAt = Comment.UpdatedAt;

                    var CommEnti = _mapper.Map<Comment>(upComm);
                    _db.Update(CommEnti);
                    await _db.SaveChangesAsync();
                    response.Result = CommEnti;               
                }
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }
            return response;
        }

        public async Task<ResponseModel> DeleteComment(string id)
        {
            ResponseModel response = new();
            try
            {
                var comments = await GetListById(id,1,9999);
                var list = _mapper.Map<ICollection<Comment>>(comments.Result);
                if (comments.Result != null)
                {
                    _db.Comments.RemoveRange(list);
                    await _db.SaveChangesAsync();
                    response.Message = "Delete successfully";
                }
                else
                {
                    response.IsSuccess = false;
                    response.Message = "Not found any Comment";
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
                var Products = _db.Comments.AsQueryable();
                if (!string.IsNullOrEmpty(searchstring))
                {
                    var resultByName = Products.Where(x => x.UserName.Contains(searchstring));                 
                    if (resultByName.Any())
                    {
                        response.Result = _mapper.Map<ICollection<Comment>>(resultByName).ToPagedList(page, pageSize);
                    }
                    // Kiểm tra nếu searchstring chỉ là năm
                    if (searchstring.Length == 4 && int.TryParse(searchstring, out int year))
                    {
                        var resultByYear = Products.Where(x => x.CreatedAt.Year == year);
                        if (resultByYear.Any())
                        {
                            response.Result = _mapper.Map<ICollection<Comment>>(resultByYear).ToPagedList(page,pageSize);
                        }
                    }
                    else
                    {
                        // Kiểm tra nếu searchstring là ngày/tháng/năm
                        DateTime searchDate;
                        if (DateTime.TryParse(searchstring, out searchDate))
                        {
                            // Ngày, tháng, năm
                            var resultByDate = Products.Where(x => x.CreatedAt.Year == searchDate.Year && x.CreatedAt.Month == searchDate.Month && x.CreatedAt.Day == searchDate.Day);
                            if (resultByDate.Any())
                            {
                                response.Result = _mapper.Map<ICollection<Comment>>(resultByDate).ToPagedList(page, pageSize);
                            }
                            // Tháng và năm
                            var resultByMonthYear = Products.Where(x => x.CreatedAt.Year == searchDate.Year && x.CreatedAt.Month == searchDate.Month);
                            if (resultByMonthYear.Any())
                            {
                                response.Result = _mapper.Map<ICollection<Comment>>(resultByMonthYear).ToPagedList(page, pageSize);
                            }
                        }
                    }
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
        public async Task<ResponseModel> GetByproductId(string productId, int page, int pageSize)
        {
            ResponseModel response = new();
            try
            {
                var Comm = await _db.Comments.Where(c => c.ProductId == productId && c.ParentId==null).ToListAsync();
                if (Comm != null)
                {
                    response.Result = _mapper.Map<ICollection<Comment>>(Comm).ToPagedList(page, pageSize);
                }
                else
                {
                    response.IsSuccess = false;
                    response.Message = "Not found any Commemt";
                }
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }
            return response;
        }

        public async Task<ResponseModel> GetByParentId(string Parentid, int page, int pageSize)
        {
            ResponseModel response = new();
            try
            {
                var Comm = await _db.Comments.Where(c => c.ParentId == Parentid).ToListAsync();

                if (Comm != null)
                {
                    response.Result = _mapper.Map<ICollection<Comment>>(Comm).ToPagedList(page,pageSize);
                }
                else
                {
                    response.IsSuccess = false;
                    response.Message = "Not found any Commemt reply";
                }
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }
            return response;
        }

        public async Task<ResponseModel> IncreaseLike(string commentId, UserLikesModel userLike)
        {
            ResponseModel response = new();
            var comment = await _db.Comments.FindAsync(commentId);
            var checkExitUser = comment?.UserLikes.FirstOrDefault(ul => ul.UserName == userLike.UserName);
            if (checkExitUser == null)
            {
                try
                {
                    if (comment != null)
                    {
                        comment.NumberOfLikes++;
                        comment.UserLikes.Add(_mapper.Map<UserLikes>(userLike));
                        _db.Comments.Update(comment);
                        await _db.SaveChangesAsync();

                        response.IsSuccess = true;
                        response.Result = comment.NumberOfLikes;
                        response.Message = "Increased like count successfully.";
                    }
                    else
                    {
                        response.IsSuccess = false;
                        response.Message = "Comment not found.";
                    }
                }
                catch (Exception ex)
                {
                    response.IsSuccess = false;
                    response.Message = ex.Message;
                }
                return response;
            }
            else
            {
                response.IsSuccess = true;
                response.Result = comment.NumberOfLikes;
                response.Message = "You liked this comment before.";
                return response;
            }
        }

        public async Task<ResponseModel> DecreaseLike(string commentId , UserDisLikesModel userDisLike)
        {
            ResponseModel response = new();
            var comment = await _db.Comments.FindAsync(commentId);
            var checkExitUser = comment?.UserDisLikes.FirstOrDefault(ul => ul.UserName == userDisLike.UserName);
            if (checkExitUser == null)
            {
                try
                {
                    if (comment != null)
                    {
                        // Tăng số lượng dislike thay vì giảm số lượng like
                        comment.NumberOfDisLikes++;
                        comment.UserDisLikes.Add(_mapper.Map<UserDisLikes>(userDisLike));
                        _db.Comments.Update(comment);
                        await _db.SaveChangesAsync();

                        response.IsSuccess = true;
                        response.Result = comment.NumberOfDisLikes;
                        response.Message = "Increased dislike count successfully.";
                    }
                    else
                    {
                        response.IsSuccess = false;
                        response.Message = "Comment not found.";
                    }
                }
                catch (Exception ex)
                {
                    response.IsSuccess = false;
                    response.Message = ex.Message;
                }
                return response;
            }
            else
            {
                response.IsSuccess = true;
                response.Result = comment.NumberOfDisLikes;
                response.Message = "You disliked this comment before.";
                return response;
            }

        }



        #region method

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
            "goujat", "goujATttt", "dégénéré", "DEGENEREEEEE", "salopard", "saloPARDddd", "con", "connNNNN", "imbécile heureux",
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

        private List<Comment> GetCommentsHierarchy(List<Comment> allComments, string parentId)
        {
            var result = new List<Comment>();
            var stack = new Stack<Comment>();

            // Tìm các comment gốc khớp với parentId và thêm vào stack
            var rootComments = allComments.Where(c => c.Id == parentId).ToList();
            foreach (var root in rootComments)
            {
                stack.Push(root);
                result.Add(root); // Thêm comment gốc vào kết quả
            }

            // Xử lý các comment con, cháu và sâu hơn
            while (stack.Any())
            {
                var currentComment = stack.Pop();
                var childComments = allComments.Where(c => c.ParentId == currentComment.Id).ToList();

                foreach (var child in childComments)
                {
                    stack.Push(child);    // Thêm con vào stack để duyệt tiếp
                    result.Add(child);   // Thêm con vào danh sách kết quả
                }
            }

            return result;
        }



        #endregion
    }
}
