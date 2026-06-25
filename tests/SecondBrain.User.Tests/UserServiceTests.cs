using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using SecondBrain.BuildingBlocks.Core.Repositories;
using SecondBrain.Services.UserService.Entites;
using SecondBrain.Services.UserService.Models;
using System.Linq.Expressions;
using Xunit;
using UserServiceImpl = SecondBrain.Services.UserService.Services.Implementations.UserService;

namespace SecondBrain.User.Tests;

public class UserServiceTests
{
    private readonly Mock<IRepository<UserEntity, Guid>> _repositoryMock;
    private readonly JwtSettings _jwtSettings;
    private readonly UserServiceImpl _sut;

    public UserServiceTests()
    {
        _repositoryMock = new Mock<IRepository<UserEntity, Guid>>();

        _jwtSettings = new JwtSettings
        {
            Secret = "this-is-a-test-secret-key-with-32+chars",
            Issuer = "SecondBrain.Tests",
            Audience = "SecondBrain.Tests.Clients",
            ExpiryHours = 1
        };

        var optionsMock = new Mock<IOptions<JwtSettings>>();
        optionsMock.Setup(o => o.Value).Returns(_jwtSettings);

        _sut = new UserServiceImpl(_repositoryMock.Object, optionsMock.Object);
    }

    private static RegisterRequestDto ValidRegisterRequest(
        string userName = "johndoe",
        string email = "john@example.com",
        string password = "Passw0rd") =>
        new()
        {
            UserName = userName,
            Email = email,
            Password = password
        };

    // ---------- RegisterAsync ----------

    [Fact]
    public async Task RegisterAsync_WithValidData_CreatesUserAndHashesPassword()
    {
        var dto = ValidRegisterRequest();

        _repositoryMock
            .Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<UserEntity, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        UserEntity? createdEntity = null;
        _repositoryMock
            .Setup(r => r.CreateAsync(It.IsAny<UserEntity>(), It.IsAny<CancellationToken>()))
            .Callback<UserEntity, CancellationToken>((u, _) => createdEntity = u)
            .ReturnsAsync((UserEntity u, CancellationToken _) => u);

        var result = await _sut.RegisterAsync(dto);

        result.Should().NotBeNull();
        result.Email.Should().Be(dto.Email);
        result.UserName.Should().Be(dto.UserName);
        result.IsActive.Should().BeTrue();
        result.PasswordHash.Should().NotBeNullOrEmpty();
        result.PasswordHash.Should().NotBe(dto.Password);
        BCrypt.Net.BCrypt.Verify(dto.Password, result.PasswordHash).Should().BeTrue();

        createdEntity.Should().NotBeNull();
        _repositoryMock.Verify(r => r.CreateAsync(It.IsAny<UserEntity>(), It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_WhenEmailAlreadyExists_ThrowsInvalidOperationException()
    {
        var dto = ValidRegisterRequest();

        _repositoryMock
            .Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<UserEntity, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var act = async () => await _sut.RegisterAsync(dto);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*email already exists*");

        _repositoryMock.Verify(r => r.CreateAsync(It.IsAny<UserEntity>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RegisterAsync_WhenUserNameAlreadyExists_ThrowsInvalidOperationException()
    {
        var dto = ValidRegisterRequest();

        _repositoryMock
            .SetupSequence(r => r.ExistsAsync(It.IsAny<Expression<Func<UserEntity, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false)  // email check
            .ReturnsAsync(true);  // username check

        var act = async () => await _sut.RegisterAsync(dto);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*username already exists*");
    }

    [Theory]
    [InlineData("short")]      // too short
    [InlineData("")]
    [InlineData(null)]
    public async Task RegisterAsync_WithInvalidPasswordLength_ThrowsInvalidOperationException(string? password)
    {
        var dto = ValidRegisterRequest(password: password!);

        var act = async () => await _sut.RegisterAsync(dto);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*at least 6 characters*");
    }

    [Fact]
    public async Task RegisterAsync_WithPasswordMissingDigit_ThrowsInvalidOperationException()
    {
        var dto = ValidRegisterRequest(password: "Password");

        var act = async () => await _sut.RegisterAsync(dto);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*digit*");
    }

    [Fact]
    public async Task RegisterAsync_WithPasswordMissingUppercase_ThrowsInvalidOperationException()
    {
        var dto = ValidRegisterRequest(password: "password1");

        var act = async () => await _sut.RegisterAsync(dto);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*uppercase*");
    }

    [Fact]
    public async Task RegisterAsync_DoesNotCheckEmailTwice()
    {
        // Regression test: previous version of RegisterAsync called
        // ExistsAsync(email) twice. After the fix, the email-existence
        // predicate should only be evaluated once.
        var dto = ValidRegisterRequest();
        var emailCheckCount = 0;

        _repositoryMock
            .Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<UserEntity, bool>>>(), It.IsAny<CancellationToken>()))
            .Callback<Expression<Func<UserEntity, bool>>, CancellationToken>((predicate, _) =>
            {
                var compiled = predicate.Compile();
                var probe = new UserEntity
                {
                    Id = Guid.NewGuid(),
                    Email = dto.Email,
                    UserName = "someone-else",
                    PasswordHash = "x",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                if (compiled(probe)) emailCheckCount++;
            })
            .ReturnsAsync(false);

        await _sut.RegisterAsync(dto);

        emailCheckCount.Should().Be(1);
    }

    // ---------- LoginAsync ----------

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsJwtTokenAndUpdatesLastLogin()
    {
        var password = "Passw0rd";
        var user = new UserEntity
        {
            Id = Guid.NewGuid(),
            Email = "john@example.com",
            UserName = "johndoe",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _repositoryMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<UserEntity, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UserEntity> { user });

        var token = await _sut.LoginAsync(new LoginRequestDto { Email = user.Email, Password = password });

        token.Should().NotBeNullOrEmpty();
        user.LastLoginAt.Should().NotBeNull();
        _repositoryMock.Verify(r => r.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_WithUnknownEmail_ThrowsUnauthorizedAccessException()
    {
        _repositoryMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<UserEntity, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UserEntity>());

        var act = async () => await _sut.LoginAsync(new LoginRequestDto { Email = "nobody@example.com", Password = "Passw0rd" });

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task LoginAsync_WithWrongPassword_ThrowsUnauthorizedAccessException()
    {
        var user = new UserEntity
        {
            Id = Guid.NewGuid(),
            Email = "john@example.com",
            UserName = "johndoe",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Passw0rd"),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _repositoryMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<UserEntity, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UserEntity> { user });

        var act = async () => await _sut.LoginAsync(new LoginRequestDto { Email = user.Email, Password = "WrongPass1" });

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task LoginAsync_WithInactiveUser_ThrowsUnauthorizedAccessException()
    {
        var password = "Passw0rd";
        var user = new UserEntity
        {
            Id = Guid.NewGuid(),
            Email = "john@example.com",
            UserName = "johndoe",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            IsActive = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _repositoryMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<UserEntity, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UserEntity> { user });

        var act = async () => await _sut.LoginAsync(new LoginRequestDto { Email = user.Email, Password = password });

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*inactive*");

        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<UserEntity>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // ---------- GetByIdAsync ----------

    [Fact]
    public async Task GetByIdAsync_WithExistingId_ReturnsUser()
    {
        var user = new UserEntity
        {
            Id = Guid.NewGuid(),
            Email = "john@example.com",
            UserName = "johndoe",
            PasswordHash = "hash",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _repositoryMock
            .Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _sut.GetByIdAsync(user.Id);

        result.Should().Be(user);
    }

    [Fact]
    public async Task GetByIdAsync_WithMissingId_ThrowsKeyNotFoundException()
    {
        var id = Guid.NewGuid();

        _repositoryMock
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserEntity?)null);

        var act = async () => await _sut.GetByIdAsync(id);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    // ---------- GetByEmailAsync ----------

    [Fact]
    public async Task GetByEmailAsync_WithExistingEmail_ReturnsUser()
    {
        var user = new UserEntity
        {
            Id = Guid.NewGuid(),
            Email = "john@example.com",
            UserName = "johndoe",
            PasswordHash = "hash",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _repositoryMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<UserEntity, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UserEntity> { user });

        var result = await _sut.GetByEmailAsync(user.Email);

        result.Should().Be(user);
    }

    [Fact]
    public async Task GetByEmailAsync_WithMissingEmail_ThrowsKeyNotFoundException()
    {
        _repositoryMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<UserEntity, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UserEntity>());

        var act = async () => await _sut.GetByEmailAsync("nobody@example.com");

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    // ---------- ValidateUserAsync ----------

    [Fact]
    public async Task ValidateUserAsync_WithActiveUser_ReturnsTrue()
    {
        var user = new UserEntity
        {
            Id = Guid.NewGuid(),
            Email = "john@example.com",
            UserName = "johndoe",
            PasswordHash = "hash",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _repositoryMock
            .Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _sut.ValidateUserAsync(user.Id);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateUserAsync_WithInactiveUser_ReturnsFalse()
    {
        var user = new UserEntity
        {
            Id = Guid.NewGuid(),
            Email = "john@example.com",
            UserName = "johndoe",
            PasswordHash = "hash",
            IsActive = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _repositoryMock
            .Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _sut.ValidateUserAsync(user.Id);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateUserAsync_WithMissingUser_ReturnsFalse()
    {
        var id = Guid.NewGuid();

        _repositoryMock
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserEntity?)null);

        var result = await _sut.ValidateUserAsync(id);

        result.Should().BeFalse();
    }
}