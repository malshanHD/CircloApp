using CircloApp.Application.Common.Interfaces;
using CircloApp.Application.Exceptions;
using CircloApp.Application.Features.Authentication.Commands.GoogleLogin;
using CircloApp.Application.Features.Authentication.Commands.Login;
using CircloApp.Application.Features.Authentication.DTOs;
using CircloApp.Application.Interfaces;
using CircloApp.Domain.Entities;
using CircloApp.Infrastructure.Authentication;
using CircloApp.Infrastructure.Persistence;
using CircloApp.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;

namespace CircloApp.API.Tests.Authentication;

public sealed class LocalSqlFactAttribute : FactAttribute
{
    public LocalSqlFactAttribute()
    {
        if (Environment.GetEnvironmentVariable("CIRCLO_RUN_LOCAL_SQL") != "1")
            Skip="Opt-in disposable SQL Server LocalDB test. Set CIRCLO_RUN_LOCAL_SQL=1 on Windows.";
    }
}

public class GoogleSqlTests
{
    private sealed class Clock : IDateTimeProvider { public DateTime UtcNow=>DateTime.UtcNow; }
    private sealed class Passwords : IPasswordHasher
    {
        public string HashPassword(string value)=>"test-hash:"+value;
        public bool VerifyPassword(string value,string hash)=>hash==HashPassword(value);
    }
    private sealed class Validator(GoogleUserInfo? info) : IGoogleTokenValidator
    { public Task<GoogleUserInfo?> ValidateAsync(string token,CancellationToken ct=default)=>Task.FromResult(info); }
    private static GoogleUserInfo Info(string subject,string email,bool authoritative=true)=>new(subject,email,authoritative,"Test","Member","Test Member",null);
    private static JwtTokenGenerator Jwt()=>new(Options.Create(new JwtSettings{Secret=new string('s',64),Issuer="tests",Audience="tests",ExpiryMinutes=43}));
    private static GoogleLoginCommandHandler Handler(ApplicationDbContext db,GoogleUserInfo? info)=>new(
        new UserRepository(db),new Validator(info),new Passwords(),new Clock(),Jwt(),new RefreshTokenGenerator(),new UnitOfWork(db));
    private static GoogleLoginCommand Command()=>new(new GoogleLoginRequest{IdToken="synthetic-validated-google-token"});

    [LocalSqlFact]
    public async Task Sql_server_reproduces_old_failure_and_verifies_new_existing_returning_and_password_login()
    {
        var database="Circlo_GoogleLoginTests_"+Guid.NewGuid().ToString("N");
        var connection=$"Server=(localdb)\\MSSQLLocalDB;Database={database};Integrated Security=true;TrustServerCertificate=true";
        ApplicationDbContext Context()=>new(new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlServer(connection).Options);
        await using var setup=Context();
        try
        {
            await setup.Database.EnsureCreatedAsync();
            var local=new User{Email="existing@gmail.com",Username="local",PasswordHash=new Passwords().HashPassword("password123")};
            setup.Users.Add(local);await setup.SaveChangesAsync();var localId=local.Id;
            // Reproduce the actual zero-row UPDATE and transaction rollback on an isolated database.
            await using(var broken=Context())
            {
                var tracked=await broken.Users.SingleAsync(u=>u.Id==localId);
                tracked.EmailVerified=true; tracked.RefreshToken="must-rollback";
                tracked.AddExternalLogin("Google","repro-only");
                await Assert.ThrowsAsync<DbUpdateConcurrencyException>(()=>broken.SaveChangesAsync());
            }
            await using(var rollback=Context())
            {
                var stored=await rollback.Users.SingleAsync(u=>u.Id==localId);
                Assert.False(stored.EmailVerified);Assert.Null(stored.RefreshToken);
            }
            LoginResponse created;
            await using(var db=Context())
            {
                var saves=0;db.SavingChanges+=(_,_)=>saves++;
                created=await Handler(db,Info("new-subject","new@gmail.com")).Handle(Command(),default);
                Assert.Equal(1,saves);
            }
            Assert.NotEqual(Guid.Empty,created.UserId);
            Assert.Equal(created.UserId.ToString(),new JwtSecurityTokenHandler().ReadJwtToken(created.AccessToken).Subject);
            Assert.Equal(created.ExpiresAt,new JwtSecurityTokenHandler().ReadJwtToken(created.AccessToken).ValidTo);
            await using(var db=Context())
            {
                var stored=await db.Users.Include(u=>u.ExternalLogins).SingleAsync(u=>u.Id==created.UserId);
                Assert.True(stored.EmailVerified);Assert.Equal(created.RefreshToken,stored.RefreshToken);
                Assert.NotEqual(created.AccessToken,stored.RefreshToken);Assert.Equal("new-subject",Assert.Single(stored.ExternalLogins).ProviderSubject);
            }
            for(var i=0;i<3;i++)
            {
                await using var db=Context();
                var returning=await Handler(db,Info("new-subject","new@gmail.com")).Handle(Command(),default);
                Assert.Equal(created.UserId,returning.UserId);Assert.NotEqual(created.RefreshToken,returning.RefreshToken);
                Assert.Equal(1,await db.UserExternalLogins.CountAsync(l=>l.UserId==created.UserId));
            }
            await using(var db=Context())
            {
                var saves=0;db.SavingChanges+=(_,_)=>saves++;
                var linked=await Handler(db,Info("existing-subject","existing@gmail.com")).Handle(Command(),default);
                Assert.Equal(1,saves);
                Assert.Equal(localId,linked.UserId);
                Assert.Equal(EntityState.Unchanged,db.Entry(await db.Users.SingleAsync(u=>u.Id==localId)).State);
                Assert.Equal(1,await db.UserExternalLogins.CountAsync(l=>l.UserId==localId));
            }
            await using(var db=Context())
            {
                var normal=new LoginCommandHandler(new UserRepository(db),new Passwords(),new Clock(),Jwt(),new UnitOfWork(db),new RefreshTokenGenerator());
                var response=await normal.Handle(new LoginCommand(new LoginRequest{UsernameOrEmail="local",Password="password123"}),default);
                Assert.Equal(localId,response.UserId);
                Assert.Equal(response.RefreshToken,(await db.Users.SingleAsync(u=>u.Id==localId)).RefreshToken);
            }
            await using(var db=Context())
            {
                await Assert.ThrowsAsync<BadRequestException>(()=>Handler(db,Info("untrusted","existing@gmail.com",false)).Handle(Command(),default));
                await Assert.ThrowsAsync<BadRequestException>(()=>Handler(db,null).Handle(Command(),default));
                Assert.Equal(2,await db.UserExternalLogins.CountAsync());
                var repo=new UserRepository(db);
                Assert.Null(await repo.GetByExternalLoginAsync("Other","new-subject"));
                Assert.Null(await repo.GetByExternalLoginAsync("Google","wrong-subject"));
            }
            await using(var db=Context())
            {
                var legacy=new User{Email="legacy@gmail.com",Username="legacy",PasswordHash="unchanged"};
                legacy.AddExternalLogin("Google",legacy.Email);db.Users.Add(legacy);await db.SaveChangesAsync();
            }
            await using(var db=Context())
            {
                var response=await Handler(db,Info("legacy-subject","legacy@gmail.com")).Handle(Command(),default);
                var login=Assert.Single(await db.UserExternalLogins.Where(l=>l.UserId==response.UserId).ToListAsync());
                Assert.Equal("legacy-subject",login.ProviderSubject);
            }
        }
        finally
        {
            // This test owns only this freshly generated database, never the application's database.
            Assert.StartsWith("Circlo_GoogleLoginTests_",database);
            await setup.Database.EnsureDeletedAsync();
        }
    }
}
