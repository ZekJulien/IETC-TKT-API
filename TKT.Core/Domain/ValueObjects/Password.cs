using System.ComponentModel.DataAnnotations;

namespace TKT.Core.Domain.ValueObjects;

public sealed class Password                                                                                                                                                              
{                                                                                                                                                                                         
    public string Value { get; }                                                                                                                                                          
    private Password(string v) => Value = v;                                                                                                                                              
    public static Password Create(string raw)                                                                                                                                             
    {                                                                                                                                                                                     
        if (raw.Length < 8 || !raw.Any(char.IsUpper) || !raw.Any(char.IsDigit))                                                                                                           
            throw new ValidationException("MDP : 8 caractères, 1 majuscule, 1 chiffre minimum.");                                                                                         
        return new Password(raw);                                                                                                                                                         
    }                                                                                                                                                                                     
}    