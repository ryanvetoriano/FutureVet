using System.ComponentModel.DataAnnotations;
using FutureVet.Application.Common;
using FutureVet.Application.DTOs.Common;
using FutureVet.Application.DTOs.Consulta;
using FutureVet.Application.DTOs.Pet;
using FutureVet.Application.DTOs.RegistroClinico;
using FutureVet.Application.DTOs.Usuario;
using FutureVet.Application.DTOs.Vacina;
using FutureVet.Domain.Enums;

namespace FutureVet.UnitTests.Application;

/// <summary>
/// Regras de paginação, ordenação e filtros. A validação é a mesma que o model binding do
/// ASP.NET Core executa (DataAnnotations + <see cref="IValidatableObject"/>).
/// </summary>
public class PaginacaoTests
{
    [Fact]
    public void Validar_FiltroPadrao_EhValidoComPagina1ETamanho10()
    {
        // Arrange
        var filtro = new PetFiltro();

        // Act
        var erros = Validar(filtro);

        // Assert
        Assert.Empty(erros);
        Assert.Equal(1, filtro.Pagina);
        Assert.Equal(PaginacaoQuery.TamanhoPaginaPadrao, filtro.TamanhoPagina);
        Assert.Equal(0, filtro.Deslocamento());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(PaginacaoQuery.TamanhoPaginaMaximo + 1)]
    public void Validar_TamanhoPaginaForaDoLimite_RetornaErro(int tamanho)
    {
        // Arrange
        var filtro = new PetFiltro { TamanhoPagina = tamanho };

        // Act
        var erros = Validar(filtro);

        // Assert
        Assert.Contains(erros, e => e.MemberNames.Contains(nameof(PaginacaoQuery.TamanhoPagina)));
    }

    [Fact]
    public void Validar_TamanhoPaginaNoLimiteMaximo_EhValido()
    {
        // Arrange
        var filtro = new PetFiltro { TamanhoPagina = PaginacaoQuery.TamanhoPaginaMaximo };

        // Act
        var erros = Validar(filtro);

        // Assert
        Assert.Empty(erros);
    }

    [Fact]
    public void Validar_PaginaZero_RetornaErro()
    {
        // Arrange
        var filtro = new UsuarioFiltro { Pagina = 0 };

        // Act
        var erros = Validar(filtro);

        // Assert
        Assert.Contains(erros, e => e.MemberNames.Contains(nameof(PaginacaoQuery.Pagina)));
    }

    [Fact]
    public void Validar_CampoDeOrdenacaoNaoPermitido_RetornaErroListandoOsAceitos()
    {
        // Arrange
        var filtro = new UsuarioFiltro { OrdenarPor = "senha" };

        // Act
        var erro = Assert.Single(Validar(filtro));

        // Assert
        Assert.Contains("nome, email, dataCriacao", erro.ErrorMessage);
    }

    [Theory]
    [InlineData("nome")]
    [InlineData("NOME")]
    [InlineData("dataCriacao")]
    [InlineData("datacriacao")]
    public void Validar_CampoDeOrdenacaoPermitidoEmQualquerCaixa_EhValido(string campo)
    {
        // Arrange
        var filtro = new UsuarioFiltro { OrdenarPor = campo };

        // Act
        var erros = Validar(filtro);

        // Assert
        Assert.Empty(erros);
        Assert.Equal(campo.ToLowerInvariant(), filtro.CampoOrdenacao());
    }

    [Fact]
    public void Validar_DirecaoInvalida_RetornaErro()
    {
        // Arrange
        var filtro = new PetFiltro { Direcao = "cima" };

        // Act
        var erros = Validar(filtro);

        // Assert
        Assert.Contains(erros, e => e.MemberNames.Contains(nameof(PaginacaoQuery.Direcao)));
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData("asc", false)]
    [InlineData("DESC", true)]
    public void OrdemDescendente_DirecaoInformada_RespeitaOValor(string? direcao, bool esperado)
    {
        // Arrange
        var filtro = new PetFiltro { Direcao = direcao };

        // Act
        var descendente = filtro.OrdemDescendente();

        // Assert
        Assert.Equal(esperado, descendente);
    }

    [Fact]
    public void OrdemDescendente_ProntuarioSemDirecao_UsaMaisRecentesPrimeiro()
    {
        // Arrange
        var filtro = new RegistroClinicoFiltro();

        // Act
        var descendente = filtro.OrdemDescendente();

        // Assert
        Assert.True(descendente);
    }

    [Fact]
    public void Validar_EspecieForaDoEnum_RetornaErro()
    {
        // Arrange
        var filtro = new PetFiltro { Especie = (EspeciePet)99 };

        // Act
        var erros = Validar(filtro);

        // Assert
        Assert.Contains(erros, e => e.MemberNames.Contains(nameof(PetFiltro.Especie)));
    }

    [Fact]
    public void Validar_IdadeMinimaMaiorQueMaxima_RetornaErro()
    {
        // Arrange
        var filtro = new PetFiltro { IdadeMinima = 10, IdadeMaxima = 2 };

        // Act
        var erro = Assert.Single(Validar(filtro));

        // Assert
        Assert.Equal("IdadeMinima não pode ser posterior a IdadeMaxima.", erro.ErrorMessage);
    }

    [Fact]
    public void Validar_PeriodoInvertido_RetornaErro()
    {
        // Arrange
        var filtro = new ConsultaFiltro
        {
            DataInicio = new DateTime(2026, 12, 1),
            DataFim = new DateTime(2026, 1, 1)
        };

        // Act
        var erros = Validar(filtro);

        // Assert
        Assert.Single(erros);
    }

    [Fact]
    public void Validar_MesmoDiaComoInicioEFim_EhValido()
    {
        // Arrange
        var dia = new DateTime(2026, 3, 15);
        var filtro = new ConsultaFiltro { DataInicio = dia, DataFim = dia };

        // Act
        var erros = Validar(filtro);

        // Assert
        Assert.Empty(erros);
    }

    [Fact]
    public void Validar_VacinaComPeriodoDeAplicacaoInvertido_RetornaErro()
    {
        // Arrange
        var filtro = new VacinaFiltro
        {
            AplicacaoInicio = new DateTime(2026, 6, 1),
            AplicacaoFim = new DateTime(2026, 1, 1)
        };

        // Act
        var erro = Assert.Single(Validar(filtro));

        // Assert
        Assert.Contains(nameof(VacinaFiltro.AplicacaoInicio), erro.MemberNames);
    }

    [Fact]
    public void Validar_VacinaOrdenadaPorProximaDose_EhValido()
    {
        // Arrange
        var filtro = new VacinaFiltro { OrdenarPor = "proximaDose", ProximaDoseAte = new DateTime(2026, 12, 31) };

        // Act
        var erros = Validar(filtro);

        // Assert
        Assert.Empty(erros);
    }

    [Fact]
    public void Validar_ProntuarioComTipoForaDoEnum_RetornaErro()
    {
        // Arrange
        var filtro = new RegistroClinicoFiltro { Tipo = (TipoRegistroClinico)42 };

        // Act
        var erro = Assert.Single(Validar(filtro));

        // Assert
        Assert.Contains(nameof(RegistroClinicoFiltro.Tipo), erro.MemberNames);
    }

    [Fact]
    public void Validar_ProntuarioComFiltrosValidos_EhValido()
    {
        // Arrange
        var filtro = new RegistroClinicoFiltro
        {
            PetId = Guid.NewGuid(),
            Tipo = TipoRegistroClinico.Exame,
            Texto = "hemograma",
            Tag = "sangue",
            DataInicio = new DateTime(2026, 1, 1),
            DataFim = new DateTime(2026, 12, 31),
            OrdenarPor = "titulo",
            Direcao = "asc"
        };

        // Act
        var erros = Validar(filtro);

        // Assert
        Assert.Empty(erros);
        Assert.False(filtro.OrdemDescendente());
    }

    [Fact]
    public void Deslocamento_TerceiraPaginaDe20_Pula40Registros()
    {
        // Arrange
        var filtro = new PetFiltro { Pagina = 3, TamanhoPagina = 20 };

        // Act
        var deslocamento = filtro.Deslocamento();

        // Assert
        Assert.Equal(40, deslocamento);
    }

    [Theory]
    [InlineData(0, 10, 0)]
    [InlineData(10, 10, 1)]
    [InlineData(11, 10, 2)]
    [InlineData(1, 50, 1)]
    public void TotalPaginas_TotalETamanho_ArredondaParaCima(int total, int tamanho, int esperado)
    {
        // Arrange
        var pagina = new PagedResponse<int>([], 1, tamanho, total);

        // Act
        var totalPaginas = pagina.TotalPaginas;

        // Assert
        Assert.Equal(esperado, totalPaginas);
    }

    [Fact]
    public void De_ResultadoDoRepositorio_MapeiaItensEUsaPaginaDoFiltro()
    {
        // Arrange
        var resultado = new PagedResult<int>([1, 2], 7);
        var filtro = new PetFiltro { Pagina = 2, TamanhoPagina = 2 };

        // Act
        var pagina = PagedResponse<string>.De(resultado, filtro, n => $"item {n}");

        // Assert
        Assert.Equal(["item 1", "item 2"], pagina.Itens);
        Assert.Equal(2, pagina.Pagina);
        Assert.Equal(7, pagina.TotalItens);
        Assert.Equal(4, pagina.TotalPaginas);
        Assert.True(pagina.TemPaginaAnterior);
        Assert.True(pagina.TemProximaPagina);
    }

    [Fact]
    public void ComLinks_RecursoSemLinks_RetornaCopiaComLinksSemAlterarOOriginal()
    {
        // Arrange
        var original = new PetResponse(
            Guid.NewGuid(), "Thor", EspeciePet.Cao, null, 3, PortePet.Grande, 30m, Guid.NewGuid());
        var link = new LinkResponse("http://localhost/api/Pet/1", "self", "GET");

        // Act
        var comLinks = (PetResponse)original.ComLinks([link]);

        // Assert
        Assert.Empty(original.Links);
        Assert.Equal(link, Assert.Single(comLinks.Links));
        Assert.Equal(original.Id, comLinks.Id);
    }

    private static List<ValidationResult> Validar(object filtro)
    {
        var erros = new List<ValidationResult>();
        Validator.TryValidateObject(filtro, new ValidationContext(filtro), erros, validateAllProperties: true);
        return erros;
    }
}
