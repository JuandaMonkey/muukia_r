using UnityEngine;

/// <summary>
/// Gestiona el resaltado visual (outline) de objetos interactuables mediante la adición de un pase de contorno.
/// Optimizado para evitar la instanciación de materiales en tiempo de ejecución (CERO Garbage Collection en Update).
/// </summary>
[DisallowMultipleComponent]
public class InteractableOutline : MonoBehaviour
{
    [Header("Configuración de Contorno")]
    [SerializeField] private Material outlineMaterial;

    private Renderer[] targetRenderers;
    private Material[][] originalMaterials;
    private Material[][] highlightedMaterials;
    private bool isHighlighted;

    public bool IsHighlighted => isHighlighted;

    private void Awake()
    {
        InitializeRenderers();
    }

    private void OnDisable()
    {
        if (isHighlighted)
        {
            SetHighlighted(false);
        }
    }

    /// <summary>
    /// Configura y preasigna los arreglos de materiales compartidos para evitar alocaciones en runtime.
    /// </summary>
    public void InitializeRenderers()
    {
        targetRenderers = GetComponentsInChildren<Renderer>(true);

        if (outlineMaterial == null)
        {
            // Intentar cargar el material de contorno predeterminado desde la carpeta de materiales
            outlineMaterial = Resources.Load<Material>("M_InteractableOutline");
            if (outlineMaterial == null)
            {
                // Fallback seguro: shader PS2Outline si existe en la build
                Shader outlineShader = Shader.Find("Universal Render Pipeline/Custom/PS2Outline");
                if (outlineShader != null)
                {
                    outlineMaterial = new Material(outlineShader);
                    outlineMaterial.SetColor("_OutlineColor", new Color(0.65f, 0.12f, 0.12f, 1f));
                    outlineMaterial.SetFloat("_OutlineWidth", 0.018f);
                }
            }
        }

        if (targetRenderers == null || targetRenderers.Length == 0 || outlineMaterial == null)
            return;

        originalMaterials = new Material[targetRenderers.Length][];
        highlightedMaterials = new Material[targetRenderers.Length][];

        for (int i = 0; i < targetRenderers.Length; i++)
        {
            Renderer rend = targetRenderers[i];
            if (rend == null) continue;

            Material[] currentMats = rend.sharedMaterials;
            originalMaterials[i] = currentMats;

            // Arreglo con el material de contorno añadido al final
            Material[] withOutline = new Material[currentMats.Length + 1];
            for (int m = 0; m < currentMats.Length; m++)
            {
                withOutline[m] = currentMats[m];
            }
            withOutline[currentMats.Length] = outlineMaterial;
            highlightedMaterials[i] = withOutline;
        }
    }

    /// <summary>
    /// Activa o desactiva el contorno rojo apagado.
    /// </summary>
    public void SetHighlighted(bool highlight)
    {
        if (isHighlighted == highlight) return;
        if (targetRenderers == null || highlightedMaterials == null)
        {
            InitializeRenderers();
            if (targetRenderers == null || highlightedMaterials == null) return;
        }

        isHighlighted = highlight;

        for (int i = 0; i < targetRenderers.Length; i++)
        {
            Renderer rend = targetRenderers[i];
            if (rend == null) continue;

            rend.sharedMaterials = highlight ? highlightedMaterials[i] : originalMaterials[i];
        }
    }
}
