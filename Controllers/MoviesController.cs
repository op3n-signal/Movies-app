using L08HandsOn.Data;
using L08HandsOn.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace L08HandsOn.Controllers
{
    public class MoviesController : Controller
    {
        private readonly MoviesContext _context;
        private readonly UsersContext _user;
        public MoviesController(MoviesContext context, UsersContext user)
        {
            _context = context;
            _user = user;
        }

        //get movies

        public async Task<IActionResult> Index()
        {
            setTimesWatched();

            return View(await _context.Movies.ToListAsync());
        }
        // method to set the TimesWatched to the lists
        public void setTimesWatched()
        {
            string email = User.Identity.Name;
            var userEntries = _user.Users.Select(u => u).Where(u => u.UserName == email);
            var userMovies = userEntries.Select(a => a.MovieId).ToList();
            var userTimes = userEntries.Select(a => a.TimesWatched).ToList();
            var some = _context.Movies.Select(m => m).Where(movie => userMovies.Contains(movie.Id)).ToList();

            for (int i = 0; i < some.Count; i++)
            {
                for(int o = 0; o < userTimes.Count; o++)
                {
                    if(some[i].Id == userEntries.Select(e => e.MovieId).ToList()[o])
                    {
                        some[i].TimesWatched = userTimes[o];
                    }
                }
            }
        }
        //for the create view
        [Authorize(Policy = "Manager")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create([Bind("Id", "Name", "Message", "TimesWatched")] Movie movie)
        {
            if (ModelState.IsValid)
            {
                _context.Add(movie);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(movie);
        }

        //for the edit view
        [Authorize(Policy = "Manager")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movies.FindAsync(id);
            if (movie == null)
            {
                return NotFound();
            }
            return View(movie);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, [Bind("Id", "Name", "Message", "TimesWatched")] Movie movie)
        {
            if(id != movie.Id)
            {
                return NotFound();
            }

            if(ModelState.IsValid)
            {
                try
                {
                    _context.Update(movie);
                    await _context.SaveChangesAsync();
                }
                catch(DbUpdateConcurrencyException)
                {
                    if(!MovieExists(movie.Id))
                    {
                        return NotFound();
                    } else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(movie);
        }
        //to check if a movie exists
        private bool MovieExists(int id)
        {
            return _context.Movies.Any(e => e.Id == id);
        }

        //for the delete view
        [Authorize(Policy = "Manager")]
        public async Task<IActionResult> Delete(int? id)
        {
            if(id == null)
            {
                return NotFound();
            }


            var movie = await _context.Movies.FirstOrDefaultAsync(m => m.Id == id);
            if(movie == null)
            {
                return NotFound();
            }
            return View(movie);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var movie = await _context.Movies.FindAsync(id);
            _context.Movies.Remove(movie);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
 
        [Authorize(Policy = "Manager")]
        // for the details view
        public async Task<IActionResult> Details(int? id)
        {
            if(id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movies.FirstOrDefaultAsync(m => m.Id == id);
            if(movie == null)
            {
                return NotFound();
            }
            return View(movie);
        }
        [Authorize]
        //for the watch view
        public async Task<IActionResult> Watch(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
          
            var movie = await _context.Movies.FindAsync(id);
            if (movie == null)
            {
                return NotFound();
            }
            //getting the current email signed in
            string email = User.Identity.Name;
            //opening a connecting to the User DB below
            //if the user has not watched a movie it will be added to the DB
            //if the user has watched a movie the TimesWatched column for that entry will be incremented
            var connectionString = "Data Source=" + Environment.CurrentDirectory + "\\Users.db";
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                var insertCmd = connection.CreateCommand();
                if (_user.Users.Any(a => a.UserName == email && a.MovieId == id))
                {
                    insertCmd.CommandText = "UPDATE Users SET TimesWatched = TimesWatched + 1 WHERE UserName == @email AND MovieId == @id";
                    insertCmd.Parameters.AddWithValue("email", email);
                    insertCmd.Parameters.AddWithValue("id", id);
                    insertCmd.ExecuteNonQuery();
                }
                else
                {
                    insertCmd.CommandText = "INSERT INTO Users(UserName, MovieId, TimesWatched) VALUES (@email, @id, @times)";
                    insertCmd.Parameters.AddWithValue("email", email);
                    insertCmd.Parameters.AddWithValue("id", id);
                    insertCmd.Parameters.AddWithValue("times", 1);
                    insertCmd.ExecuteNonQuery();
                }
                
                connection.Close();
            }
            return View(movie);
        }

        //to display all movies the user has watched
        public async Task<IActionResult> Watched()
        {
            //I use the current users email
            string email = User.Identity.Name;
            //access Users db here to get all movies the user has watched by getting all of the MovieId's registered in his email
            var userEntries = _user.Users.Select(u => u).Where(u => u.UserName == email).Select(a => a.MovieId).ToList();
            //Look into Movies DB and pull out those matching the IDs
            var allMovies = _context.Movies.Select(m => m).Where(movie => userEntries.Contains(movie.Id));

            setTimesWatched();

            return View(await allMovies.ToListAsync());
        }

    }
}
