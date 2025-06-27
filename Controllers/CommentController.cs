using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyMvcApp.Models;
using MyMvcApp.Data;
using Microsoft.ML;
using System.IO;

namespace MyMvcApp.Controllers
{
    public class CommentController : Controller
    {
        private static readonly string MODEL_PATH = "/Users/ivelindilqnovmihaylov/Desktop/UKTC/ИТ - кариера/C#_DEMOS/2025_modul07/MODEL/model.zip";
        private static readonly MLContext mlContext = new MLContext();
        private static ITransformer? mlModel;
        private static PredictionEngine<CommentInput, CommentPrediction>? predEngine;

        private readonly AppDbContext _context;

        public CommentController(AppDbContext context)
        {
            _context = context;
        }

        // Показване на всички коментари и форма за добавяне
        public async Task<IActionResult> Forum()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            IQueryable<Comment> commentsQuery = _context.Comments.Include(c => c.User);

            if (userRole != "Moderator" && userRole != "Admin")
            {
                commentsQuery = commentsQuery.Where(c => c.Status == CommentStatus.Approved);
            }

            var comments = await commentsQuery.OrderByDescending(c => c.CreatedAt).ToListAsync();
            return View(comments);
        }

        // ДОБАВЯНЕ НА КОМЕНТАР
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOnIndex(Comment comment)
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
            if (user == null)
            {
                TempData["Error"] = "Трябва да сте логнат, за да добавите коментар.";
                return RedirectToAction("Forum");
            }

            comment.UserId = user.Id;
            comment.CreatedAt = DateTime.Now;

            bool isFlagged = await AnalyzeWithMLModel(comment.Content);

            comment.Status = isFlagged ? CommentStatus.Flagged : CommentStatus.Approved;

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            if (isFlagged)
                TempData["message"] = "Коментарът ви ще бъде прегледан от модератор.";
            else
                TempData["message"] = "Коментарът е публикуван успешно.";

            return RedirectToAction("Forum");
        }

        private async Task<bool> AnalyzeWithMLModel(string content)
        {
            if (mlModel == null)
            {
                using var stream = new FileStream(MODEL_PATH, FileMode.Open, FileAccess.Read, FileShare.Read);
                mlModel = mlContext.Model.Load(stream, out _);
                predEngine = mlContext.Model.CreatePredictionEngine<CommentInput, CommentPrediction>(mlModel);
            }
            var input = new CommentInput { Text = content };
            var prediction = predEngine!.Predict(input);

            return prediction.PredictedLabel;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var comment = await _context.Comments.FindAsync(id);
            if (comment != null)
            {
                _context.Comments.Remove(comment);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Forum");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var comment = await _context.Comments.FindAsync(id);
            if (comment != null)
            {
                comment.Status = CommentStatus.Approved;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Forum");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var comment = await _context.Comments.FindAsync(id);
            if (comment == null)
                return NotFound();
            return View(comment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Comment comment)
        {
            if (id != comment.Id)
                return BadRequest();

            _context.Update(comment);
            await _context.SaveChangesAsync();
            return RedirectToAction("Forum");
            
            return View(comment);
        }
    }
}