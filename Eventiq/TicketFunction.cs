using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
//using Microsoft.Azure.WebJobs;
//using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using Microsoft.Azure.Functions.Worker;

namespace AzureFunctionsEventiq
{
    public static class TicketFunction
    {
        private static readonly string connectionString = Environment.GetEnvironmentVariable("MongoDBAtlasConnectionString");
        private static readonly string dbName = "EventiqSuport";
        private static readonly string collectionName = "suporte";


        //http://localhost:7171/api/ticket/create?userId=307&adminId=551&title=fã%20do%20ticketit&messageContent=aplicativo%20lixo
        [Function("CreateTicket")]
        public static async Task<IActionResult> CreateTicket(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "ticket/create")] HttpRequest req, ILogger log)
        {
            try
            {
                string userId = req.Query["userId"];
                string adminId = req.Query["adminId"];
                string title = req.Query["title"];
                string messageContent = req.Query["messageContent"];

                var client = new MongoClient(connectionString);
                var database = client.GetDatabase(dbName);
                var collection = database.GetCollection<BsonDocument>(collectionName);

                var ticket = new BsonDocument
                {
                    { "userId", userId },
                    { "adminId", adminId },
                    { "title", title },
                    { "createdAt", DateTime.UtcNow },
                    { "message", new BsonArray
                {
                    new BsonDocument
                    {
                        { "_id", ObjectId.GenerateNewId() },
                        { "createdAt", DateTime.UtcNow },
                        { "senderId", userId },
                        { "content", messageContent }
                    }
                }
            }
        };

                await collection.InsertOneAsync(ticket);

                return new OkObjectResult($"Ticket created with ID: {ticket["_id"]}");
            }
            catch (Exception e)
            {
                log.LogError($"Error creating ticket: {e.Message}");
                return new BadRequestObjectResult("Error creating ticket");
            }
        }


        //http://localhost:7171/api/ticket/addMessage?ticketId=66e0e1d4e0a76f07182403a2&senderId=551&messageContent=NãoSouPagoParaIsso
        [Function("AddMessageToTicket")]
        public static async Task<IActionResult> AddMessageToTicket(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "ticket/addMessage")] HttpRequest req, ILogger log)
        {
            try
            {
                string ticketId = req.Query["ticketId"];
                string senderId = req.Query["senderId"];
                string messageContent = req.Query["messageContent"];

                var client = new MongoClient(connectionString);
                var database = client.GetDatabase(dbName);
                var collection = database.GetCollection<BsonDocument>(collectionName);

                var objectId = new ObjectId(ticketId);
                var ticket = await collection.Find(new BsonDocument { { "_id", objectId } }).FirstOrDefaultAsync();

                if (ticket == null)
                {
                    return new NotFoundObjectResult("Ticket not found");
                }

                var newMessage = new BsonDocument
                {
                    { "_id", ObjectId.GenerateNewId() },
                    { "createdAt", DateTime.UtcNow },
                    { "senderId", senderId },
                    { "content", messageContent }
                };

                var update = Builders<BsonDocument>.Update.Push("message", newMessage);
                await collection.UpdateOneAsync(new BsonDocument { { "_id", objectId } }, update);

                return new OkObjectResult("Message added to ticket");
            }
            catch (Exception e)
            {
                log.LogError($"Error adding message to ticket: {e.Message}");
                return new BadRequestObjectResult("Error adding message to ticket");
            }
        }


        //http://localhost:7171/api/ticket/get?ticketId=66e0e3abe262603264194977
        [Function("GetTicket")]
        public static async Task<IActionResult> GetTicket(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "ticket/get")] HttpRequest req, ILogger log)
        {
            try
            {
                string ticketId = req.Query["ticketId"];

                var client = new MongoClient(connectionString);
                var database = client.GetDatabase(dbName);
                var collection = database.GetCollection<BsonDocument>(collectionName);

                var objectId = new ObjectId(ticketId);
                var ticket = await collection.Find(new BsonDocument { { "_id", objectId } }).FirstOrDefaultAsync();

                if (ticket == null)
                {
                    return new NotFoundObjectResult("Ticket not found");
                }

                var messages = ticket["message"].AsBsonArray.Select(m => new
                {
                    Usuario = m["senderId"].AsString,
                    Mensagem = m.ToJson()
                }).ToList();

                return new OkObjectResult(messages);
            }
            catch (Exception e)
            {
                log.LogError($"Error retrieving ticket: {e.Message}");
                return new BadRequestObjectResult("Error retrieving ticket");
            }
        }


        //http://localhost:7171/api/ticket/update?ticketId=66e234c3271ab928ca59fbfd&title=Removido%20por%20ser%20hater&adminId=507
        [Function("UpdateTicket")]
        public static async Task<IActionResult> UpdateTicket(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "ticket/update")] HttpRequest req, ILogger log)
        {
            try
            {
                string ticketId = req.Query["ticketId"];
                string title = req.Query["title"];
                string adminId = req.Query["adminId"];

                var client = new MongoClient(connectionString);
                var database = client.GetDatabase(dbName);
                var collection = database.GetCollection<BsonDocument>(collectionName);

                var objectId = new ObjectId(ticketId);
                var update = Builders<BsonDocument>.Update
                    .Set("title", title)
                    .Set("adminId", adminId);

                var result = await collection.UpdateOneAsync(new BsonDocument { { "_id", objectId } }, update);

                if (result.MatchedCount == 0)
                {
                    return new NotFoundObjectResult("Ticket not found");
                }

                return new OkObjectResult("Ticket updated successfully");
            }
            catch (Exception e)
            {
                log.LogError($"Error updating ticket: {e.Message}");
                return new BadRequestObjectResult("Error updating ticket");
            }
        }

        //http://localhost:7171/api/ticket/delete?ticketId=66e0e3c4b7187a177e0e20c9
        [Function("DeleteTicket")]
        public static async Task<IActionResult> DeleteTicket(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "ticket/delete")] HttpRequest req, ILogger log)
        {
            try
            {
                string ticketId = req.Query["ticketId"];

                var client = new MongoClient(connectionString);
                var database = client.GetDatabase(dbName);
                var collection = database.GetCollection<BsonDocument>(collectionName);

                var objectId = new ObjectId(ticketId);
                var result = await collection.DeleteOneAsync(new BsonDocument { { "_id", objectId } });

                if (result.DeletedCount == 0)
                {
                    return new NotFoundObjectResult("Ticket not found");
                }

                return new OkObjectResult("Ticket deleted successfully");
            }
            catch (Exception e)
            {
                log.LogError($"Error deleting ticket: {e.Message}");
                return new BadRequestObjectResult("Error deleting ticket");
            }
        }
    }
}
