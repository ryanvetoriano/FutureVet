using FutureVet.Application.DTOs.Consulta;
using FutureVet.Application.Interfaces.Repositories;
using FutureVet.Application.Services;
using FutureVet.Domain.Entities;
using FutureVet.Domain.Exceptions;
using FutureVet.UnitTests.Common;
using Moq;

namespace FutureVet.UnitTests.Application;

public class ConsultaServiceTests
{
    private static readonly DateTime DataConsulta = new(2026, 3, 15);

    private readonly Mock<IConsultaRepository> _repositoryMock = new(MockBehavior.Strict);
    private readonly ConsultaService _service;

    public ConsultaServiceTests()
    {
        _service = new ConsultaService(_repositoryMock.Object);
    }

    [Fact]
    public async Task CreateAsync_DadosValidos_PersisteConsultaERetornaResponse()
    {
        // Arrange
        var petId = Guid.NewGuid();
        var request = TestData.CreateConsultaRequestValido(petId);
        Consulta? consultaPersistida = null;

        _repositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Consulta>()))
            .Callback<Consulta>(c => consultaPersistida = c)
            .Returns(Task.CompletedTask);

        // Act
        var response = await _service.CreateAsync(request);

        // Assert
        Assert.NotNull(consultaPersistida);
        Assert.Equal(consultaPersistida.Id, response.Id);
        Assert.Equal(petId, response.PetId);
        Assert.Equal(request.TipoConsulta, response.TipoConsulta);
        Assert.Equal(request.Hora, response.Hora);
        _repositoryMock.Verify(x => x.AddAsync(It.IsAny<Consulta>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_TipoConsultaVazio_LancaDomainExceptionENaoPersiste()
    {
        // Arrange
        var request = new CreateConsultaRequest(
            "  ", DataConsulta, "14:30", "Clínica Central", Guid.NewGuid());

        // Act
        var excecao = await Record.ExceptionAsync(() => _service.CreateAsync(request));

        // Assert
        var domainException = Assert.IsType<DomainException>(excecao);
        Assert.Equal("Consulta inválida.", domainException.Message);
        _repositoryMock.Verify(x => x.AddAsync(It.IsAny<Consulta>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_HoraVazia_LancaDomainExceptionENaoPersiste()
    {
        // Arrange
        var request = new CreateConsultaRequest(
            "Rotina", DataConsulta, "", "Clínica Central", Guid.NewGuid());

        // Act
        var excecao = await Record.ExceptionAsync(() => _service.CreateAsync(request));

        // Assert
        Assert.IsType<DomainException>(excecao);
        _repositoryMock.Verify(x => x.AddAsync(It.IsAny<Consulta>()), Times.Never);
    }

    [Fact]
    public async Task GetByIdAsync_ConsultaExiste_RetornaResponseCorrespondente()
    {
        // Arrange
        var consulta = TestData.ConsultaValida();

        _repositoryMock
            .Setup(x => x.GetByIdAsync(consulta.Id))
            .ReturnsAsync(consulta);

        // Act
        var response = await _service.GetByIdAsync(consulta.Id);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(consulta.Id, response.Id);
        Assert.Equal(consulta.Local, response.Local);
    }

    [Fact]
    public async Task GetByIdAsync_ConsultaNaoEncontrada_RetornaNull()
    {
        // Arrange
        var idInexistente = Guid.NewGuid();

        _repositoryMock
            .Setup(x => x.GetByIdAsync(idInexistente))
            .ReturnsAsync((Consulta?)null);

        // Act
        var response = await _service.GetByIdAsync(idInexistente);

        // Assert
        Assert.Null(response);
    }

    [Fact]
    public async Task GetByDataAsync_DataSemConsultas_RetornaColecaoVazia()
    {
        // Arrange
        _repositoryMock
            .Setup(x => x.GetByDataAsync(DataConsulta))
            .ReturnsAsync([]);

        // Act
        var response = await _service.GetByDataAsync(DataConsulta);

        // Assert
        Assert.Empty(response);
        _repositoryMock.Verify(x => x.GetByDataAsync(DataConsulta), Times.Once);
    }

    [Fact]
    public async Task GetByTipoAsync_TipoComConsultas_MapeiaTodosOsRegistros()
    {
        // Arrange
        var consultas = new List<Consulta>
        {
            TestData.ConsultaValida(tipo: "Rotina", hora: "09:00"),
            TestData.ConsultaValida(tipo: "Rotina", hora: "10:30")
        };

        _repositoryMock
            .Setup(x => x.GetByTipoAsync("Rotina"))
            .ReturnsAsync(consultas);

        // Act
        var response = (await _service.GetByTipoAsync("Rotina")).ToList();

        // Assert
        Assert.Equal(2, response.Count);
        Assert.All(response, r => Assert.Equal("Rotina", r.TipoConsulta));
    }

    [Fact]
    public async Task UpdateAsync_ConsultaExiste_AtualizaDataHoraELocal()
    {
        // Arrange
        var consulta = TestData.ConsultaValida(data: DataConsulta, hora: "14:30", local: "Clínica Central");
        var novaData = new DateTime(2026, 4, 20);
        var request = new UpdateConsultaRequest(novaData, "09:00", "Clínica Norte");

        _repositoryMock
            .Setup(x => x.GetByIdAsync(consulta.Id))
            .ReturnsAsync(consulta);

        _repositoryMock
            .Setup(x => x.UpdateAsync(consulta))
            .Returns(Task.CompletedTask);

        // Act
        await _service.UpdateAsync(consulta.Id, request);

        // Assert
        Assert.Equal(novaData, consulta.Data);
        Assert.Equal("09:00", consulta.Hora);
        Assert.Equal("Clínica Norte", consulta.Local);
        _repositoryMock.Verify(x => x.UpdateAsync(consulta), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ConsultaNaoEncontrada_LancaNotFoundException()
    {
        // Arrange
        var idInexistente = Guid.NewGuid();

        _repositoryMock
            .Setup(x => x.GetByIdAsync(idInexistente))
            .ReturnsAsync((Consulta?)null);

        // Act
        var excecao = await Record.ExceptionAsync(
            () => _service.UpdateAsync(idInexistente, TestData.UpdateConsultaRequestValido()));

        // Assert
        Assert.IsType<NotFoundException>(excecao);
        _repositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Consulta>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_LocalVazio_LancaDomainExceptionENaoPersiste()
    {
        // Arrange
        var consulta = TestData.ConsultaValida();
        var request = new UpdateConsultaRequest(DataConsulta, "09:00", "   ");

        _repositoryMock
            .Setup(x => x.GetByIdAsync(consulta.Id))
            .ReturnsAsync(consulta);

        // Act
        var excecao = await Record.ExceptionAsync(() => _service.UpdateAsync(consulta.Id, request));

        // Assert
        Assert.IsType<DomainException>(excecao);
        _repositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Consulta>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_ConsultaExiste_RemoveConsulta()
    {
        // Arrange
        var consulta = TestData.ConsultaValida();

        _repositoryMock
            .Setup(x => x.GetByIdAsync(consulta.Id))
            .ReturnsAsync(consulta);

        _repositoryMock
            .Setup(x => x.DeleteAsync(consulta))
            .Returns(Task.CompletedTask);

        // Act
        await _service.DeleteAsync(consulta.Id);

        // Assert
        _repositoryMock.Verify(x => x.DeleteAsync(consulta), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ConsultaNaoEncontrada_NaoChamaRepositorioDeExclusao()
    {
        // Arrange
        var idInexistente = Guid.NewGuid();

        _repositoryMock
            .Setup(x => x.GetByIdAsync(idInexistente))
            .ReturnsAsync((Consulta?)null);

        // Act
        await _service.DeleteAsync(idInexistente);

        // Assert
        _repositoryMock.Verify(x => x.DeleteAsync(It.IsAny<Consulta>()), Times.Never);
    }
}
