using AdvertApi.Models;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;

namespace AdvertApi.Services
{
    public class DynamoDbAdvertStorage : IAdvertStorageService
    {
        private readonly IMapper _mapper;
        public DynamoDbAdvertStorage(IMapper mapper)
        {
            _mapper = mapper;
        }

        public async Task<string> Add(AdvertModel model)
        {
            //Add validations in production code

            //Map the DB model from the API model
            var dbModel = _mapper.Map<AdvertDbModel>(model);

            using (var client = new AmazonDynamoDBClient())
            {
                // Populate properties not mapped from API model
                dbModel.Id = Guid.NewGuid().ToString();
                dbModel.CreationDateTime = DateTime.UtcNow;
                //Set status to pending. Only after the S3 storage is done is this changed as this is a
                //distributed transaction
                dbModel.Status = AdvertStatus.Pending;

                using (var context = new DynamoDBContext(client))
                {
                    await context.SaveAsync(dbModel);

                }
            }

            return dbModel.Id;

            //Return the right error code when save is not done
        }

        // Check Dynamo DB health check
        public async Task<bool> CheckHealthAsync()
        {
            using (var client = new AmazonDynamoDBClient())
            {
                // Check the availability of dependent table
                var tableData = await client.DescribeTableAsync("Adverts");
                return string.Compare(tableData.Table.TableStatus, "active", true) == 0;

            }
        }

        public async Task ConfirmAsync(ConfirmAdvertModel model)
        {
            using (var client = new AmazonDynamoDBClient())
            {
                using (var context = new DynamoDBContext(client))
                {
                    // Load the existing record using the Id
                    var record = await context.LoadAsync<AdvertDbModel>(model.Id);
                    if (record == null)
                    {
                        throw new KeyNotFoundException($"A record with ID={model.Id} was not found.");
                    }

                    if (model.Status == AdvertStatus.Active)
                    {
                        record.FilePath = model.FilePath;
                        //Set the status to active when record is found
                        record.Status = AdvertStatus.Active;
                        await context.SaveAsync(record);
                    }
                    else
                    {
                        await context.DeleteAsync(record);
                    }
                }
            }
        }

        public async Task<AdvertDbModel> FindByIdAsync(string id)
        {
            using (var client = new AmazonDynamoDBClient())
            {
                using (var context = new DynamoDBContext(client))
                {
                    // Load the existing record using the Id
                    var record = await context.LoadAsync<AdvertDbModel>(id);
                    if (record == null)
                    {
                        throw new KeyNotFoundException($"A record with ID={id} was not found.");
                    }

                    return record;
                }
            }
        }

        public async Task<List<AdvertModel>> GetAllAsync()
        {
            using (var client = new AmazonDynamoDBClient())
            {
                using (var context = new DynamoDBContext(client))
                {
                    //Scanning is not suitable for production database and use index in that case
                    var allItems = await context.ScanAsync<AdvertDbModel>(new List<ScanCondition>())
                        .GetRemainingAsync();
                    return allItems.Select(item => _mapper.Map<AdvertModel>(item)).ToList();
                }
            }
        }
    }
}
