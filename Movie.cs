using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using NpgsqlTypes;

namespace controller_api_test;

public class Movie
{

    [Column("id")]
    public int Id { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("title")]
    [MaxLength(500)]
    [Required]
    public required string Title { get; set; }

    [Column("year")]
    [Range(1888, int.MaxValue)]
    [Required]
    public int Year { get; set; }

    [Column("runtime")]
    [Range(1, int.MaxValue)]
    [Required]
    public int RunTime { get; set; }

    [Column("genres")]
    [MinLength(1)]
    [MaxLength(5)]
    [Required]
    public List<string> Genres { get; set; } = [];


    [Column("version")]
    public int Version { get; set; }
}

public class UpdateMovieDto
{
    public string Title { get; set; } = string.Empty;
    public int Year { get; set; }
    public int RunTime { get; set; }
    public List<string> Genres { get; set; } = [];
}

// v.Check(f.Page > 0, "page", "must be greater than zero")
// 	v.Check(f.Page <= 10_000_000, "page", "must be a maximum of 10,000,000")
// 	v.Check(f.PageSize > 0, "page_size", "must be greater than zero")
// 	v.Check(f.PageSize <= 100, "page_size", "must be a maximum of 100")
// 	v.Check(validator.In(f.Sort, f.SortSafelist...), "sort", "invalid sort value")
public class MovieQuery
{
    public string Title { get; set; } = string.Empty;
    public List<string> Genres { get; set; } = [];
    [Range(1, 10_000_000)]
    public int Page { get; set; } = 1;
    [Range(1, 100)]
    public int PageSize { get; set; } = 20;
    public string Sort { get; set; } = "id";
}

public static class MovieSort
{
    public static readonly HashSet<string> Safelist = [
        "id", "title", "year", "runtime",
        "-id", "-title", "-year", "-runtime"
    ];
}