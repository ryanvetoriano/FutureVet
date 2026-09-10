using FutureVet.Domain.Entities;
using FutureVet.Domain.Exceptions;
using FutureVet.UnitTests.Common;

namespace FutureVet.UnitTests.Domain;

public class UsuarioTests
{
    [Fact]
    public void Construtor_DadosValidos_CriaUsuarioDisponivelComIdGerado()
    {
        // Arrange
        const string nome = "Ryan Vetoriano";
        const string email = "ryan@futurevet.com";
        const string cpf = "12345678901";

        // Act
        var usuario = new Usuario(nome, email, "SenhaSegura123", cpf, "11999998888");

        // Assert
        Assert.NotEqual(Guid.Empty, usuario.Id);
        Assert.Equal(nome, usuario.Nome);
        Assert.Equal(email, usuario.Email);
        Assert.Equal(cpf, usuario.Cpf);
        Assert.True(usuario.Disponivel);
        Assert.Empty(usuario.Pets);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Construtor_NomeVazio_LancaDomainException(string? nomeInvalido)
    {
        // Arrange
        var acao = () => new Usuario(nomeInvalido!, "ryan@futurevet.com", "SenhaSegura123", "12345678901", "11999998888");

        // Act
        var excecao = Record.Exception(acao);

        // Assert
        var domainException = Assert.IsType<DomainException>(excecao);
        Assert.Equal("Nome inválido.", domainException.Message);
    }

    [Theory]
    [InlineData("sem-arroba.com")]
    [InlineData("")]
    [InlineData("   ")]
    public void Construtor_EmailSemArroba_LancaDomainException(string emailInvalido)
    {
        // Arrange
        var acao = () => new Usuario("Ryan", emailInvalido, "SenhaSegura123", "12345678901", "11999998888");

        // Act
        var excecao = Record.Exception(acao);

        // Assert
        var domainException = Assert.IsType<DomainException>(excecao);
        Assert.Equal("Email inválido.", domainException.Message);
    }

    [Theory]
    [InlineData("1234567")]  // 7 caracteres: um abaixo do limite
    [InlineData("")]
    [InlineData("   ")]
    public void Construtor_SenhaComMenosDeOitoCaracteres_LancaDomainException(string senhaInvalida)
    {
        // Arrange
        var acao = () => new Usuario("Ryan", "ryan@futurevet.com", senhaInvalida, "12345678901", "11999998888");

        // Act
        var excecao = Record.Exception(acao);

        // Assert
        var domainException = Assert.IsType<DomainException>(excecao);
        Assert.Equal("Senha deve possuir no mínimo 8 caracteres.", domainException.Message);
    }

    [Fact]
    public void Construtor_SenhaComExatamenteOitoCaracteres_CriaUsuario()
    {
        // Arrange
        const string senhaNoLimite = "12345678";

        // Act
        var usuario = new Usuario("Ryan", "ryan@futurevet.com", senhaNoLimite, "12345678901", "11999998888");

        // Assert
        Assert.Equal(senhaNoLimite, usuario.Senha);
    }

    [Fact]
    public void Construtor_CpfVazio_LancaDomainException()
    {
        // Arrange
        var acao = () => new Usuario("Ryan", "ryan@futurevet.com", "SenhaSegura123", "  ", "11999998888");

        // Act
        var excecao = Record.Exception(acao);

        // Assert
        var domainException = Assert.IsType<DomainException>(excecao);
        Assert.Equal("CPF inválido.", domainException.Message);
    }

    [Fact]
    public void AtualizarNome_NomeValido_AlteraNomeERegistraDataAtualizacao()
    {
        // Arrange
        var usuario = TestData.UsuarioValido();
        var atualizacaoAnterior = usuario.DataAtualizacao;
        const string novoNome = "Ryan Vetoriano Silva";

        // Act
        usuario.AtualizarNome(novoNome);

        // Assert
        Assert.Equal(novoNome, usuario.Nome);
        Assert.NotNull(usuario.DataAtualizacao);
        Assert.True(usuario.DataAtualizacao >= atualizacaoAnterior);
        Assert.True(usuario.DataAtualizacao >= usuario.DataCriacao);
    }

    [Fact]
    public void AtualizarEmail_EmailInvalido_MantemEmailAnterior()
    {
        // Arrange
        var usuario = TestData.UsuarioValido(email: "original@futurevet.com");

        // Act
        var excecao = Record.Exception(() => usuario.AtualizarEmail("invalido"));

        // Assert
        Assert.IsType<DomainException>(excecao);
        Assert.Equal("original@futurevet.com", usuario.Email);
    }

    [Fact]
    public void AtualizarTelefone_ValorVazio_AceitaPorqueTelefoneNaoEObrigatorio()
    {
        // Arrange
        var usuario = TestData.UsuarioValido();

        // Act
        usuario.AtualizarTelefone(string.Empty);

        // Assert
        Assert.Equal(string.Empty, usuario.Telefone);
    }

    [Fact]
    public void Deactivate_UsuarioDisponivel_TornaIndisponivel()
    {
        // Arrange
        var usuario = TestData.UsuarioValido();

        // Act
        usuario.Deactivate();

        // Assert
        Assert.False(usuario.Disponivel);
        Assert.NotNull(usuario.DataAtualizacao);
    }

    [Fact]
    public void Activate_UsuarioIndisponivel_TornaDisponivelNovamente()
    {
        // Arrange
        var usuario = TestData.UsuarioValido();
        usuario.Deactivate();

        // Act
        usuario.Activate();

        // Assert
        Assert.True(usuario.Disponivel);
    }
}
