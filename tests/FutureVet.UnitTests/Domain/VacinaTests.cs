using FutureVet.Domain.Entities;
using FutureVet.Domain.Exceptions;
using FutureVet.UnitTests.Common;

namespace FutureVet.UnitTests.Domain;

public class VacinaTests
{
    private static readonly DateTime Aplicacao = new(2026, 1, 10);

    [Fact]
    public void Construtor_DadosValidos_CriaVacinaVinculadaAoPet()
    {
        // Arrange
        var petId = Guid.NewGuid();
        var proximaDose = Aplicacao.AddYears(1);

        // Act
        var vacina = new Vacina("V10", Aplicacao, proximaDose, "Clínica Central", petId);

        // Assert
        Assert.NotEqual(Guid.Empty, vacina.Id);
        Assert.Equal("V10", vacina.NomeVacina);
        Assert.Equal(Aplicacao, vacina.DataAplicacao);
        Assert.Equal(proximaDose, vacina.ProximaDose);
        Assert.Equal(petId, vacina.PetId);
        Assert.True(vacina.Disponivel);
    }

    [Fact]
    public void Construtor_ProximaDoseAnteriorAAplicacao_LancaDomainException()
    {
        // Arrange
        var proximaDoseNoPassado = Aplicacao.AddDays(-1);

        var acao = () => new Vacina(
            "V10", Aplicacao, proximaDoseNoPassado, "Clínica Central", Guid.NewGuid());

        // Act
        var excecao = Record.Exception(acao);

        // Assert
        var domainException = Assert.IsType<DomainException>(excecao);
        Assert.Equal("Próxima dose inválida.", domainException.Message);
    }

    [Fact]
    public void Construtor_ProximaDoseNoMesmoDiaDaAplicacao_AceitaPorqueComparacaoEPorData()
    {
        // Arrange
        var mesmoDiaOutraHora = Aplicacao.AddHours(6);

        // Act
        var vacina = new Vacina("V10", Aplicacao, mesmoDiaOutraHora, "Clínica Central", Guid.NewGuid());

        // Assert
        Assert.Equal(mesmoDiaOutraHora, vacina.ProximaDose);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Construtor_NomeVacinaVazio_LancaDomainException(string nomeInvalido)
    {
        // Arrange
        var acao = () => new Vacina(
            nomeInvalido, Aplicacao, Aplicacao.AddYears(1), "Clínica Central", Guid.NewGuid());

        // Act
        var excecao = Record.Exception(acao);

        // Assert
        var domainException = Assert.IsType<DomainException>(excecao);
        Assert.Equal("Vacina inválida.", domainException.Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Construtor_LocalAplicacaoVazio_LancaDomainException(string localInvalido)
    {
        // Arrange
        var acao = () => new Vacina(
            "V10", Aplicacao, Aplicacao.AddYears(1), localInvalido, Guid.NewGuid());

        // Act
        var excecao = Record.Exception(acao);

        // Assert
        var domainException = Assert.IsType<DomainException>(excecao);
        Assert.Equal("Local inválido.", domainException.Message);
    }

    [Fact]
    public void AtualizarProximaDose_DataPosteriorAAplicacao_AtualizaProximaDose()
    {
        // Arrange
        var vacina = TestData.VacinaValida(dataAplicacao: Aplicacao);
        var novaDose = Aplicacao.AddMonths(18);

        // Act
        vacina.AtualizarProximaDose(novaDose);

        // Assert
        Assert.Equal(novaDose, vacina.ProximaDose);
    }

    [Fact]
    public void AtualizarProximaDose_DataAnteriorAAplicacao_MantemDoseAnterior()
    {
        // Arrange
        var doseOriginal = Aplicacao.AddYears(1);
        var vacina = TestData.VacinaValida(dataAplicacao: Aplicacao, proximaDose: doseOriginal);

        // Act
        var excecao = Record.Exception(() => vacina.AtualizarProximaDose(Aplicacao.AddDays(-10)));

        // Assert
        Assert.IsType<DomainException>(excecao);
        Assert.Equal(doseOriginal, vacina.ProximaDose);
    }

    [Fact]
    public void AtualizarLocal_LocalValido_AtualizaLocalAplicacao()
    {
        // Arrange
        var vacina = TestData.VacinaValida();

        // Act
        vacina.AtualizarLocal("Clínica Norte");

        // Assert
        Assert.Equal("Clínica Norte", vacina.LocalAplicacao);
    }
}
