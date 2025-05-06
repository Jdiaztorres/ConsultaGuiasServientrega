using System.Collections.Generic;

namespace ProyectoServientrega.Utils
{
    /// <summary>
    /// Clase utilitaria para dividir listas en lotes más pequeños.
    /// </summary>
    public static class Loteador
    {
        public static IEnumerable<List<T>> DividirEnLotes<T>(List<T> lista, int tamanioLote = 90)
        {
            for (int i = 0; i < lista.Count; i += tamanioLote)
            {
                yield return lista.GetRange(i, System.Math.Min(tamanioLote, lista.Count - i));
            }
        }
    }
}
