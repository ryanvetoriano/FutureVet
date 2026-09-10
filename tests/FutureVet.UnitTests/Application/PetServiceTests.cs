using FutureVet.Application.DTOs.Pet;
using FutureVet.Application.Interfaces.Repositories;
using FutureVet.Application.Services;
using FutureVet.Domain.Entities;
using FutureVet.Domain.Enums;
using FutureVet.Domain.Exceptions;
using FutureVet.UnitTests.Common;
using Moq;

namespace FutureVet.UnitTests.Application;

public class PetServiceTests
{
    private readonly Mock<IPetRepository> _repositoryMock = new(MockBehavior.Strict);
    private readonly PetService _service;

    public PetServiceTests()
    {
        _service = new PetService(_repositoryMock.Object);
    }

    [Fact]
    public async Task CreateAsync_DadosValidos_PersistePetERetornaResponse()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var request = TestData.CreatePetRequestValido(usuarioId);
        Pet? petPersistido = null;

        _repositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Pet>()))
            .Callback<Pet>(p => petPersistido = p)
            .Returns(Task.CompletedTask);

        // Act
        var response = await _service.CreateAsync(request);

        // Assert
        Assert.NotNull(petPersistido);
        Assert.Equal(usuarioId, petPersistido.UsuarioId);
        Assert.Equal(petPersistido.Id, response.Id);
        Assert.Equal(request.NomePet, response.NomePet);
        Assert.Equal(request.Peso, response.Peso);
        _repositoryMock.Verify(x => x.AddAsync(It.IsAny<Pet>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_PesoZerado_LancaDomainExceptionENaoPersiste()
    {
        // Arrange
        var request = new CreatePetRequest(
            "Thor", EspeciePet.Cao, "Labrador", 3, PortePet.Grande, 0m, Guid.NewGuid());

        // Act
        var excecao = await Record.ExceptionAsync(() => _service.CreateAsync(request));

        // Assert
        var domainException = Assert.IsType<DomainException>(excecao);
        Assert.Equal("Peso inválido.", domainException.Message);
        _repositoryMock.Verify(x => x.AddAsync(It.IsAny<Pet>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_IdadeNegativa_LancaDomainExceptionENaoPersiste()
    {
        // Arrange
        var request = new CreatePetRequest(
            "Thor", EspeciePet.Cao, "Labrador", -1, PortePet.Grande, 30m, Guid.NewGuid());

        // Act
        var excecao = await Record.ExceptionAsync(() => _service.CreateAsync(request));

        // Assert
        Assert.IsType<DomainException>(excecao);
        _repositoryMock.Verify(x => x.AddAsync(It.IsAny<Pet>()), Times.Never);
    }

    [Fact]
    public async Task GetByIdAsync_PetExiste_RetornaResponseCorrespondente()
    {
        // Arrange
        var pet = TestData.PetValido();

        _repositoryMock
            .Setup(x => x.GetByIdAsync(pet.Id))
            .ReturnsAsync(pet);

        // Act
        var response = await _service.GetByIdAsync(pet.Id);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(pet.Id, response.Id);
        Assert.Equal(pet.NomePet, response.NomePet);
        Assert.Equal(pet.Especie, response.Especie);
    }

    [Fact]
    public async Task GetByIdAsync_PetNaoEncontrado_RetornaNull()
    {
        // Arrange
        var idInexistente = Guid.NewGuid();

        _repositoryMock
            .Setup(x => x.GetByIdAsync(idInexistente))
            .ReturnsAsync((Pet?)null);

        // Act
        var response = await _service.GetByIdAsync(idInexistente);

        // Assert
        Assert.Null(response);
    }

    [Fact]
    public async Task GetByEspecieAsync_EspecieSemPets_RetornaColecaoVazia()
    {
        // Arrange
        _repositoryMock
            .Setup(x => x.GetByEspecieAsync(EspeciePet.Coelho))
            .ReturnsAsync([]);

        // Act
        var response = await _service.GetByEspecieAsync(EspeciePet.Coelho);

        // Assert
        Assert.Empty(response);
        _repositoryMock.Verify(x => x.GetByEspecieAsync(EspeciePet.Coelho), Times.Once);
    }

    [Fact]
    public async Task GetByUsuarioAsync_UsuarioComPets_RetornaApenasOsPetsDoUsuario()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var pets = new List<Pet>
        {
            TestData.PetValido(nome: "Thor", usuarioId: usuarioId),
            TestData.PetValido(nome: "Mia", usuarioId: usuarioId)
        };

        _repositoryMock
            .Setup(x => x.GetByUsuarioAsync(usuarioId))
            .ReturnsAsync(pets);

        // Act
        var response = (await _service.GetByUsuarioAsync(usuarioId)).ToList();

        // Assert
        Assert.Equal(2, response.Count);
        Assert.Contains(response, p => p.NomePet == "Thor");
        Assert.Contains(response, p => p.NomePet == "Mia");
    }

    [Fact]
    public async Task UpdateAsync_PetExiste_AtualizaDadosEPersiste()
    {
        // Arrange
        var pet = TestData.PetValido(nome: "Thor", idade: 3, peso: 30m);
        var request = new UpdatePetRequest("Thor Atualizado", "Golden", 4, PortePet.Medio, 28.5m);

        _repositoryMock
            .Setup(x => x.GetByIdAsync(pet.Id))
            .ReturnsAsync(pet);

        _repositoryMock
            .Setup(x => x.UpdateAsync(pet))
            .Returns(Task.CompletedTask);

        // Act
        await _service.UpdateAsync(pet.Id, request);

        // Assert
        Assert.Equal("Thor Atualizado", pet.NomePet);
        Assert.Equal("Golden", pet.Raca);
        Assert.Equal(4, pet.Idade);
        Assert.Equal(PortePet.Medio, pet.Tamanho);
        Assert.Equal(28.5m, pet.Peso);
        _repositoryMock.Verify(x => x.UpdateAsync(pet), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_PetNaoEncontrado_LancaNotFoundException()
    {
        // Arrange
        var idInexistente = Guid.NewGuid();

        _repositoryMock
            .Setup(x => x.GetByIdAsync(idInexistente))
            .ReturnsAsync((Pet?)null);

        // Act
        var excecao = await Record.ExceptionAsync(
            () => _service.UpdateAsync(idInexistente, TestData.UpdatePetRequestValido()));

        // Assert
        Assert.IsType<NotFoundException>(excecao);
        _repositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Pet>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_PesoInvalido_LancaDomainExceptionENaoPersiste()
    {
        // Arrange
        var pet = TestData.PetValido(peso: 30m);
        var request = new UpdatePetRequest("Thor", "Labrador", 3, PortePet.Grande, -2m);

        _repositoryMock
            .Setup(x => x.GetByIdAsync(pet.Id))
            .ReturnsAsync(pet);

        // Act
        var excecao = await Record.ExceptionAsync(() => _service.UpdateAsync(pet.Id, request));

        // Assert
        Assert.IsType<DomainException>(excecao);
        _repositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Pet>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_PetExiste_RemovePet()
    {
        // Arrange
        var pet = TestData.PetValido();

        _repositoryMock
            .Setup(x => x.GetByIdAsync(pet.Id))
            .ReturnsAsync(pet);

        _repositoryMock
            .Setup(x => x.DeleteAsync(pet))
            .Returns(Task.CompletedTask);

        // Act
        await _service.DeleteAsync(pet.Id);

        // Assert
        _repositoryMock.Verify(x => x.DeleteAsync(pet), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_PetNaoEncontrado_NaoChamaRepositorioDeExclusao()
    {
        // Arrange
        var idInexistente = Guid.NewGuid();

        _repositoryMock
            .Setup(x => x.GetByIdAsync(idInexistente))
            .ReturnsAsync((Pet?)null);

        // Act
        await _service.DeleteAsync(idInexistente);

        // Assert
        _repositoryMock.Verify(x => x.DeleteAsync(It.IsAny<Pet>()), Times.Never);
    }
}
