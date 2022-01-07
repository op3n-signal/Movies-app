using L08HandsOn.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace L08HandsOn.Controllers
{
    [Authorize(Policy = "Manager")]
    public class UsersController : Controller
    {
        private readonly UsersContext _user;
        private readonly MoviesContext _movie;

        public UsersController(UsersContext user, MoviesContext movie)
        {
            _user = user;
            _movie = movie;

        }

        public async Task<IActionResult> Index(string? username)
        {
            setTimesWatched();
            if (username != null)
            {
                var allEntries = _user.Users.Select(user => user).Where(u => u.UserName == username).ToList();

                
                if (allEntries.Count > 0)
                {
                    return View(allEntries);
                } 
                else
                {
                    return View("PageNotFound");
                }
            }

            return View(await _user.Users.ToListAsync());
        }

        public void setTimesWatched()
        {
            string email = User.Identity.Name;
            var userEntries = _user.Users.Select(u => u).Where(u => u.UserName == email);
            var userMovies = userEntries.Select(a => a.MovieId).ToList();
            var userTimes = userEntries.Select(a => a.TimesWatched).ToList();
            var some = _user.Users.Select(m => m).Where(movie => userMovies.Contains(movie.Id)).ToList();

            for (int i = 0; i < some.Count; i++)
            {
                for (int o = 0; o < userTimes.Count; o++)
                {
                    if (some[i].Id == userEntries.Select(e => e.MovieId).ToList()[o])
                    {
                        some[i].TimesWatched = userTimes[o];
                    }
                }
            }
        }

        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create([Bind("Id", "UserName", "MovieId", "TimesWatched")] User user)
        {
            if (ModelState.IsValid)
            {
                _user.Add(user);
                await _user.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _user.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(int id, [Bind("Id", "UserName", "MovieId", "TimesWatched")] User user)
        {
            if (id != user.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _user.Update(user);
                    await _user.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(user.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }

        private bool UserExists(int id)
        {
            return _user.Users.Any(e => e.Id == id);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if(id == null)
            {
                return NotFound();
            }


            var user = await _user.Users.FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _user.Users.FindAsync(id);
            _user.Users.Remove(user);
            await _user.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _user.Users.FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }
 
     
    }
}
