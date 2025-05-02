using System.Collections.Generic;

namespace ProyectoServientrega.Utils
{
    public static class Loteador
    {
        public static IEnumerable<List<T>> DividirEnLotes<T>(List<T> lista, int tamanioLote = 100)
        {
            for (int i = 0; i < lista.Count; i += tamanioLote)
            {
                yield return lista.GetRange(i, System.Math.Min(tamanioLote, lista.Count - i));
            }
        }
    }
}
