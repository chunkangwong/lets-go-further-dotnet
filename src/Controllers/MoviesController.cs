using Asp.Versioning;
using controller_api_test.src.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace controller_api_test.src.Controllers;

[ApiVersion("1.0")]
[ApiController]
[Route("v{version:apiVersion}/[controller]")]
public class MoviesController(AppDbContext dbContext) : ControllerBase
{
    private readonly AppDbContext _dbContext = dbContext;

    [HttpGet(Name = "GetMovies")]
    [Authorize(Policy = "MoviesRead")]
    public async Task<ActionResult<IEnumerable<Movie>>> Get([FromQuery] MovieQuery query)
    {
        var q = _dbContext.Movies.AsQueryable();

        if (!string.IsNullOrEmpty(query.Title))
        {
            q = q.Where(m =>
                EF.Functions.ToTsVector("simple", m.Title)
                .Matches(EF.Functions.PlainToTsQuery("simple", query.Title))
            );
        }

        if (query.Genres.Count > 0)
        {
            q = q.Where(m => m.Genres.Any(g => query.Genres.Contains(g)));
        }

        q = query.Sort switch
        {
            "title" => q.OrderBy(m => m.Title),
            "-title" => q.OrderByDescending(m => m.Title),
            "year" => q.OrderBy(m => m.Year),
            "-year" => q.OrderByDescending(m => m.Year),
            "runtime" => q.OrderBy(m => m.RunTime),
            "-runtime" => q.OrderByDescending(m => m.RunTime),
            "created_at" => q.OrderBy(m => m.CreatedAt),
            "-created_at" => q.OrderByDescending(m => m.CreatedAt),
            _ => q.OrderBy(m => m.Id)
        };

        // --- PAGINATION --
        var skip = (query.Page - 1) * query.PageSize;

        var movies = await q
            .Skip(skip)
            .Take(query.PageSize)
            .ToListAsync();

        return Ok(movies);
    }

    [HttpGet("{id}", Name = "GetMovie")]
    [Authorize(Policy = "MoviesRead")]
    public ActionResult<Movie> Get(int id)
    {
        var movie = _dbContext.Movies.Find(id);
        if (movie == null)
        {
            return NotFound();
        }
        return movie;
    }

    [HttpPost(Name = "CreateMovie")]
    [Authorize(Policy = "MoviesWrite")]
    public ActionResult<Movie> Post(Movie movie)
    {
        _dbContext.Movies.Add(movie);
        _dbContext.SaveChanges();
        return CreatedAtAction(nameof(Get), new { id = movie.Id }, movie);
    }

    [HttpPut("{id}", Name = "UpdateMovie")]
    [Authorize(Policy = "MoviesWrite")]
    public ActionResult<Movie> Put(int id, UpdateMovieDto movie)
    {
        var existingMovie = _dbContext.Movies.Find(id);
        if (existingMovie == null)
        {
            return NotFound();
        }
        _dbContext.Entry(existingMovie).CurrentValues.SetValues(movie);
        _dbContext.SaveChanges();
        return Ok(existingMovie);
    }

    [HttpDelete("{id}", Name = "DeleteMovie")]
    [Authorize(Policy = "MoviesWrite")]
    public ActionResult Delete(int id)
    {
        var movie = _dbContext.Movies.Find(id);
        if (movie == null)
        {
            return NotFound();
        }
        _dbContext.Movies.Remove(movie);
        _dbContext.SaveChanges();
        return NoContent();
    }
}
