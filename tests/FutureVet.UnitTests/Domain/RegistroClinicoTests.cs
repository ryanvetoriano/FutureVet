using FutureVet.Domain.Entities;
using FutureVet.Domain.Enums;
using FutureVet.Domain.Exceptions;
using FutureVet.UnitTests.Common;

namespace FutureVet.UnitTests.Domain;

public class RegistroClinicoTests
{
    [Fact]
    public void Construtor_DadosValidos_CriaRegistroComTodosOsCampos()
    {
        // Arrange
        var petId = Guid.NewGuid();
        var data = new DateTime(2026, 3, 15, 10, 0, 0);

        // Act
        var registro = new RegistroClinico(
            petId,
            TipoRegistroClinico.Exame,
            "Hemograma",
            "Valores dentro da referência.",
            "Dra. Ana Souza",
            data,
            ["apatia"],
            null,
            ["sangue"]);

        // Assert
        Assert.NotEqual(Guid.Empty, registro.Id);
        Assert.Equal(petId, registro.PetId);
        Assert.Equal(TipoRegistroClinico.Exame, registro.Tipo);
        Assert.Equal("Hemograma", registro.Titulo);
        Assert.Equal(data, registro.DataRegistro);
        Assert.Equal(["apatia"], registro.Sintomas);
        Assert.Empty(registro.Medicamentos);
        Assert.Equal(["sangue"], registro.Tags);
        Assert.True(registro.Disponivel);
    }

    [Fact]
    public void Construtor_PetVazio_LancaDomainException()
    {
        // Arrange
        var petVazio = Guid.Empty;

        // Act
        var excecao = Record.Exception(() => TestData.RegistroClinicoValido(petId: petVazio));

        // Assert
        var domainException = Assert.IsType<DomainException>(excecao);
        Assert.Equal("Pet inválido.", domainException.Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Construtor_TituloVazio_LancaDomainException(string titulo)
    {
        // Arrange & Act
        var excecao = Record.Exception(() => TestData.RegistroClinicoValido(titulo: titulo));

        // Assert
        var domainException = Assert.IsType<DomainException>(excecao);
        Assert.Equal("Título inválido.", domainException.Message);
    }

    [Fact]
    public void Construtor_DescricaoVazia_LancaDomainException()
    {
        // Arrange & Act
        var excecao = Record.Exception(() => TestData.RegistroClinicoValido(descricao: " "));

        // Assert
        var domainException = Assert.IsType<DomainException>(excecao);
        Assert.Equal("Descrição inválida.", domainException.Message);
    }

    [Fact]
    public void Construtor_TipoForaDoEnum_LancaDomainException()
    {
        // Arrange
        var tipoInexistente = (TipoRegistroClinico)42;

        // Act
        var excecao = Record.Exception(() => TestData.RegistroClinicoValido(tipo: tipoInexistente));

        // Assert
        var domainException = Assert.IsType<DomainException>(excecao);
        Assert.Equal("Tipo de registro clínico inválido.", domainException.Message);
    }

    [Fact]
    public void Construtor_DataNoFuturo_LancaDomainException()
    {
        // Arrange
        var dataFutura = DateTime.UtcNow.AddDays(10);

        // Act
        var excecao = Record.Exception(() => TestData.RegistroClinicoValido(dataRegistro: dataFutura));

        // Assert
        var domainException = Assert.IsType<DomainException>(excecao);
        Assert.Equal("A data do registro não pode estar no futuro.", domainException.Message);
    }

    [Fact]
    public void Construtor_PrescricaoSemMedicamento_LancaDomainException()
    {
        // Arrange & Act
        var excecao = Record.Exception(() =>
            TestData.RegistroClinicoValido(tipo: TipoRegistroClinico.Prescricao, medicamentos: []));

        // Assert
        var domainException = Assert.IsType<DomainException>(excecao);
        Assert.Equal("Uma prescrição deve informar ao menos um medicamento.", domainException.Message);
    }

    [Fact]
    public void Construtor_PrescricaoComMedicamento_CriaRegistro()
    {
        // Arrange
        var medicamento = TestData.MedicamentoValido();

        // Act
        var registro = TestData.RegistroClinicoValido(
            tipo: TipoRegistroClinico.Prescricao,
            medicamentos: [medicamento]);

        // Assert
        Assert.Same(medicamento, Assert.Single(registro.Medicamentos));
    }

    [Fact]
    public void Construtor_TagsComCaixaMistaEDuplicadas_NormalizaEmMinusculasSemRepeticao()
    {
        // Arrange
        string[] tags = ["Alergia", "ALERGIA", " pele ", ""];

        // Act
        var registro = TestData.RegistroClinicoValido(tags: tags);

        // Assert
        Assert.Equal(["alergia", "pele"], registro.Tags);
    }

    [Fact]
    public void Construtor_SintomasComEspacosEEmBranco_DescartaVaziosEAparaOsDemais()
    {
        // Arrange
        string[] sintomas = [" Coceira ", "", "  ", "Vômito"];

        // Act
        var registro = TestData.RegistroClinicoValido(sintomas: sintomas);

        // Assert
        Assert.Equal(["Coceira", "Vômito"], registro.Sintomas);
    }

    [Fact]
    public void Construtor_DataSemFuso_GravaComoUtcSemAlterarOHorario()
    {
        // Arrange
        var dataSemFuso = new DateTime(2026, 3, 15, 10, 0, 0, DateTimeKind.Unspecified);

        // Act
        var registro = TestData.RegistroClinicoValido(dataRegistro: dataSemFuso);

        // Assert
        Assert.Equal(DateTimeKind.Utc, registro.DataRegistro.Kind);
        Assert.Equal(10, registro.DataRegistro.Hour);
    }

    [Fact]
    public void Atualizar_DadosValidos_SubstituiOConteudoEMantemPetEId()
    {
        // Arrange
        var registro = TestData.RegistroClinicoValido();
        var idOriginal = registro.Id;
        var petOriginal = registro.PetId;

        // Act
        registro.Atualizar(
            TipoRegistroClinico.Diagnostico,
            "Dermatite",
            "Confirmada.",
            null,
            new DateTime(2026, 4, 1),
            null,
            null,
            ["pele"]);

        // Assert
        Assert.Equal(idOriginal, registro.Id);
        Assert.Equal(petOriginal, registro.PetId);
        Assert.Equal(TipoRegistroClinico.Diagnostico, registro.Tipo);
        Assert.Equal("Dermatite", registro.Titulo);
        Assert.Null(registro.Veterinario);
        Assert.Empty(registro.Sintomas);
        Assert.NotNull(registro.DataAtualizacao);
    }

    [Fact]
    public void Atualizar_DadosInvalidos_NaoAlteraNenhumCampo()
    {
        // Arrange
        var registro = TestData.RegistroClinicoValido(titulo: "Original");

        // Act
        var excecao = Record.Exception(() => registro.Atualizar(
            TipoRegistroClinico.Prescricao,
            "Novo título",
            "Nova descrição",
            null,
            new DateTime(2026, 4, 1),
            null,
            medicamentos: null,
            null));

        // Assert
        Assert.IsType<DomainException>(excecao);
        Assert.Equal("Original", registro.Titulo);
        Assert.Equal(TipoRegistroClinico.Observacao, registro.Tipo);
    }
}
