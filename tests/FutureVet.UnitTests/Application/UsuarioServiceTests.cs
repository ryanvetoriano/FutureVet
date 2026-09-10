using FutureVet.Application.DTOs.Usuario;
using FutureVet.Application.Interfaces.Repositories;
using FutureVet.Application.Services;
using FutureVet.Domain.Entities;
using FutureVet.Domain.Exceptions;
using FutureVet.UnitTests.Common;
using Moq;

namespace FutureVet.UnitTests.Application;

public class UsuarioServiceTests
{
    private readonly Mock<IUsuarioRepository> _repositoryMock = new(MockBehavior.Strict);
    private readonly UsuarioService _service;

    public UsuarioServiceTests()
    {
        _service = new UsuarioService(_repositoryMock.Object);
    }

    [Fact]
    public async Task CreateAsync_DadosValidos_PersisteUsuarioERetornaResponse()
    {
        // Arrange
        var request = TestData.CreateUsuarioRequestValido();
        Usuario? usuarioPersistido = null;

        _repositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Usuario>()))
            .Callback<Usuario>(u => usuarioPersistido = u)
            .Returns(Task.CompletedTask);

        // Act
        var response = await _service.CreateAsync(request);

        // Assert
        Assert.NotNull(usuarioPersistido);
        Assert.Equal(usuarioPersistido.Id, response.Id);
        Assert.Equal(request.Nome, response.Nome);
        Assert.Equal(request.Email, response.Email);
        Assert.Equal(request.Cpf, response.Cpf);
        _repositoryMock.Verify(x => x.AddAsync(It.IsAny<Usuario>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_EmailInvalido_LancaDomainExceptionENaoPersiste()
    {
        // Arrange
        var request = new CreateUsuarioRequest(
            "Ryan", "email-sem-arroba", "SenhaSegura123", "12345678901", "11999998888");

        // Act
        var excecao = await Record.ExceptionAsync(() => _service.CreateAsync(request));

        // Assert
        Assert.IsType<DomainException>(excecao);
        _repositoryMock.Verify(x => x.AddAsync(It.IsAny<Usuario>()), Times.Never);
    }

    [Fact]
    public async Task GetByIdAsync_UsuarioExiste_RetornaResponseCorrespondente()
    {
        // Arrange
        var usuario = TestData.UsuarioValido();

        _repositoryMock
            .Setup(x => x.GetByIdAsync(usuario.Id))
            .ReturnsAsync(usuario);

        // Act
        var response = await _service.GetByIdAsync(usuario.Id);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(usuario.Id, response.Id);
        Assert.Equal(usuario.Email, response.Email);
    }

    [Fact]
    public async Task GetByIdAsync_UsuarioNaoEncontrado_RetornaNull()
    {
        // Arrange
        var idInexistente = Guid.NewGuid();

        _repositoryMock
            .Setup(x => x.GetByIdAsync(idInexistente))
            .ReturnsAsync((Usuario?)null);

        // Act
        var response = await _service.GetByIdAsync(idInexistente);

        // Assert
        Assert.Null(response);
    }

    [Fact]
    public async Task GetByEmailAsync_EmailCadastrado_RetornaUsuario()
    {
        // Arrange
        var usuario = TestData.UsuarioValido(email: "ryan@futurevet.com");

        _repositoryMock
            .Setup(x => x.GetByEmailAsync("ryan@futurevet.com"))
            .ReturnsAsync(usuario);

        // Act
        var response = await _service.GetByEmailAsync("ryan@futurevet.com");

        // Assert
        Assert.NotNull(response);
        Assert.Equal("ryan@futurevet.com", response.Email);
    }

    [Fact]
    public async Task GetAllAsync_SemUsuariosCadastrados_RetornaColecaoVazia()
    {
        // Arrange
        _repositoryMock
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync([]);

        // Act
        var response = await _service.GetAllAsync();

        // Assert
        Assert.Empty(response);
    }

    [Fact]
    public async Task GetAllAsync_ComUsuariosCadastrados_MapeiaTodosOsRegistros()
    {
        // Arrange
        var usuarios = new List<Usuario>
        {
            TestData.UsuarioValido(email: "a@futurevet.com", cpf: "11111111111"),
            TestData.UsuarioValido(email: "b@futurevet.com", cpf: "22222222222")
        };

        _repositoryMock
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(usuarios);

        // Act
        var response = (await _service.GetAllAsync()).ToList();

        // Assert
        Assert.Equal(2, response.Count);
        Assert.Contains(response, r => r.Email == "a@futurevet.com");
        Assert.Contains(response, r => r.Email == "b@futurevet.com");
    }

    [Fact]
    public async Task UpdateAsync_UsuarioExiste_AtualizaNomeETelefone()
    {
        // Arrange
        var usuario = TestData.UsuarioValido(nome: "Nome Antigo", telefone: "11000000000");
        var request = new UpdateUsuarioRequest("Nome Novo", "11988887777");

        _repositoryMock
            .Setup(x => x.GetByIdAsync(usuario.Id))
            .ReturnsAsync(usuario);

        _repositoryMock
            .Setup(x => x.UpdateAsync(usuario))
            .Returns(Task.CompletedTask);

        // Act
        await _service.UpdateAsync(usuario.Id, request);

        // Assert
        Assert.Equal("Nome Novo", usuario.Nome);
        Assert.Equal("11988887777", usuario.Telefone);
        _repositoryMock.Verify(x => x.UpdateAsync(usuario), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_UsuarioNaoEncontrado_LancaNotFoundException()
    {
        // Arrange
        var idInexistente = Guid.NewGuid();

        _repositoryMock
            .Setup(x => x.GetByIdAsync(idInexistente))
            .ReturnsAsync((Usuario?)null);

        // Act
        var excecao = await Record.ExceptionAsync(
            () => _service.UpdateAsync(idInexistente, TestData.UpdateUsuarioRequestValido()));

        // Assert
        var notFound = Assert.IsType<NotFoundException>(excecao);
        Assert.Contains(idInexistente.ToString(), notFound.Message);
        _repositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Usuario>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_NomeVazio_LancaDomainExceptionENaoPersiste()
    {
        // Arrange
        var usuario = TestData.UsuarioValido();

        _repositoryMock
            .Setup(x => x.GetByIdAsync(usuario.Id))
            .ReturnsAsync(usuario);

        // Act
        var excecao = await Record.ExceptionAsync(
            () => _service.UpdateAsync(usuario.Id, new UpdateUsuarioRequest("  ", "11988887777")));

        // Assert
        Assert.IsType<DomainException>(excecao);
        _repositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Usuario>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_UsuarioExiste_RemoveUsuario()
    {
        // Arrange
        var usuario = TestData.UsuarioValido();

        _repositoryMock
            .Setup(x => x.GetByIdAsync(usuario.Id))
            .ReturnsAsync(usuario);

        _repositoryMock
            .Setup(x => x.DeleteAsync(usuario))
            .Returns(Task.CompletedTask);

        // Act
        await _service.DeleteAsync(usuario.Id);

        // Assert
        _repositoryMock.Verify(x => x.DeleteAsync(usuario), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_UsuarioNaoEncontrado_NaoChamaRepositorioDeExclusao()
    {
        // Arrange
        var idInexistente = Guid.NewGuid();

        _repositoryMock
            .Setup(x => x.GetByIdAsync(idInexistente))
            .ReturnsAsync((Usuario?)null);

        // Act
        await _service.DeleteAsync(idInexistente);

        // Assert
        _repositoryMock.Verify(x => x.DeleteAsync(It.IsAny<Usuario>()), Times.Never);
    }
}
