using CircloApp.Domain.Entities;
using CircloApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CircloApp.API.Tests.Authentication;

public class GoogleTrackingTests
{
    private static ApplicationDbContext Context() => new(new DbContextOptionsBuilder<ApplicationDbContext>()
        .UseSqlServer("Server=unused;Database=tracking_only;Integrated Security=true;TrustServerCertificate=true")
        .Options);

    [Fact]
    public void Reproduces_original_navigation_discovery_treating_prekeyed_login_as_modified()
    {
        using var db = Context();
        var user = new User { Id = Guid.NewGuid() };
        db.Attach(user);
        user.AddExternalLogin("Google", "subject-123");
        db.ChangeTracker.DetectChanges();
        Assert.Equal(EntityState.Unchanged, db.Entry(user).State);
        // No database exists for this new row; UPDATE would affect zero rows.
        Assert.Equal(EntityState.Modified, db.Entry(user.ExternalLogins.Single()).State);
    }
}

public class GoogleTrackingRegressionTests
{
    private static ApplicationDbContext Context() => new(new DbContextOptionsBuilder<ApplicationDbContext>()
        .UseSqlServer("Server=unused;Database=tracking_only;Integrated Security=true;TrustServerCertificate=true").Options);
    [Fact] public async Task Explicit_add_inserts_link_without_updating_the_existing_user_graph()
    {
        using var db=Context(); var user=new User{Id=Guid.NewGuid()}; db.Attach(user);
        var link=user.AddExternalLogin("Google","subject")!;
        await new CircloApp.Infrastructure.Repositories.UserRepository(db).AddExternalLoginAsync(link,default);
        user.RefreshToken="actual-refresh"; db.ChangeTracker.DetectChanges();
        Assert.Equal(EntityState.Modified,db.Entry(user).State);
        Assert.Equal(EntityState.Added,db.Entry(link).State);
        Assert.Equal(user.Id,link.UserId);
        Assert.Null(user.AddExternalLogin("Google","subject")); Assert.Single(user.ExternalLogins);
    }
    [Fact] public async Task New_user_graph_has_added_states_and_matching_keys()
    {
        using var db=Context();var user=new User();var link=user.AddExternalLogin("Google","subject")!;
        await new CircloApp.Infrastructure.Repositories.UserRepository(db).AddAsync(user,default);
        Assert.Equal(EntityState.Added,db.Entry(user).State);Assert.Equal(EntityState.Added,db.Entry(link).State);
        Assert.NotEqual(Guid.Empty,user.Id);Assert.Equal(user.Id,link.UserId);
    }
    [Fact] public void Composite_provider_subject_index_is_unique()
    {
        using var db=Context();
        Assert.Contains(db.Model.FindEntityType(typeof(UserExternalLogin))!.GetIndexes(),i=>i.IsUnique && i.Properties.Select(p=>p.Name).SequenceEqual(new[]{"Provider","ProviderSubject"}));
    }
    [Fact] public void Jwt_metadata_matches_configured_expiry_and_actual_token()
    {
        var generator=new CircloApp.Infrastructure.Authentication.JwtTokenGenerator(Microsoft.Extensions.Options.Options.Create(
            new CircloApp.Infrastructure.Authentication.JwtSettings{Secret=new string('s',64),Issuer="test",Audience="test",ExpiryMinutes=43}));
        var issued=generator.GenerateToken(new User{Id=Guid.NewGuid(),Email="test@example.test",Username="test"});
        var decoded=new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().ReadJwtToken(issued.AccessToken);
        Assert.Equal(decoded.ValidTo,issued.ExpiresAt);
        Assert.InRange(issued.ExpiresAt-DateTime.UtcNow,TimeSpan.FromMinutes(42.9),TimeSpan.FromMinutes(43.1));
    }
}
