using Neo4j.Driver;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
/// <summary>
/// // This class manages connections and operations with Neo4j graph database
// It's used to create a graph representation of predicates and their relationships
/// </summary>
public class Neo4jService : IDisposable
{
    private readonly IDriver _driver;
    private bool _disposed = false;
/// <summary>
/// Constructor for Neo4jService
/// </summary>
/// <param name="uri"> The URI of the Neo4j database   </param>
/// <param name="user"> The username for the Neo4j database </param>
/// <param name="password"> The password for the Neo4j database </param>
    public Neo4jService(string uri, string user, string password)
    {

        _driver = GraphDatabase.Driver(uri, AuthTokens.Basic(user, password));
    }
    /// <summary>
    /// Adds a relationship between two nodes in the graph
    /// </summary>
    /// <param name="predicateType"> The type of predicate to create </param>
    /// <param name="parameters"> A dictionary of parameters for the predicate </param>
    /// <returns> A task that represents the asynchronous operation </returns>

    public async Task AddPredicateRelationship(string predicateType, Dictionary<string, IEntity> parameters)
    {
        // Create an asynchronous session to interact with the Neo4j database
        var session = _driver.AsyncSession();
        try
        {
            // Execute the operation in a write transaction
            await session.ExecuteWriteAsync(async tx =>
            {
                // Create nodes for each parameter if they don't exist
                foreach (var param in parameters)
                {
                    await tx.RunAsync(@"
                        MERGE (thing:Thing {id: $id, type: $type, name: $name})
                        ",
                        new 
                        {
                            id = param.Value.ID,
                            type = param.Value.GetType().Name,
                            name = param.Value.NameKey.ToString()
                        });
                }

                // Create the relationship based on predicate type
                var query = BuildPredicateQuery(predicateType, parameters);
                await tx.RunAsync(query.query, query.parameters);
            });
        }
        finally
        {
            await session.CloseAsync();
        }
    }

    private (string query, object parameters) BuildPredicateQuery(string predicateType, Dictionary<string, IEntity> parameters)
    {
        // Example for a binary predicate like "holding(agent, element)"
        if (parameters.Count == 2)
        {
            var param1 = parameters.First();
            var param2 = parameters.Last();
            
            return (@"
                MATCH (a:Thing {id: $id1})
                MATCH (b:Thing {id: $id2})
                CREATE (a)-[r:" + predicateType + @"]->(b)
                RETURN type(r)
                ",
                new { id1 = param1.Value.ID, id2 = param2.Value.ID }
            );
        }

        // Handle other predicate arities as needed
        throw new NotImplementedException($"Predicate with {parameters.Count} parameters not supported yet");
    }

    public async Task<bool> TestConnection()
    {
        try
        {
            // Try to open a session and run a simple query
            using var session = _driver.AsyncSession();
            var result = await session.ExecuteReadAsync(async tx =>
            {
                var cursor = await tx.RunAsync("RETURN 1 as n");
                var record = await cursor.SingleAsync();
                return record["n"].As<int>();
            });
            
            return result == 1;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Neo4j connection test failed: {ex.Message}");
            return false;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _driver?.Dispose();
            }
            _disposed = true;
        }
    }
} 