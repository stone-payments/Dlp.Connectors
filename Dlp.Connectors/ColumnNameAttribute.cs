
namespace Dlp.Connectors
{
    /// <summary>
    /// Classe de atributo usada para identificar o nome da coluna que corresponde a propriedade.
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Property)]
    public sealed class ColumnNameAttribute : System.Attribute
    {
        private string name;

        /// <summary>
        /// Construtor da classe.
        /// </summary>
        /// <param name="name">Nome da coluna que corresponde à propriedade.</param>
        public ColumnNameAttribute(string name)
        {
            this.name = name;
        }

        /// <summary>
        /// Retorna o nome da coluna que corresponde à propriedade..
        /// </summary>
        /// <returns></returns>
        public string GetName()
        {
            return name;
        }
    }
}
