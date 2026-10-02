using System.Text.Json;
using UnfollowersInstagram.DTOs;

namespace UnfollowersInstagram.Services
{
    public class UnfollowersService
    {
        public async Task<List<string>> ScanAsync(IFormFile seguidoresFile, IFormFile seguidosFile)
        {
            // Leer JSON de seguidores
            using var seguidoresStream = seguidoresFile.OpenReadStream();
            using var seguidoresReader = new StreamReader(seguidoresStream);

            string seguidoresJson = await seguidoresReader.ReadToEndAsync();


            // Leer JSON de seguidos
            using var seguidosStream = seguidosFile.OpenReadStream();
            using var seguidosReader = new StreamReader(seguidosStream);

            string seguidosJson = await seguidosReader.ReadToEndAsync();


            // Deserializar seguidores
            List<FollowerDto>? seguidores = JsonSerializer.Deserialize<List<FollowerDto>>(seguidoresJson);

            // Deserializar seguidos
            FollowingResponseDto? seguidos = JsonSerializer.Deserialize<FollowingResponseDto>(seguidosJson);


            if (seguidores == null || seguidos == null)
            {
                throw new JsonException("No se pudo deserializar uno de los archivos.");
            }


            // Obtener usernames de seguidores
            List<string> listaSeguidores = seguidores
                .Where(x => x.StringListData is { Count: > 0 })
                .Select(x => x.StringListData![0].Value)
                .OfType<string>()
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .ToList();


            // Obtener usernames de seguidos
            if (seguidos.RelationshipsFollowing == null)
            {
                throw new JsonException("El archivo de seguidos no contiene la lista relationships_following.");
            }

            List<string> listaSeguidos = seguidos.RelationshipsFollowing
                .Select(x => x?.Title)
                .OfType<string>()
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .ToList();


            // Buscar personas que sigo y no me siguen
            List<string> personasUnfollow = listaSeguidos
                .Except(listaSeguidores)
                .ToList();


            return personasUnfollow;
        }
    }
}