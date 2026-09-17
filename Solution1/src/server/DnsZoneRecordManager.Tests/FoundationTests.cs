using DnsZoneRecordManager.Data;
using DnsZoneRecordManager.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DnsZoneRecordManager.Tests
{
    /// <summary>Seeder tests: seeds once, then leaves existing data alone.</summary>
    public class DbSeederTests
    {
        [Fact]
        public async Task should_seed_sample_zone_once()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase("SeederDb-" + Guid.NewGuid())
                .Options;
            await using var context = new AppDbContext(options);

            await DbSeeder.SeedAsync(context);
            context.Zones.Should().ContainSingle(z => z.Name == "nahuexolab.com");
            context.Records.Should().HaveCount(5);
            context.Records.Count(r => r.Type == RecordType.NS).Should().Be(4);
            context
                .Records.Should()
                .ContainSingle(r => r.Name == "_dmarc" && r.Type == RecordType.TXT);

            await DbSeeder.SeedAsync(context);
            context.Zones.Should().ContainSingle();
            context.Records.Should().HaveCount(5);
        }
    }

    /// <summary>Home controller tests (integration suite covers the request-Activity path).</summary>
    public class HomeControllerTests
    {
        [Fact]
        public void should_render_index_and_privacy()
        {
            var controller = new Controllers.HomeController();
            controller.Index().Should().BeOfType<ViewResult>();
            controller.Privacy().Should().BeOfType<ViewResult>();
        }

        [Fact]
        public void should_render_error_without_ambient_activity()
        {
            var controller = new Controllers.HomeController
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext(),
                },
            };

            var result = controller.Error().Should().BeOfType<ViewResult>().Subject;
            var model = result.Model.Should().BeOfType<ErrorViewModel>().Subject;
            model.RequestId.Should().Be(controller.HttpContext.TraceIdentifier);
            model.ShowRequestId.Should().BeTrue();
            new ErrorViewModel { RequestId = null }
                .ShowRequestId.Should()
                .BeFalse();
        }
    }
}
