using FutureVet.Domain.Exceptions;
using FutureVet.Domain.ValueObjects;

namespace FutureVet.UnitTests.Domain;

public class MedicamentoTests
{
    [Fact]
    public void Construtor_DadosValidos_AparaEspacosDosCampos()
    {
        // Arrange & Act
        var medicamento = new Medicamento(" Amoxicilina ", " 250 mg ", " 12/12h ", 7);

        // Assert
        Assert.Equal("Amoxicilina", medicamento.Nome);
        Assert.Equal("250 mg", medicamento.Dosagem);
        Assert.Equal("12/12h", medicamento.Frequencia);
        Assert.Equal(7, medicamento.DuracaoDias);
    }

    [Fact]
    public void Construtor_FrequenciaEmBranco_GuardaNull()
    {
        // Arrange & Act
        var medicamento = new Medicamento("Dipirona", "1 gota/kg", "  ");

        // Assert
        Assert.Null(medicamento.Frequencia);
        Assert.Null(medicamento.DuracaoDias);
    }

    [Fact]
    public void Construtor_NomeVazio_LancaDomainException()
    {
        // Arrange & Act
        var excecao = Record.Exception(() => new Medicamento("", "250 mg"));

        // Assert
        var domainException = Assert.IsType<DomainException>(excecao);
        Assert.Equal("Nome do medicamento inválido.", domainException.Message);
    }

    [Fact]
    public void Construtor_DosagemVazia_LancaDomainException()
    {
        // Arrange & Act
        var excecao = Record.Exception(() => new Medicamento("Amoxicilina", " "));

        // Assert
        var domainException = Assert.IsType<DomainException>(excecao);
        Assert.Equal("Dosagem do medicamento inválida.", domainException.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-3)]
    public void Construtor_DuracaoNaoPositiva_LancaDomainException(int duracao)
    {
        // Arrange & Act
        var excecao = Record.Exception(() => new Medicamento("Amoxicilina", "250 mg", null, duracao));

        // Assert
        var domainException = Assert.IsType<DomainException>(excecao);
        Assert.Equal("Duração do tratamento inválida.", domainException.Message);
    }
}
