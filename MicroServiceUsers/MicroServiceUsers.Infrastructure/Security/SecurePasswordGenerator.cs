using MicroServiceUsers.Domain.Interfaces;

namespace MicroServiceUsers.Infrastructure.Security
{
    public class SecurePasswordGenerator : IPasswordGenerator
    {
        private static readonly string[] WordsDictionary = 
        {
            "sol", "mar", "luna", "casa", "rio", "flor", "ave", "luz",
            "paz", "vida", "amor", "nube", "voz", "isla", "rey", "sal",
            "oro", "ala", "aire", "onda", "rosa", "nido", "rama", "hoja",
            "vino", "miel", "copa", "mesa", "silla", "puerta", "llave", "reloj"
        };

        private readonly Random _random;

        public SecurePasswordGenerator()
        {
            _random = new Random();
        }

        public string GenerateSecurePassword()
        {
            var word1 = WordsDictionary[_random.Next(WordsDictionary.Length)];
            var num1 = _random.Next(10, 100);
            var word2 = WordsDictionary[_random.Next(WordsDictionary.Length)];
            var num2 = _random.Next(10, 100);
            word1 = CapitalizeFirst(word1);
            word2 = CapitalizeFirst(word2);
            return $"{word1}{num1}{word2}{num2}";
        }

        private string CapitalizeFirst(string word)
        {
            if (string.IsNullOrEmpty(word))
                return word;
            return char.ToUpper(word[0]) + word.Substring(1);
        }
    }
}
