#if UNITY_EDITOR
using UnityEditor;

namespace MUUKIA.Editor
{
    /// <summary>
    /// Acceso directo de menú para invocar la reconstrucción maestra de MUUKIA.
    /// </summary>
    public static class MUUKIAChapter1Builder
    {
        [MenuItem("MUUKIA/Construir Capitulo 1 Completo")]
        public static void BuildChapter1()
        {
            MUUKIAMasterBuilder.BuildEverything();
        }
    }
}
#endif
