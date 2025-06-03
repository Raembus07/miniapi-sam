using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

public class MongoMovieService : IMovieService
{
    private readonly IOptions<DatabaseSettings> _options;
    private readonly IMongoCollection<Movie> _collection;

    public MongoMovieService(IOptions<DatabaseSettings> options)
    {
        _options = options;
        var client = new MongoClient(_options.Value.ConnectionString);
        var database = client.GetDatabase("movies");
        _collection = database.GetCollection<Movie>("movies");
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
        if (string.IsNullOrEmpty(movie.Id) || _collection.Find(m => m.Id == movie.Id).Any())
        {
            return Results.Conflict("Movie with this ID already exists.");
        }
        _collection.InsertOne(movie);
        return Results.Ok(movie);
    }

    public IResult Get()
    {
        var movies = _collection.Find(_ => true).ToList();
        return Results.Ok(movies);
    }

    public IResult Get(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return Results.NotFound("Movie not found.");
        }
        var movie = _collection.Find(m => m.Id == id).FirstOrDefault();
        if (movie == null)
        {
            return Results.NotFound("Movie not found.");
        }
        return Results.Ok(movie);
    }

    public IResult Update(string id, Movie movie)
    {
        if (string.IsNullOrEmpty(id))
        {
            return Results.NotFound("Movie not found.");
        }
        var result = _collection.ReplaceOne(m => m.Id == id, movie);
        if (result.MatchedCount == 0)
        {
            return Results.NotFound("Movie not found.");
        }
        return Results.Ok(movie);
    }

    public IResult Remove(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return Results.NotFound("Movie not found.");
        }
        var result = _collection.DeleteOne(m => m.Id == id);
        if (result.DeletedCount == 0)
        {
            return Results.NotFound("Movie not found.");
        }
        return Results.Ok("Movie deleted successfully.");
    }
}