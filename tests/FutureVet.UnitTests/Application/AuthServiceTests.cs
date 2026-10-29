using FutureVet.Application.DTOs.Auth;
using FutureVet.Application.Interfaces.Repositories;
using FutureVet.Application.Interfaces.Services;
using FutureVet.Application.Services;
using FutureVet.Domain.Entities;
using FutureVet.UnitTests.Common;
using Moq;

namespace FutureVet.UnitTests.Application;

public class AuthServiceTests
{
    private const string SenhaCorreta = "SenhaSegura123";

    private readonly Mock<IUsuarioRepository> _repositoryMock = new(MockBehavior.Strict);
    private readonly Mock<ITokenGenerator> _tokenGeneratorMock = new(MockBehavior.Strict);
    private readonly AuthService _service;

    public AuthServiceTests()
    {
        _service = new AuthService(_repositoryMock.Object, _tokenGeneratorMock.Object);
    }

    [Fact]
    public async Task AutenticarAsync_CredenciaisValidas_RetornaTokenEDadosDoUsuario()
    {
        // Arrange
        var usuario = TestData.UsuarioValido(senha: SenhaCorreta);
        var expiraEm = new DateTime(2026, 11, 4, 12, 0, 0, DateTimeKind.Utc);

        _repositoryMock
            .Setup(x => x.GetByEmailAsync(usuario.Email))
            .ReturnsAsync(usuario);

        _tokenGeneratorMock
            .Setup(x => x.Gerar(usuario))
            .Returns(("token-jwt", expiraEm));

        // Act
        var response = await _service.AutenticarAsync(new LoginRequest(usuario.Email, SenhaCorreta));

        // Assert
        Assert.NotNull(response);
        Assert.Equal(usuario.Id, response.Id);
        Assert.Equal(usuario.Nome, response.Nome);
        Assert.Equal("token-jwt", response.Token);
        Assert.Equal(expiraEm, response.ExpiraEm);
        _tokenGeneratorMock.Verify(x => x.Gerar(usuario), Times.Once);
    }

    [Fact]
    public async Task AutenticarAsync_SenhaIncorreta_RetornaNullSemEmitirToken()
    {
        // Arrange
        var usuario = TestData.UsuarioValido(senha: SenhaCorreta);

        _repositoryMock
            .Setup(x => x.GetByEmailAsync(usuario.Email))
            .ReturnsAsync(usuario);

        // Act
        var response = await _service.AutenticarAsync(new LoginRequest(usuario.Email, "SenhaErrada99"));

        // Assert
        Assert.Null(response);
        _tokenGeneratorMock.Verify(x => x.Gerar(It.IsAny<Usuario>()), Times.Never);
    }

    [Fact]
    public async Task AutenticarAsync_EmailNaoCadastrado_RetornaNullSemEmitirToken()
    {
        // Arrange
        const string email = "ninguem@futurevet.com";

        _repositoryMock
            .Setup(x => x.GetByEmailAsync(email))
            .ReturnsAsync((Usuario?)null);

        // Act
        var response = await _service.AutenticarAsync(new LoginRequest(email, SenhaCorreta));

        // Assert
        Assert.Null(response);
        _tokenGeneratorMock.Verify(x => x.Gerar(It.IsAny<Usuario>()), Times.Never);
    }

    [Theory]
    [InlineData("")]
    [InlineData("senhasegura123")]
    [InlineData("SenhaSegura1234")]
    public async Task AutenticarAsync_SenhaParecidaComACorreta_RetornaNull(string senhaInformada)
    {
        // Arrange
        var usuario = TestData.UsuarioValido(senha: SenhaCorreta);

        _repositoryMock
            .Setup(x => x.GetByEmailAsync(usuario.Email))
            .ReturnsAsync(usuario);

        // Act
        var response = await _service.AutenticarAsync(new LoginRequest(usuario.Email, senhaInformada));

        // Assert
        Assert.Null(response);
    }
}
