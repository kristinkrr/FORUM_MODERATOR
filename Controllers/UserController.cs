using Microsoft.AspNetCore.Mvc;
using MyMvcApp.Data;        
using MyMvcApp.Models;      
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;

namespace MyMvcApp.Controllers
{
    public class UserController : Controller
    {
        private readonly AppDbContext _context;

        public UserController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(User user)
        {
            if (!ModelState.IsValid)
            {
                return View(user);
            }

            try
            {
                user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                TempData["message"] = "Регистрацията беше успешна! Моля, влезте в профила си.";

                return RedirectToAction("Login", "User");
            }
            catch (Exception ex)
            {
    
                Console.WriteLine($"Грешка при регистрация: {ex.Message}");

                ModelState.AddModelError(string.Empty, "Възникна грешка при регистрацията. Моля, опитайте отново.");
                return View(user);
            }
        }

         // 👉 GET: /User/Register
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.Password))
            {
                ModelState.AddModelError(string.Empty, "Грешен имейл или парола. Моля, опитайте отново.");
                return View();
            }

            HttpContext.Session.SetString("UserName", $"{user.FirstName} {user.LastName}");
            HttpContext.Session.SetString("UserRole", user.Role);
            HttpContext.Session.SetString("UserEmail", user.Email); // ТОВА Е ВАЖНО!

            TempData["message"] = "Логнахте се успешно";
            return RedirectToAction("Index", "Home");
        }

        // 👉 GET: /User/Logout
        public IActionResult Logout()
        {
            // Изчистване на сесията
            HttpContext.Session.Clear();

            // Пренасочване към страницата за логване
            TempData["message"] = "Излезнахте успешно";
            return RedirectToAction("Login", "User");
        }

        // 👉 Списък с потребители
        public async Task<IActionResult> Users()
        {
             // Проверка дали потребителят е логнат
            if (HttpContext.Session.GetString("UserName") == null)
            {
                return RedirectToAction("Login", "User");
            }

            var users = await _context.Users.ToListAsync();

            // Проверка дали има данни
            if (users == null || !users.Any())
            {
                return Content("Няма налични потребители в базата данни.");
            }

            return View(users);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return RedirectToAction("Users","User");
        }

        // 👉 GET: /User/Edit/{id}
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, User updatedUser)
        {
            if (id != updatedUser.Id)
            {
                return BadRequest();
            }

            // Проверка дали имейлът вече е записан за друг потребител
            if (await _context.Users.AnyAsync(u => u.Email == updatedUser.Email && u.Id != updatedUser.Id))
            {
                ModelState.AddModelError("Email", "Имейлът вече е регистриран за друг потребител.");
                return View(updatedUser);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    updatedUser.Password = BCrypt.Net.BCrypt.HashPassword(updatedUser.Password);
                    _context.Update(updatedUser);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Users");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Users.Any(u => u.Id == id))
                    {
                        return NotFound();
                    }
                    throw;
                }
            }

            return View(updatedUser);
        }

        public async Task<IActionResult> Profil()
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
            if (user == null)
            {
                return RedirectToAction("Login", "User");
            }

            return View(user); 
        }
    }
}
