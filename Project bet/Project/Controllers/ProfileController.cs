using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project.Data;
using Project.Models;
using System.Linq;
using System.Threading.Tasks;

namespace Project.Controllers
{
    public class ProfileController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProfileController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Profile
        public async Task<IActionResult> Index()
        {
            var profile = await _context.Profiles.FirstOrDefaultAsync();
            return View(profile);
        }

        // GET: /Profile/Edit/1
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var profile = await _context.Profiles.FindAsync(id);
            if (profile == null)
                return NotFound();

            return View(profile);
        }

        // POST: /Profile/Edit/1
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Profile model)
        {
            if (id != model.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(model);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        // Optional: seed a profile if none exists
        public async Task<IActionResult> Seed()
        {
            if (!_context.Profiles.Any())
            {
                _context.Profiles.Add(new Profile
                {
                    Name = "John",
                    Surname = "Doe",
                    Username = "johndoe",
                    Email = "john.doe@example.com",
                    PhoneNumber = "(123) 456-7890",
                    Location = "Ibhayi, Eastern Cape"
                });

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
