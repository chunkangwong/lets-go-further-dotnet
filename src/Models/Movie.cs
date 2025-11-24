using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace controller_api_test.src.Models;

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