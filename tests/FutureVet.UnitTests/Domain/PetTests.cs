using FutureVet.Domain.Entities;
using FutureVet.Domain.Enums;
using FutureVet.Domain.Exceptions;
using FutureVet.UnitTests.Common;

namespace FutureVet.UnitTests.Domain;

public class PetTests
{
    [Fact]
    public void Construtor_DadosValidos_CriaPetVinculadoAoUsuario()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();

        // Act
        var pet = new Pet("Thor", EspeciePet.Cao, "Labrador", 3, PortePet.Grande, 32.5m, usuarioId);

        // Assert
        Assert.NotEqual(Guid.Empty, pet.Id);
        Assert.Equal("Thor", pet.NomePet);
        Assert.Equal(EspeciePet.Cao, pet.Especie);
        Assert.Equal(usuarioId, pet.UsuarioId);
        Assert.True(pet.Disponivel);
        Assert.Empty(pet.Vacinas);
        Assert.Empty(pet.Consultas);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Construtor_NomeVazio_LancaDomainException(string nomeInvalido)
    {
        // Arrange
        var acao = () => new Pet(nomeInvalido, EspeciePet.Gato, null, 1, PortePet.Pequeno, 4m, Guid.NewGuid());

        // Act
        var excecao = Record.Exception(acao);

        // Assert
        var domainException = Assert.IsType<DomainException>(excecao);
        Assert.Equal("Nome inválido.", domainException.Message);
    }

    [Fact]
    public void Construtor_RacaNula_CriaPetPorqueRacaEOpcional()
    {
        // Arrange & Act
        var pet = new Pet("Mia", EspeciePet.Gato, null, 2, PortePet.Pequeno, 4.2m, Guid.NewGuid());

        // Assert
        Assert.Null(pet.Raca);
    }

    [Fact]
    public void DefinirIdade_IdadeNegativa_LancaDomainException()
    {
        // Arrange
        var pet = TestData.PetValido();

        // Act
        var excecao = Record.Exception(() => pet.DefinirIdade(-1));

        // Assert
        var domainException = Assert.IsType<DomainException>(excecao);
        Assert.Equal("Idade inválida.", domainException.Message);
    }

    [Fact]
    public void DefinirIdade_IdadeZero_AceitaPorqueFilhoteRecemNascidoEValido()
    {
        // Arrange
        var pet = TestData.PetValido();

        // Act
        pet.DefinirIdade(0);

        // Assert
        Assert.Equal(0, pet.Idade);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void AtualizarPeso_PesoMenorOuIgualAZero_LancaDomainException(int pesoInvalido)
    {
        // Arrange
        var pet = TestData.PetValido(peso: 20m);

        // Act
        var excecao = Record.Exception(() => pet.AtualizarPeso(pesoInvalido));

        // Assert
        var domainException = Assert.IsType<DomainException>(excecao);
        Assert.Equal("Peso inválido.", domainException.Message);
        Assert.Equal(20m, pet.Peso);
    }

    [Fact]
    public void AtualizarPeso_PesoPositivo_AtualizaPeso()
    {
        // Arrange
        var pet = TestData.PetValido(peso: 20m);

        // Act
        pet.AtualizarPeso(0.1m);

        // Assert
        Assert.Equal(0.1m, pet.Peso);
    }

    [Fact]
    public void DefinirEspecie_NovaEspecie_AtualizaEspecie()
    {
        // Arrange
        var pet = TestData.PetValido(especie: EspeciePet.Cao);

        // Act
        pet.DefinirEspecie(EspeciePet.Coelho);

        // Assert
        Assert.Equal(EspeciePet.Coelho, pet.Especie);
    }

    [Fact]
    public void AtualizarRaca_ValorNulo_LimpaRaca()
    {
        // Arrange
        var pet = TestData.PetValido(raca: "Labrador");

        // Act
        pet.AtualizarRaca(null);

        // Assert
        Assert.Null(pet.Raca);
    }
}
