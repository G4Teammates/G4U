﻿using AutoMapper;
using CommentMicroservice.DBContexts;
using CommentMicroservice.DBContexts.Entities;
using CommentMicroservice.Models;
using CommentMicroservice.Models.DTO;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using X.PagedList.Extensions;

namespace CommentMicroservice.Repositories
{
    public class RepoComment : IRepoComment
    {
        private readonly CommentDbContext _db;
        private readonly IMapper _mapper;

        public RepoComment(CommentDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
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
                var Comm = await _db.Comments.Where(c => c.Id == id || c.ParentId == id).ToListAsync();
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

        public async Task<ResponseModel> CreateComment(CreateCommentDTO Comment)
        {
            ResponseModel response = new();
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
                var Comm = await _db.Comments.Where(c => c.ProductId == productId).ToListAsync();
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
            "nứng", "nứng nà", "nứng ơi", "dâm đãng", "d*m đãng", "d*m dục", "địt", "địtttt", "đjt", "dm", "dm bạn", "ch*ch",

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


        #endregion

        public async Task<ResponseModel> GetByproductId(string productId, int page, int pageSize)
        {
            ResponseModel response = new();
            try
            {
                var Comm = await _db.Comments.Where(c => c.ProductId == productId).ToListAsync();
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
    }
}
