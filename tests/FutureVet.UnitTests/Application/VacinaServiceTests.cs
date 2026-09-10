using FutureVet.Application.DTOs.Vacina;
using FutureVet.Application.Interfaces.Repositories;
using FutureVet.Application.Services;
using FutureVet.Domain.Entities;
using FutureVet.Domain.Exceptions;
using FutureVet.UnitTests.Common;
using Moq;

namespace FutureVet.UnitTests.Application;

public class VacinaServiceTests
{
    private static readonly DateTime Aplicacao = new(2026, 1, 10);

    private readonly Mock<IVacinaRepository> _repositoryMock = new(MockBehavior.Strict);
    private readonly VacinaService _service;

    public VacinaServiceTests()
    {
        _service = new VacinaService(_repositoryMock.Object);
    }

    [Fact]
    public async Task CreateAsync_DadosValidos_PersisteVacinaERetornaResponse()
    {
        // Arrange
        var petId = Guid.NewGuid();
        var request = TestData.CreateVacinaRequestValido(petId);
        Vacina? vacinaPersistida = null;

        _repositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Vacina>()))
            .Callback<Vacina>(v => vacinaPersistida = v)
            .Returns(Task.CompletedTask);

        // Act
        var response = await _service.CreateAsync(request);

        // Assert
        Assert.NotNull(vacinaPersistida);
        Assert.Equal(vacinaPersistida.Id, response.Id);
        Assert.Equal(petId, response.PetId);
        Assert.Equal(request.NomeVacina, response.NomeVacina);
        _repositoryMock.Verify(x => x.AddAsync(It.IsAny<Vacina>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ProximaDoseAnteriorAAplicacao_LancaDomainExceptionENaoPersiste()
    {
        // Arrange
        var request = new CreateVacinaRequest(
            "V10", Aplicacao, Aplicacao.AddDays(-5), "Clínica Central", Guid.NewGuid());

        // Act
        var excecao = await Record.ExceptionAsync(() => _service.CreateAsync(request));

        // Assert
        var domainException = Assert.IsType<DomainException>(excecao);
        Assert.Equal("Próxima dose inválida.", domainException.Message);
        _repositoryMock.Verify(x => x.AddAsync(It.IsAny<Vacina>()), Times.Never);
    }

    [Fact]
    public async Task GetByIdAsync_VacinaExiste_RetornaResponseCorrespondente()
    {
        // Arrange
        var vacina = TestData.VacinaValida();

        _repositoryMock
            .Setup(x => x.GetByIdAsync(vacina.Id))
            .ReturnsAsync(vacina);

        // Act
        var response = await _service.GetByIdAsync(vacina.Id);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(vacina.Id, response.Id);
        Assert.Equal(vacina.NomeVacina, response.NomeVacina);
    }

    [Fact]
    public async Task GetByIdAsync_VacinaNaoEncontrada_RetornaNull()
    {
        // Arrange
        var idInexistente = Guid.NewGuid();

        _repositoryMock
            .Setup(x => x.GetByIdAsync(idInexistente))
            .ReturnsAsync((Vacina?)null);

        // Act
        var response = await _service.GetByIdAsync(idInexistente);

        // Assert
        Assert.Null(response);
    }

    [Fact]
    public async Task GetByPetAsync_PetSemVacinas_RetornaColecaoVazia()
    {
        // Arrange
        var petId = Guid.NewGuid();

        _repositoryMock
            .Setup(x => x.GetByPetAsync(petId))
            .ReturnsAsync([]);

        // Act
        var response = await _service.GetByPetAsync(petId);

        // Assert
        Assert.Empty(response);
    }

    [Fact]
    public async Task GetByProximaDoseAsync_DataLimiteInformada_RepassaFiltroAoRepositorio()
    {
        // Arrange
        var dataLimite = new DateTime(2027, 1, 31);
        var vacinas = new List<Vacina> { TestData.VacinaValida(nome: "Antirrábica") };

        _repositoryMock
            .Setup(x => x.GetByProximaDoseAsync(dataLimite))
            .ReturnsAsync(vacinas);

        // Act
        var response = (await _service.GetByProximaDoseAsync(dataLimite)).ToList();

        // Assert
        Assert.Single(response);
        Assert.Equal("Antirrábica", response[0].NomeVacina);
        _repositoryMock.Verify(x => x.GetByProximaDoseAsync(dataLimite), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_VacinaExiste_AtualizaProximaDoseELocal()
    {
        // Arrange
        var vacina = TestData.VacinaValida(dataAplicacao: Aplicacao, local: "Clínica Central");
        var novaDose = Aplicacao.AddMonths(18);
        var request = new UpdateVacinaRequest(novaDose, "Clínica Norte");

        _repositoryMock
            .Setup(x => x.GetByIdAsync(vacina.Id))
            .ReturnsAsync(vacina);

        _repositoryMock
            .Setup(x => x.UpdateAsync(vacina))
            .Returns(Task.CompletedTask);

        // Act
        await _service.UpdateAsync(vacina.Id, request);

        // Assert
        Assert.Equal(novaDose, vacina.ProximaDose);
        Assert.Equal("Clínica Norte", vacina.LocalAplicacao);
        _repositoryMock.Verify(x => x.UpdateAsync(vacina), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_VacinaNaoEncontrada_LancaNotFoundException()
    {
        // Arrange
        var idInexistente = Guid.NewGuid();

        _repositoryMock
            .Setup(x => x.GetByIdAsync(idInexistente))
            .ReturnsAsync((Vacina?)null);

        // Act
        var excecao = await Record.ExceptionAsync(
            () => _service.UpdateAsync(idInexistente, TestData.UpdateVacinaRequestValido()));

        // Assert
        Assert.IsType<NotFoundException>(excecao);
        _repositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Vacina>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_ProximaDoseAnteriorAAplicacao_LancaDomainExceptionENaoPersiste()
    {
        // Arrange
        var vacina = TestData.VacinaValida(dataAplicacao: Aplicacao);
        var request = new UpdateVacinaRequest(Aplicacao.AddDays(-1), "Clínica Norte");

        _repositoryMock
            .Setup(x => x.GetByIdAsync(vacina.Id))
            .ReturnsAsync(vacina);

        // Act
        var excecao = await Record.ExceptionAsync(() => _service.UpdateAsync(vacina.Id, request));

        // Assert
        Assert.IsType<DomainException>(excecao);
        _repositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Vacina>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_VacinaExiste_RemoveVacina()
    {
        // Arrange
        var vacina = TestData.VacinaValida();

        _repositoryMock
            .Setup(x => x.GetByIdAsync(vacina.Id))
            .ReturnsAsync(vacina);

        _repositoryMock
            .Setup(x => x.DeleteAsync(vacina))
            .Returns(Task.CompletedTask);

        // Act
        await _service.DeleteAsync(vacina.Id);

        // Assert
        _repositoryMock.Verify(x => x.DeleteAsync(vacina), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_VacinaNaoEncontrada_NaoChamaRepositorioDeExclusao()
    {
        // Arrange
        var idInexistente = Guid.NewGuid();

        _repositoryMock
            .Setup(x => x.GetByIdAsync(idInexistente))
            .ReturnsAsync((Vacina?)null);

        // Act
        await _service.DeleteAsync(idInexistente);

        // Assert
        _repositoryMock.Verify(x => x.DeleteAsync(It.IsAny<Vacina>()), Times.Never);
    }
}
