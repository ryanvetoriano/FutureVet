using FutureVet.Application.Common;
using FutureVet.Application.DTOs.RegistroClinico;
using FutureVet.Application.Interfaces.Repositories;
using FutureVet.Application.Services;
using FutureVet.Domain.Entities;
using FutureVet.Domain.Enums;
using FutureVet.Domain.Exceptions;
using FutureVet.UnitTests.Common;
using Moq;

namespace FutureVet.UnitTests.Application;

public class RegistroClinicoServiceTests
{
    private readonly Mock<IRegistroClinicoRepository> _repositoryMock = new(MockBehavior.Strict);
    private readonly Mock<IPetRepository> _petRepositoryMock = new(MockBehavior.Strict);
    private readonly RegistroClinicoService _service;

    public RegistroClinicoServiceTests()
    {
        _service = new RegistroClinicoService(_repositoryMock.Object, _petRepositoryMock.Object);
    }

    [Fact]
    public async Task CreateAsync_PetExistente_PersisteNoRepositorioERetornaResponse()
    {
        // Arrange
        var petId = Guid.NewGuid();
        var request = TestData.CreateRegistroClinicoRequestValido(petId);
        RegistroClinico? persistido = null;

        _petRepositoryMock
            .Setup(x => x.ExistsAsync(petId))
            .ReturnsAsync(true);

        _repositoryMock
            .Setup(x => x.AddAsync(It.IsAny<RegistroClinico>()))
            .Callback<RegistroClinico>(r => persistido = r)
            .Returns(Task.CompletedTask);

        // Act
        var response = await _service.CreateAsync(request);

        // Assert
        Assert.NotNull(persistido);
        Assert.Equal(persistido.Id, response.Id);
        Assert.Equal(petId, response.PetId);
        Assert.Equal(TipoRegistroClinico.Prescricao, response.Tipo);
        Assert.Equal("Otomax", Assert.Single(response.Medicamentos).Nome);
        Assert.Equal(["ouvido"], response.Tags);
        _repositoryMock.Verify(x => x.AddAsync(It.IsAny<RegistroClinico>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_PetInexistente_LancaNotFoundExceptionENaoPersiste()
    {
        // Arrange
        var petId = Guid.NewGuid();
        var request = TestData.CreateRegistroClinicoRequestValido(petId);

        _petRepositoryMock
            .Setup(x => x.ExistsAsync(petId))
            .ReturnsAsync(false);

        // Act
        var excecao = await Record.ExceptionAsync(() => _service.CreateAsync(request));

        // Assert
        var notFound = Assert.IsType<NotFoundException>(excecao);
        Assert.Contains(petId.ToString(), notFound.Message);
        _repositoryMock.Verify(x => x.AddAsync(It.IsAny<RegistroClinico>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_PrescricaoSemMedicamento_LancaDomainExceptionSemConsultarOPet()
    {
        // Arrange
        var request = TestData.CreateRegistroClinicoRequestValido() with { Medicamentos = [] };

        // Act
        var excecao = await Record.ExceptionAsync(() => _service.CreateAsync(request));

        // Assert
        Assert.IsType<DomainException>(excecao);
        _petRepositoryMock.Verify(x => x.ExistsAsync(It.IsAny<Guid>()), Times.Never);
        _repositoryMock.Verify(x => x.AddAsync(It.IsAny<RegistroClinico>()), Times.Never);
    }

    [Fact]
    public async Task GetByIdAsync_RegistroExiste_RetornaResponseCorrespondente()
    {
        // Arrange
        var registro = TestData.RegistroClinicoValido(titulo: "Hemograma");

        _repositoryMock
            .Setup(x => x.GetByIdAsync(registro.Id))
            .ReturnsAsync(registro);

        // Act
        var response = await _service.GetByIdAsync(registro.Id);

        // Assert
        Assert.NotNull(response);
        Assert.Equal("Hemograma", response.Titulo);
        Assert.Equal(registro.DataCriacao, response.DataCriacao);
    }

    [Fact]
    public async Task GetByIdAsync_RegistroNaoEncontrado_RetornaNull()
    {
        // Arrange
        var idInexistente = Guid.NewGuid();

        _repositoryMock
            .Setup(x => x.GetByIdAsync(idInexistente))
            .ReturnsAsync((RegistroClinico?)null);

        // Act
        var response = await _service.GetByIdAsync(idInexistente);

        // Assert
        Assert.Null(response);
    }

    [Fact]
    public async Task GetByPetAsync_PetComRegistros_MapeiaTodos()
    {
        // Arrange
        var petId = Guid.NewGuid();
        var registros = new List<RegistroClinico>
        {
            TestData.RegistroClinicoValido(petId, titulo: "Primeiro"),
            TestData.RegistroClinicoValido(petId, titulo: "Segundo")
        };

        _repositoryMock
            .Setup(x => x.GetByPetAsync(petId))
            .ReturnsAsync(registros);

        // Act
        var response = (await _service.GetByPetAsync(petId)).ToList();

        // Assert
        Assert.Equal(["Primeiro", "Segundo"], response.Select(r => r.Titulo));
    }

    [Fact]
    public async Task SearchAsync_FiltroRepassado_RetornaPaginaComMetadados()
    {
        // Arrange
        var filtro = new RegistroClinicoFiltro { Tag = "alergia", TamanhoPagina = 1 };
        var registro = TestData.RegistroClinicoValido(tags: ["alergia"]);

        _repositoryMock
            .Setup(x => x.SearchAsync(filtro))
            .ReturnsAsync(new PagedResult<RegistroClinico>([registro], 2));

        // Act
        var pagina = await _service.SearchAsync(filtro);

        // Assert
        Assert.Equal(registro.Id, Assert.Single(pagina.Itens).Id);
        Assert.Equal(2, pagina.TotalPaginas);
        Assert.True(pagina.TemProximaPagina);
    }

    [Fact]
    public async Task UpdateAsync_RegistroExiste_AtualizaEPersiste()
    {
        // Arrange
        var registro = TestData.RegistroClinicoValido();
        var request = TestData.UpdateRegistroClinicoRequestValido();

        _repositoryMock
            .Setup(x => x.GetByIdAsync(registro.Id))
            .ReturnsAsync(registro);

        _repositoryMock
            .Setup(x => x.UpdateAsync(registro))
            .Returns(Task.CompletedTask);

        // Act
        await _service.UpdateAsync(registro.Id, request);

        // Assert
        Assert.Equal(TipoRegistroClinico.Diagnostico, registro.Tipo);
        Assert.Equal("Dermatite alérgica", registro.Titulo);
        Assert.Equal(["dermatologia"], registro.Tags);
        _repositoryMock.Verify(x => x.UpdateAsync(registro), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_RegistroNaoEncontrado_LancaNotFoundException()
    {
        // Arrange
        var idInexistente = Guid.NewGuid();

        _repositoryMock
            .Setup(x => x.GetByIdAsync(idInexistente))
            .ReturnsAsync((RegistroClinico?)null);

        // Act
        var excecao = await Record.ExceptionAsync(
            () => _service.UpdateAsync(idInexistente, TestData.UpdateRegistroClinicoRequestValido()));

        // Assert
        Assert.IsType<NotFoundException>(excecao);
        _repositoryMock.Verify(x => x.UpdateAsync(It.IsAny<RegistroClinico>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_RegraDeDominioViolada_LancaDomainExceptionENaoPersiste()
    {
        // Arrange
        var registro = TestData.RegistroClinicoValido();
        var request = TestData.UpdateRegistroClinicoRequestValido() with
        {
            Tipo = TipoRegistroClinico.Prescricao,
            Medicamentos = null
        };

        _repositoryMock
            .Setup(x => x.GetByIdAsync(registro.Id))
            .ReturnsAsync(registro);

        // Act
        var excecao = await Record.ExceptionAsync(() => _service.UpdateAsync(registro.Id, request));

        // Assert
        Assert.IsType<DomainException>(excecao);
        _repositoryMock.Verify(x => x.UpdateAsync(It.IsAny<RegistroClinico>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_RegistroExiste_RemoveDoRepositorio()
    {
        // Arrange
        var registro = TestData.RegistroClinicoValido();

        _repositoryMock
            .Setup(x => x.GetByIdAsync(registro.Id))
            .ReturnsAsync(registro);

        _repositoryMock
            .Setup(x => x.DeleteAsync(registro))
            .Returns(Task.CompletedTask);

        // Act
        await _service.DeleteAsync(registro.Id);

        // Assert
        _repositoryMock.Verify(x => x.DeleteAsync(registro), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_RegistroNaoEncontrado_NaoChamaRepositorioDeExclusao()
    {
        // Arrange
        var idInexistente = Guid.NewGuid();

        _repositoryMock
            .Setup(x => x.GetByIdAsync(idInexistente))
            .ReturnsAsync((RegistroClinico?)null);

        // Act
        await _service.DeleteAsync(idInexistente);

        // Assert
        _repositoryMock.Verify(x => x.DeleteAsync(It.IsAny<RegistroClinico>()), Times.Never);
    }
}
