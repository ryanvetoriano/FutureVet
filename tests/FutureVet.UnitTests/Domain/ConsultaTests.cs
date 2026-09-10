using FutureVet.Domain.Entities;
using FutureVet.Domain.Exceptions;
using FutureVet.UnitTests.Common;

namespace FutureVet.UnitTests.Domain;

public class ConsultaTests
{
    private static readonly DateTime DataConsulta = new(2026, 3, 15);

    [Fact]
    public void Construtor_DadosValidos_CriaConsultaVinculadaAoPet()
    {
        // Arrange
        var petId = Guid.NewGuid();

        // Act
        var consulta = new Consulta("Rotina", DataConsulta, "14:30", "Clínica Central", petId);

        // Assert
        Assert.NotEqual(Guid.Empty, consulta.Id);
        Assert.Equal("Rotina", consulta.TipoConsulta);
        Assert.Equal(DataConsulta, consulta.Data);
        Assert.Equal("14:30", consulta.Hora);
        Assert.Equal("Clínica Central", consulta.Local);
        Assert.Equal(petId, consulta.PetId);
        Assert.True(consulta.Disponivel);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Construtor_TipoConsultaVazio_LancaDomainException(string tipoInvalido)
    {
        // Arrange
        var acao = () => new Consulta(tipoInvalido, DataConsulta, "14:30", "Clínica Central", Guid.NewGuid());

        // Act
        var excecao = Record.Exception(acao);

        // Assert
        var domainException = Assert.IsType<DomainException>(excecao);
        Assert.Equal("Consulta inválida.", domainException.Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Construtor_HoraVazia_LancaDomainException(string horaInvalida)
    {
        // Arrange
        var acao = () => new Consulta("Rotina", DataConsulta, horaInvalida, "Clínica Central", Guid.NewGuid());

        // Act
        var excecao = Record.Exception(acao);

        // Assert
        var domainException = Assert.IsType<DomainException>(excecao);
        Assert.Equal("Hora inválida.", domainException.Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Construtor_LocalVazio_LancaDomainException(string localInvalido)
    {
        // Arrange
        var acao = () => new Consulta("Rotina", DataConsulta, "14:30", localInvalido, Guid.NewGuid());

        // Act
        var excecao = Record.Exception(acao);

        // Assert
        var domainException = Assert.IsType<DomainException>(excecao);
        Assert.Equal("Local inválido.", domainException.Message);
    }

    [Fact]
    public void DefinirData_DataNoPassado_AceitaPorqueConsultaPodeSerRegistradaRetroativamente()
    {
        // Arrange
        var consulta = TestData.ConsultaValida();
        var dataPassada = new DateTime(2020, 1, 1);

        // Act
        consulta.DefinirData(dataPassada);

        // Assert
        Assert.Equal(dataPassada, consulta.Data);
    }

    [Fact]
    public void AtualizarTipoConsulta_TipoValido_AtualizaTipo()
    {
        // Arrange
        var consulta = TestData.ConsultaValida(tipo: "Rotina");

        // Act
        consulta.AtualizarTipoConsulta("Emergência");

        // Assert
        Assert.Equal("Emergência", consulta.TipoConsulta);
    }

    [Fact]
    public void DefinirHora_HoraInvalida_MantemHoraAnterior()
    {
        // Arrange
        var consulta = TestData.ConsultaValida(hora: "14:30");

        // Act
        var excecao = Record.Exception(() => consulta.DefinirHora("  "));

        // Assert
        Assert.IsType<DomainException>(excecao);
        Assert.Equal("14:30", consulta.Hora);
    }
}
