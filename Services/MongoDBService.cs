using MongoExample.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Bson;

namespace MongoExample.Services;

public class MongoDBService
{

    private readonly IMongoCollection<Playlist> _playlistCollection;
    private readonly IMongoCollection<BsonDocument> playlistCollection;

    public MongoDBService(IOptions<MongoDBSettings> mongoDBSettings)
    {
        MongoClient client = new MongoClient(mongoDBSettings.Value.ConnectionURI);
        IMongoDatabase database = client.GetDatabase(mongoDBSettings.Value.DatabaseName);
        _playlistCollection = database.GetCollection<Playlist>(mongoDBSettings.Value.CollectionName);
        playlistCollection = database.GetCollection<BsonDocument>(mongoDBSettings.Value.CollectionName);
    }

    public async Task<List<Playlist>> GetAsync()
    {
        return await _playlistCollection.Find(new BsonDocument()).ToListAsync();
    }

    public async Task<Playlist> FindOne(string id)
    {
        FilterDefinition<Playlist> filter = Builders<Playlist>.Filter.Eq("Id", id);
        return await _playlistCollection.Find(filter).Limit(1).SingleAsync();
    }

    public async Task CreateAsync(Playlist playlist)
    {
        await _playlistCollection.InsertOneAsync(playlist);
        return;
    }
    public async Task AddToPlaylistAsync(string id, string movieId)
    {
        FilterDefinition<Playlist> filter = Builders<Playlist>.Filter.Eq("Id", id);
        UpdateDefinition<Playlist> update = Builders<Playlist>.Update.AddToSet<string>("items", movieId);
        await _playlistCollection.UpdateOneAsync(filter, update);
        return;
    }
    public async Task DeleteAsync(string id)
    {
        FilterDefinition<Playlist> filter = Builders<Playlist>.Filter.Eq("Id", id);
        await _playlistCollection.DeleteOneAsync(filter);
    }

    public async Task UpdateAsync(string id, Playlist playlist)
    {
        FilterDefinition<Playlist> filter = Builders<Playlist>.Filter.Eq("Id", id);
        var updName = Builders<Playlist>.Update.Set(p => p.username, playlist.username);
        // var updmovie = Builders<Playlist>.Update.Set<List<string>>("items", playlist.items);
        // var combine = Builders<Playlist>.Update.Combine(updName, updmovie);
        await _playlistCollection.UpdateOneAsync(filter, updName);
        return;
    }

    public async Task<List<BsonDocument>> GetLists()
    {
        BsonDocument pipelineStage1 = new BsonDocument{
    {
        "$match", new BsonDocument{
            { "username", "Rio johny" }
        }
    }
};

        BsonDocument pipelineStage2 = new BsonDocument{
    {
        "$project", new BsonDocument{
            { "_id", 1 },
            { "username", 1 },
            {
                "items", new BsonDocument{
                    {
                        "$map", new BsonDocument{
                            { "input", "$items" },
                            { "as", "item" },
                            {
                                "in", new BsonDocument{
                                    {
                                        "$convert", new BsonDocument{
                                            { "input", "$$item" },
                                            { "to", "objectId" }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
};


        BsonDocument pipelineStage3 = new BsonDocument{
    {
        "$lookup", new BsonDocument{
            { "from", "movies" },
            { "localField", "items" },
            { "foreignField", "_id" },
            { "as", "movies" }
        }
    }
};

        BsonDocument pipelineStage4 = new BsonDocument{
    { "$unwind", "$movies" }
};

        BsonDocument pipelineStage5 = new BsonDocument{
    {
        "$group", new BsonDocument{
            { "_id", "$_id" },
            {
                "username", new BsonDocument{
                    { "$first", "$username" }
                }
            },
            {
                "movies", new BsonDocument{
                    { "$addToSet", "$movies" }
                }
            }
        }
    }
};

        BsonDocument[] pipeline = new BsonDocument[] { 
    pipelineStage1, 
    pipelineStage2,
    pipelineStage3, 
    pipelineStage4, 
    pipelineStage5 
};

        List<BsonDocument> pResults = playlistCollection.Aggregate<BsonDocument>(pipeline).ToList();
        Console.WriteLine(pResults[0]);
        foreach (BsonDocument pResult in pResults)
        {
            Console.WriteLine(pResult);
        }
        return pResults ;
    }

    public async Task<List<BsonDocument>> GetAllPlaylist()
    {
        //Empty Pipeline
        var playlistAggregationPipeline = playlistCollection.Aggregate();
    
        //Lookup
        var playlistsJoinedWithCoursesPipeline = playlistAggregationPipeline
           .Lookup("movies", "items", "_id", "ListMovies")
           .ToList();
        //Project
        // var projection = Builders<Playlist>.Projection
        //     .Exclude("movies");
        // var studentsWithoutCourseIdsPipeline = playlistsJoinedWithCoursesPipeline.Project<Playlist>(projection);
        // var students = await studentsWithoutCourseIdsPipeline.ToListAsync();
        return playlistsJoinedWithCoursesPipeline;
    }
}