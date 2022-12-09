using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.SqlServer.Test.Models;
using Xunit;

namespace Microsoft.EntityFrameworkCore.SqlServer
{
    public class QueryTests : IDisposable
    {
        private readonly DateAndTimeContext _db;

        private string SelectStatement => "SELECT [e].[Id], [e].[StartDate], [e].[StartTime] FROM [Events] AS [e]";

        public QueryTests()
        {
            _db = new DateAndTimeContext();
            _db.Database.EnsureDeleted();
            _db.Database.EnsureCreated();
        }

        [Fact]
        public void GetDateOnly_can_translate()
        {
            var results = Enumerable.ToList(
                from p in _db.Events
                where p.StartDate == new DateOnly(2022, 12, 13)
                select p.Id);

            Assert.Equal(
                condense(@"SELECT [e].[Id] FROM [Events] AS [e] WHERE [e].[StartDate] = '2022-12-13'"),
                condense(_db.Sql));

            Assert.Equal(new[] { 1 }, results);
        }

        [Fact]
        public void GetDateOnly_can_translate_Parameter()
        {
            var startDate = new DateOnly(2022, 12, 13);

            var results = Enumerable.ToList(
                from p in _db.Events
                where p.StartDate == startDate
                select p.Id);

            Assert.Equal(
                condense(@"SELECT [e].[Id] FROM [Events] AS [e] WHERE [e].[StartDate] = @__startDate_0"),
                condense(_db.Sql));

            Assert.Equal(new[] { 1 }, results);
        }

        [Fact]
        public void GetTimeOnly_can_translate()
        {
            var results = Enumerable.ToList(
                from p in _db.Events
                where p.StartTime == new TimeOnly(9, 9)
                select p.Id);

            Assert.Equal(
                condense(@"SELECT [e].[Id] FROM [Events] AS [e] WHERE [e].[StartTime] = '09:09:00'"),
                condense(_db.Sql));

            Assert.Equal(new[] { 1 }, results);
        }

        [Fact]
        public void GetTimeOnly_can_translate_Parameter()
        {
            var startTime = new TimeOnly(9, 9);

            var results = Enumerable.ToList(
                from p in _db.Events
                where p.StartTime == startTime
                select p.Id);

            Assert.Equal(
                condense(@"SELECT [e].[Id] FROM [Events] AS [e] WHERE [e].[StartTime] = @__startTime_0"),
                condense(_db.Sql));

            Assert.Equal(new[] { 1 }, results);
        }

        [Fact]
        public async Task DateOnly_AddYears()
        {
            var results = await _db.Events.Where(e => e.StartDate.AddYears(1) >= new DateOnly(2019, 7, 1)).ToListAsync();

            Assert.Equal(
               condense(@$"{SelectStatement} WHERE DATEADD(year, CAST(1 AS int), [e].[StartDate]) >= '2019-07-01'"),
               condense(_db.Sql));

            Assert.Equal(2, results.Count);
        }

        [Fact]
        public async Task DateOnly_AddMonths()
        {
            var results = await _db.Events.Where(r => r.StartDate.AddMonths(1) >= new DateOnly(2019, 7, 1)).ToListAsync();

            Assert.Equal(
                condense(@$"{SelectStatement} WHERE DATEADD(month, CAST(1 AS int), [e].[StartDate]) >= '2019-07-01'"),
                condense(_db.Sql));

            Assert.Equal(2, results.Count);
        }

        [Fact]
        public async Task DateOnly_AddDays()
        {
            var results = await _db.Events.Where(r => r.StartDate.AddDays(45) >= new DateOnly(2019, 7, 1)).ToListAsync();

            Assert.Equal(
                condense(@$"{SelectStatement} WHERE DATEADD(day, CAST(45 AS int), [e].[StartDate]) >= '2019-07-01'"),
                condense(_db.Sql));

            Assert.Equal(2, results.Count);
        }

        [Fact]
        public async Task DateOnly_DatePart_Year()
        {
            var results = await _db.Events.Where(r => r.StartDate.Year == 2022).ToListAsync();
            Assert.Equal(
                condense(@$"{SelectStatement} WHERE DATEPART(year, [e].[StartDate]) = 2022"),
                condense(_db.Sql));

            Assert.Equal(2, results.Count);
        }

        [Fact]
        public async Task DateOnly_DatePart_Month()
        {
            var results = await _db.Events.Where(r => r.StartDate.Month == 12).ToListAsync();
            Assert.Equal(
                condense(@$"{SelectStatement} WHERE DATEPART(month, [e].[StartDate]) = 12"),
                condense(_db.Sql));

            Assert.Equal(2, results.Count);
        }

        [Fact]
        public async Task DateOnly_DatePart_DayOfYear()
        {
            var results = await _db.Events.Where(e => e.StartDate.DayOfYear == 1).ToListAsync();
            Assert.Equal(
               condense(@$"{SelectStatement} WHERE DATEPART(dayofyear, [e].[StartDate]) = 1"),
               condense(_db.Sql));

            Assert.Single(results);
        }

        [Fact]
        public async Task DateOnly_DatePart_Day()
        {
            var results = await _db.Events.Where(r => r.StartDate.Day == 1).ToListAsync();
            Assert.Equal(
                condense(@$"{SelectStatement} WHERE DATEPART(day, [e].[StartDate]) = 1"),
                condense(_db.Sql));

            Assert.Single(results);
        }

        public void Dispose()
        {
            _db.Dispose();
        }

        // replace whitespace with a single space
        private static string condense(string str)
        {
            var split = str.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
            return string.Join(" ", split);
        }
    }
}
