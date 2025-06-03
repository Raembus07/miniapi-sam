using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

public class MongoMovieService : IMovieService
{
    private readonly IOptions<DatabaseSettings> _options;

    List<Movie> movies = new List<Movie> { };

    public MongoMovieService(IOptions<DatabaseSettings> options)
    {
        _options = options;
    }

    public IResult Check()
    {
        var mongoDbConnectionString = _options.Value.ConnectionString;

        try
        {
            var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var cancellationToken = cancellationTokenSource.Token;

            var client = new MongoClient(mongoDbConnectionString);

            var databases = client.ListDatabaseNames(cancellationToken).ToList();

            return Results.Ok("Zugriff auf MongoDB ok. Datenbanken: " + string.Join(", ", databases));
        }
        catch (TimeoutException ex)
        {
            return Results.Problem("Fehler: Timeout beim Zugriff auf MongoDB: " + ex.Message);
        }
        catch (Exception ex)
        {
            return Results.Problem("Fehler beim Zugriff auf MongoDB: " + ex.Message);
        }
    }

    public IResult Create(Movie movie)
    {
        if (movies.Contains(movie))
        {
            return Results.Conflict("Movie already exists.");
        }
        else
        {
            movies.Add(movie);
            return Results.Ok(movie);
        }
    }

    public IResult Get()
    {
        return Results.Ok(movies);
    }
    public IResult Get(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return Results.NotFound("Movie not found.");
        }
        else if (!movies.Any(m => m.Id == id))
        {
            return Results.NotFound("Movie not found.");
        }
        else
        {
            var movie = movies.First(m => m.Id == id);
            return Results.Ok(movie);
        }
    }
    public IResult Update(string id, Movie movie)
    {
        if (string.IsNullOrEmpty(id) || !movies.Any(m => m.Id == id))
        {
            return Results.NotFound("Movie not found.");
        }

        var existingMovie = movies.First(m => m.Id == id);
        existingMovie.Title = movie.Title;
        existingMovie.Year = movie.Year;
        existingMovie.Summary = movie.Summary;
        existingMovie.Actors = movie.Actors;

        return Results.Ok(existingMovie);
    }
    public IResult Remove(string id)
    {
        if (string.IsNullOrEmpty(id) || !movies.Any(m => m.Id == id))
        {
            return Results.NotFound("Movie not found.");
        }

        var movieToDelete = movies.First(m => m.Id == id);
        movies.Remove(movieToDelete);

        return Results.Ok("Movie deleted successfully.");
    }
}