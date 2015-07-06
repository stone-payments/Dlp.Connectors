using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dlp.Connectors {

    public interface ICacheConnector {

        /// <summary>
        /// Connects to the cache service.
        /// </summary>
        bool Connect();

        /// <summary>
        /// Checks if the cache service is connected.
        /// </summary>
        /// <returns>Returns true if we are connected to the cache service.</returns>
        bool IsConnected();

        /// <summary>
        /// Disconnects from the cache service.
        /// </summary>
        void Disconnect();

        /// <summary>
        /// Adiciona um atualiza um item existente no cache.
        /// </summary>
        /// <typeparam name="T">Tipo do item a ser inserido/atualziado.</typeparam>
        /// <param name="key">Nome único no cache.</param>
        /// <param name="item">Objeto a ser inserido/atualizado.</param>
        /// <param name="itemExpirationInMinutes">Quanto tempo o item deverá permanecer no cache.</param>
        /// <returns>Retorna true caso a operação seja realizada com sucesso.</returns>
        bool Set<T>(string key, T item, int itemExpirationInMinutes) where T : new();

        /// <summary>
        /// Obtém um item do cache a partir do nome.
        /// </summary>
        /// <typeparam name="T">Tipo do objeto a ser retornado.</typeparam>
        /// <param name="key">Nome do objeto no cache.</param>
        /// <returns>Retorna um objeto do tipo T, encontrado no cache, ou default(T), caso o item não exista.</returns>
        T GetByKey<T>(string key) where T : class, new();
    }
}
