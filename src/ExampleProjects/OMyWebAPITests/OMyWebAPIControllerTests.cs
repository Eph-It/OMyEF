using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using OMyEFDbContext;
using System;
using System.Net;
using System.Net.Http;
using Xunit;
using Simple.OData.Client;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace OMyWebAPITests
{
    public class OMyWebAPIControllerTests : IClassFixture<WebApplicationFactory<OMyWebAPI.Startup>>
    {
        readonly ODataClient _client;
        public OMyWebAPIControllerTests(WebApplicationFactory<OMyWebAPI.Startup> fixture)
        {
            var client = fixture.CreateClient();
            client.BaseAddress = new Uri("http://localhost/odata");
            var oClientSettings = new ODataClientSettings(client);
            _client = new ODataClient(oClientSettings);
        }
        [Fact]
        public async Task ShouldBasicQueryAsync()
        {
            var results = await _client
                .For<TableOne>()
                .Filter(x => x.Id == 1)
                .FindEntriesAsync();
            results.Count().Should().Be(1);

            results.First().Name.Should().Be("testName");
            results = await _client
                .For<TableOne>()
                .Filter(x => x.Id == 1)
                .Select(p => p.Id)
                .FindEntriesAsync();
            results.First().Name.Should().BeNull();

            var findByKey = await _client.For<TableOne>().Key(1).FindEntryAsync();
            findByKey.Name.Should().Be("testName");
        }
        [Fact]
        public async Task ShouldInsertResults()
        {
            var countResults = (await _client.FindEntriesAsync("TableOne")).Count();
            var results = new List<Task<TableOne>>();
            for (int i = 0; i < 100; i++)
            {
                results.Add(
                    _client
                        .For<TableOne>()
                        .Set(new { Name = $"Test{i}" })
                        .InsertEntryAsync()
                     );
            }
            Task.WaitAll(results.ToArray());
            var entries = await _client.FindEntriesAsync("TableOne");
            entries.Count().Should().Be(countResults + 100);
        }
        [Fact]
        public async Task ShouldDeleteResults()
        {
           var entry = await _client
                    .For<TableOne>()
                    .Set(new { Name = "EntryToDelete" })
                    .InsertEntryAsync();

            await _client.For<TableOne>().Key(entry.Id).DeleteEntryAsync();
            TableOne result = null;
            try
            {
                result = await _client.For<TableOne>().Key(entry.Id).FindEntryAsync();
            }
            catch { }
            result.Should().BeNull();
        }
        [Fact]
        public async Task ShouldEditData()
        {
            var entry = await _client
                 .For<TableOne>()
                 .Set(new { Name = "EntryToEdit" })
                 .InsertEntryAsync();
            await _client
                .For<TableOne>()
                .Key(entry.Id)
                .Set(new { Name = "EntryEdited" })
                .UpdateEntryAsync();
            var getEntry = await _client.For<TableOne>().Key(entry.Id).FindEntryAsync();
            getEntry.Name.Should().Be("EntryEdited");
        }
        [Fact]
        public async Task ShouldAddDateCreated()
        {
            var badDate = DateTime.UtcNow.AddDays(-100);
            var goodDate = DateTime.UtcNow.AddDays(-1);
            var entry = await _client
                 .For<TableThree>()
                 .Set(new { Name = "EntryToEdit", Created = badDate })
                 .InsertEntryAsync();
            var newEntry = await _client
                .For<TableThree>()
                .Key(entry.Id)
                .FindEntryAsync();
            newEntry.Created.Should().BeAfter(goodDate);
        }
    }
}
