using FutureVet.Domain.Common;
using FutureVet.Domain.Exceptions;

namespace FutureVet.Domain.Entities;

public class Usuario : BaseEntity
{
    public string Nome { get; private set; }

    public string Email { get; private set; }

    public string Senha { get; private set; }

    public string Cpf { get; private set; }

    public string Telefone { get; private set; }


    public ICollection<Pet> Pets
        { get; private set; }
        = new List<Pet>();


    private Usuario() { }


    public Usuario(
        string nome,
        string email,
        string senha,
        string cpf,
        string telefone)
    {
        AtualizarNome(nome);

        AtualizarEmail(email);

        AlterarSenha(senha);

        DefinirCpf(cpf);

        AtualizarTelefone(telefone);
    }


    public void AtualizarNome(
        string novoNome)
    {
        if (string.IsNullOrWhiteSpace(
            novoNome))
            throw new DomainException(
                "Nome inválido.");

        Nome = novoNome;

        AtualizarData();
    }


    public void AtualizarEmail(
        string novoEmail)
    {
        if (string.IsNullOrWhiteSpace(
            novoEmail)
            || !novoEmail.Contains("@"))
            throw new DomainException(
                "Email inválido.");

        Email = novoEmail;

        AtualizarData();
    }


    public void AlterarSenha(
        string novaSenha)
    {
        if (
            string.IsNullOrWhiteSpace(
            novaSenha)
            || novaSenha.Length < 8
            )
            throw new DomainException(
                "Senha deve possuir no mínimo 8 caracteres.");

        Senha = novaSenha;

        AtualizarData();
    }


    public void DefinirCpf(
        string cpf)
    {
        if (string.IsNullOrWhiteSpace(
            cpf))
            throw new DomainException(
                "CPF inválido.");

        Cpf = cpf;

        AtualizarData();
    }


    public void AtualizarTelefone(
        string telefone)
    {
        Telefone = telefone;

        AtualizarData();
    }
}