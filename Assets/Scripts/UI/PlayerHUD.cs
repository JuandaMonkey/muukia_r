using System.Collections;
using UnityEngine;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM || true
using TMPro;
#endif
using MUUKIA;

/// <summary>
/// Controlador principal del HUD minimalista inspirado en Outlast 1 y estética survival horror retro de PS2.
/// Muestra con máxima claridad el consumo de estamina (doble barra con efecto fantasma de drenado),
/// respuesta dinámica a saltos/carreras, mira central discreta, indicaciones de interacción y viñeta de daño.
/// </summary>
public class PlayerHUD : MonoBehaviour
{
    [Header("Referencias a Sistemas del Jugador")]
    [SerializeField] private PlayerStamina playerStamina;
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private PlayerInteraction playerInteraction;
    [SerializeField] private PlayerPickup playerPickup;

    [Header("Mira Central (Crosshair)")]
    [SerializeField] private Image crosshairImage;
    [SerializeField] private Color crosshairNormalColor = new Color(0.85f, 0.85f, 0.85f, 0.35f);
    [SerializeField] private Color crosshairInteractColor = new Color(1f, 1f, 1f, 0.85f);

    [Header("Barra de Resistencia (Stamina)")]
    [SerializeField] private CanvasGroup staminaCanvasGroup;
    [SerializeField] private Image staminaFillImage;
    [SerializeField] private Image staminaGhostFillImage; // Barra de impacto/drenado
    [SerializeField] private float staminaFadeInSpeed = 10f;
    [SerializeField] private float staminaFadeOutSpeed = 2f;
    [SerializeField] private float staminaHideDelay = 1.2f;
    [SerializeField] private float ghostCatchupSpeed = 2.5f;

    [Header("Indicador de Interacción")]
    [SerializeField] private CanvasGroup promptCanvasGroup;
    [SerializeField] private Text promptTextLegacy;
    [SerializeField] private TMP_Text promptTextTMP;
    [SerializeField] private float promptFadeSpeed = 10f;

    [Header("Indicador de Salud (Vignette Outlast)")]
    [SerializeField] private CanvasGroup damageVignetteGroup;
    [SerializeField] private Image damageVignetteImage;
    [SerializeField] private Image healthFillImage = null;
    [SerializeField] private float maxVignetteAlpha = 0.75f;

    [Header("Inventario y Notificaciones (Outlast Minimal)")]
    [SerializeField] private Text inventoryTextLegacy;
    [SerializeField] private TMP_Text inventoryTextTMP;
    [SerializeField] private Text notificationTextLegacy;
    [SerializeField] private TMP_Text notificationTextTMP;
    [SerializeField] private CanvasGroup notificationCanvasGroup;
    private float notificationTimer = 0f;

    private float staminaHideTimer;
    private float targetStaminaAlpha;
    private float targetPromptAlpha;

    private static Sprite solidWhiteSprite;
    private static Texture2D proceduralVignetteTexture;

    private void Awake()
    {
        FindPlayerReferences();
        EnsureUIElementsExist();
        InitializeHUDState();
    }

    private void OnEnable()
    {
        SubscribeEvents();
    }

    private void OnDisable()
    {
        UnsubscribeEvents();
    }

    private void Start()
    {
        UpdateHealthDisplay();
        UpdateStaminaDisplay(1f);
    }

    private void Update()
    {
        UpdateStaminaVisibility();
        UpdatePromptFade();
        UpdateDamageVignette();

        if (notificationTimer > 0f)
        {
            notificationTimer -= Time.deltaTime;
            if (notificationCanvasGroup != null) notificationCanvasGroup.alpha = Mathf.MoveTowards(notificationCanvasGroup.alpha, 1f, 5f * Time.deltaTime);
        }
        else
        {
            if (notificationCanvasGroup != null) notificationCanvasGroup.alpha = Mathf.MoveTowards(notificationCanvasGroup.alpha, 0f, 3f * Time.deltaTime);
        }
    }

    private void FindPlayerReferences()
    {
        if (playerStamina == null) playerStamina = FindAnyObjectByType<PlayerStamina>();
        if (playerHealth == null) playerHealth = FindAnyObjectByType<PlayerHealth>();
        if (playerInteraction == null) playerInteraction = FindAnyObjectByType<PlayerInteraction>();
        if (playerPickup == null) playerPickup = FindAnyObjectByType<PlayerPickup>();
    }

    private static Sprite GetOrCreateWhiteSprite()
    {
        if (solidWhiteSprite == null)
        {
            Texture2D tex = new Texture2D(8, 8, TextureFormat.RGBA32, false);
            Color[] colors = new Color[64];
            for (int i = 0; i < 64; i++) colors[i] = Color.white;
            tex.SetPixels(colors);
            tex.Apply();
            solidWhiteSprite = Sprite.Create(tex, new Rect(0, 0, 8, 8), new Vector2(0.5f, 0.5f));
        }
        return solidWhiteSprite;
    }

    /// <summary>
    /// Construye de forma limpia y procedimental los elementos visuales del HUD.
    /// Incluye doble barra de estamina (barra principal + barra fantasma de impacto).
    /// </summary>
    private void EnsureUIElementsExist()
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null) return;

        Sprite whiteSp = GetOrCreateWhiteSprite();

        // 1. Mira central
        if (crosshairImage == null)
        {
            Transform crosshairT = transform.Find("Crosshair");
            if (crosshairT != null)
            {
                crosshairImage = crosshairT.GetComponent<Image>();
            }
            else
            {
                GameObject crosshairGO = new GameObject("Crosshair", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                crosshairGO.transform.SetParent(transform, false);
                RectTransform rt = crosshairGO.GetComponent<RectTransform>();
                rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
                rt.sizeDelta = new Vector2(3f, 3f);
                crosshairImage = crosshairGO.GetComponent<Image>();
            }
        }
        if (crosshairImage != null)
        {
            crosshairImage.sprite = whiteSp;
            crosshairImage.raycastTarget = false;
            crosshairImage.color = crosshairNormalColor;
        }

        // 2. Viñeta de daño / salud (Outlast style)
        if (damageVignetteGroup == null)
        {
            Transform vignetteT = transform.Find("DamageVignette");
            if (vignetteT != null)
            {
                damageVignetteGroup = vignetteT.GetComponent<CanvasGroup>();
                damageVignetteImage = vignetteT.GetComponent<Image>();
            }
            else
            {
                GameObject vignetteGO = new GameObject("DamageVignette", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(CanvasGroup));
                vignetteGO.transform.SetParent(transform, false);
                vignetteGO.transform.SetAsFirstSibling();
                RectTransform rt = vignetteGO.GetComponent<RectTransform>();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.sizeDelta = Vector2.zero;

                damageVignetteGroup = vignetteGO.GetComponent<CanvasGroup>();
                damageVignetteGroup.blocksRaycasts = false;
                damageVignetteGroup.interactable = false;
                damageVignetteGroup.alpha = 0f;

                damageVignetteImage = vignetteGO.GetComponent<Image>();
                damageVignetteImage.raycastTarget = false;
                damageVignetteImage.color = new Color(0.45f, 0.05f, 0.05f, 1f);
                damageVignetteImage.sprite = GetOrCreateVignetteSprite();
            }
        }

        // 3. Indicador de Interacción
        if (promptCanvasGroup == null)
        {
            Transform promptT = transform.Find("InteractionPrompt");
            if (promptT != null)
            {
                promptCanvasGroup = promptT.GetComponent<CanvasGroup>();
                promptTextLegacy = promptT.GetComponentInChildren<Text>();
                promptTextTMP = promptT.GetComponentInChildren<TMP_Text>();
                if (promptTextLegacy != null) promptTextLegacy.fontSize = 20;
                if (promptTextTMP != null) promptTextTMP.fontSize = 20;
            }
            else
            {
                GameObject promptGO = new GameObject("InteractionPrompt", typeof(RectTransform), typeof(CanvasGroup));
                promptGO.transform.SetParent(transform, false);
                RectTransform rt = promptGO.GetComponent<RectTransform>();
                rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = new Vector2(0f, -50f);
                rt.sizeDelta = new Vector2(550f, 40f);

                promptCanvasGroup = promptGO.GetComponent<CanvasGroup>();
                promptCanvasGroup.blocksRaycasts = false;
                promptCanvasGroup.interactable = false;
                promptCanvasGroup.alpha = 0f;

                GameObject textGO = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text), typeof(Shadow));
                textGO.transform.SetParent(promptGO.transform, false);
                RectTransform textRt = textGO.GetComponent<RectTransform>();
                textRt.anchorMin = Vector2.zero;
                textRt.anchorMax = Vector2.one;
                textRt.sizeDelta = Vector2.zero;

                promptTextLegacy = textGO.GetComponent<Text>();
                promptTextLegacy.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
                promptTextLegacy.fontSize = 20;
                promptTextLegacy.alignment = TextAnchor.MiddleCenter;
                promptTextLegacy.color = new Color(0.95f, 0.95f, 0.92f, 0.98f);
                promptTextLegacy.raycastTarget = false;
                promptTextLegacy.text = string.Empty;

                // Sombra suave para máxima legibilidad sobre cualquier fondo
                Shadow textShadow = textGO.GetComponent<Shadow>();
                textShadow.effectColor = new Color(0f, 0f, 0f, 0.85f);
                textShadow.effectDistance = new Vector2(1.5f, -1.5f);
            }
        }

        // 4. Barra de Resistencia con Capa de Impacto Fantasma
        if (staminaCanvasGroup == null)
        {
            Transform staminaT = transform.Find("StaminaBar");
            if (staminaT != null)
            {
                staminaCanvasGroup = staminaT.GetComponent<CanvasGroup>();
                staminaGhostFillImage = staminaT.Find("GhostFill")?.GetComponent<Image>();
                staminaFillImage = staminaT.Find("Fill")?.GetComponent<Image>();
            }
            else
            {
                // Contenedor principal de la barra (fondo oscuro sutil)
                GameObject staminaGO = new GameObject("StaminaBar", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(CanvasGroup));
                staminaGO.transform.SetParent(transform, false);
                RectTransform rt = staminaGO.GetComponent<RectTransform>();
                rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0f);
                rt.anchoredPosition = new Vector2(0f, 36f);
                rt.sizeDelta = new Vector2(150f, 6f); // 150px de ancho x 6px de alto

                staminaCanvasGroup = staminaGO.GetComponent<CanvasGroup>();
                staminaCanvasGroup.blocksRaycasts = false;
                staminaCanvasGroup.interactable = false;
                staminaCanvasGroup.alpha = 0f;

                Image bgImage = staminaGO.GetComponent<Image>();
                bgImage.sprite = whiteSp;
                bgImage.raycastTarget = false;
                bgImage.color = new Color(0.08f, 0.08f, 0.08f, 0.65f); // Borde/fondo carbón oscuro

                // Capa 1: Barra Fantasma (rojo/ámbar de drenaje)
                GameObject ghostGO = new GameObject("GhostFill", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                ghostGO.transform.SetParent(staminaGO.transform, false);
                RectTransform ghostRt = ghostGO.GetComponent<RectTransform>();
                ghostRt.anchorMin = Vector2.zero;
                ghostRt.anchorMax = Vector2.one;
                ghostRt.sizeDelta = Vector2.zero;

                staminaGhostFillImage = ghostGO.GetComponent<Image>();
                staminaGhostFillImage.sprite = whiteSp;
                staminaGhostFillImage.type = Image.Type.Filled;
                staminaGhostFillImage.fillMethod = Image.FillMethod.Horizontal;
                staminaGhostFillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
                staminaGhostFillImage.fillAmount = 1f;
                staminaGhostFillImage.color = new Color(0.72f, 0.22f, 0.18f, 0.85f); // Tono ámbar/rojo de consumo
                staminaGhostFillImage.raycastTarget = false;

                // Capa 2: Barra Principal (blanco sucio / textura retro)
                GameObject fillGO = new GameObject("Fill", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                fillGO.transform.SetParent(staminaGO.transform, false);
                RectTransform fillRt = fillGO.GetComponent<RectTransform>();
                fillRt.anchorMin = Vector2.zero;
                fillRt.anchorMax = Vector2.one;
                fillRt.sizeDelta = Vector2.zero;

                staminaFillImage = fillGO.GetComponent<Image>();
                staminaFillImage.sprite = whiteSp;
                staminaFillImage.type = Image.Type.Filled;
                staminaFillImage.fillMethod = Image.FillMethod.Horizontal;
                staminaFillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
                staminaFillImage.fillAmount = 1f;
                staminaFillImage.color = new Color(0.88f, 0.86f, 0.82f, 0.95f); // Blanco sucio desgastado
                staminaFillImage.raycastTarget = false;
            }
        }
    }

    private Sprite GetOrCreateVignetteSprite()
    {
        if (proceduralVignetteTexture == null)
        {
            int size = 128;
            proceduralVignetteTexture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            proceduralVignetteTexture.name = "ProceduralDamageVignette";
            proceduralVignetteTexture.wrapMode = TextureWrapMode.Clamp;

            Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
            float maxRadius = size * 0.5f * 1.414f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center);
                    float normalizedDist = dist / maxRadius;
                    float alpha = Mathf.SmoothStep(0.2f, 0.95f, normalizedDist);
                    proceduralVignetteTexture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }
            proceduralVignetteTexture.Apply();
        }

        return Sprite.Create(proceduralVignetteTexture, new Rect(0, 0, proceduralVignetteTexture.width, proceduralVignetteTexture.height), new Vector2(0.5f, 0.5f));
    }

    private void InitializeHUDState()
    {
        if (staminaCanvasGroup != null) staminaCanvasGroup.alpha = 0f;
        if (promptCanvasGroup != null) promptCanvasGroup.alpha = 0f;
        if (damageVignetteGroup != null) damageVignetteGroup.alpha = 0f;
        if (crosshairImage != null) crosshairImage.color = crosshairNormalColor;
    }

    private void SubscribeEvents()
    {
        if (playerInteraction != null)
        {
            playerInteraction.OnPromptChanged += ShowInteractionPrompt;
            playerInteraction.OnPromptCleared += HideInteractionPrompt;
        }

        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged.AddListener(UpdateHealthDisplay);
        }

        if (playerStamina != null)
        {
            playerStamina.OnStaminaChanged += OnStaminaChangedHandler;
        }

        if (PlayerInventory.Instance != null)
        {
            PlayerInventory.Instance.OnInventoryUpdated += UpdateInventoryDisplay;
            PlayerInventory.Instance.OnNotification += ShowDirectNotification;
            UpdateInventoryDisplay(PlayerInventory.Instance.Bandages, PlayerInventory.Instance.EnergyDrinks);
        }
    }

    private void UnsubscribeEvents()
    {
        if (playerInteraction != null)
        {
            playerInteraction.OnPromptChanged -= ShowInteractionPrompt;
            playerInteraction.OnPromptCleared -= HideInteractionPrompt;
        }

        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged.RemoveListener(UpdateHealthDisplay);
        }

        if (playerStamina != null)
        {
            playerStamina.OnStaminaChanged -= OnStaminaChangedHandler;
        }

        if (PlayerInventory.Instance != null)
        {
            PlayerInventory.Instance.OnInventoryUpdated -= UpdateInventoryDisplay;
            PlayerInventory.Instance.OnNotification -= ShowDirectNotification;
        }
    }

    public void ShowDirectNotification(string message)
    {
        if (notificationTextLegacy != null) notificationTextLegacy.text = message;
        if (notificationTextTMP != null) notificationTextTMP.text = message;
        notificationTimer = 3.0f;
    }

    public void UpdateInventoryDisplay(int bandages, int energyDrinks)
    {
        string text = $"[1] VENDAS: {bandages}/{MUUKIA.PlayerInventory.MAX_BANDAGES}   [2] BEBIDAS: {energyDrinks}/{MUUKIA.PlayerInventory.MAX_ENERGY_DRINKS}";
        if (inventoryTextLegacy != null) inventoryTextLegacy.text = text;
        if (inventoryTextTMP != null) inventoryTextTMP.text = text;
    }

    #region Stamina Logic

    private void OnStaminaChangedHandler(float current, float max)
    {
        float targetPercentage = Mathf.Clamp01(current / max);

        // Al gastar estamina (saltos o carrera), hacer la barra inmediatamente visible
        if (targetPercentage < 0.999f)
        {
            targetStaminaAlpha = 1f;
            staminaHideTimer = staminaHideDelay;
        }
    }

    private void UpdateStaminaVisibility()
    {
        if (playerStamina == null || staminaCanvasGroup == null) return;

        float percentage = playerStamina.StaminaPercentage;

        // 1. La barra principal desciende de forma instantánea y precisa
        if (staminaFillImage != null)
        {
            staminaFillImage.fillAmount = percentage;

            // Parpadeo sutil de advertencia en color ámbar/rojo si la estamina es crítica (< 20%)
            if (percentage < 0.20f)
            {
                float warningFlash = (Mathf.Sin(Time.time * 12f) + 1f) * 0.5f;
                staminaFillImage.color = Color.Lerp(new Color(0.88f, 0.86f, 0.82f, 0.95f), new Color(0.85f, 0.35f, 0.25f, 0.95f), warningFlash);
            }
            else
            {
                staminaFillImage.color = new Color(0.88f, 0.86f, 0.82f, 0.95f);
            }
        }

        // 2. La barra fantasma desciende con un retardo fluido para mostrar claramente cuánto se gastó
        if (staminaGhostFillImage != null)
        {
            if (staminaGhostFillImage.fillAmount > percentage)
            {
                staminaGhostFillImage.fillAmount = Mathf.MoveTowards(
                    staminaGhostFillImage.fillAmount,
                    percentage,
                    ghostCatchupSpeed * Time.deltaTime
                );
            }
            else
            {
                staminaGhostFillImage.fillAmount = percentage;
            }
        }

        // 3. Control de visibilidad
        if (percentage < 0.99f)
        {
            targetStaminaAlpha = 1f;
            staminaHideTimer = staminaHideDelay;
        }
        else
        {
            if (staminaHideTimer > 0f)
            {
                staminaHideTimer -= Time.deltaTime;
            }
            else
            {
                targetStaminaAlpha = 0f;
            }
        }

        float speed = targetStaminaAlpha > staminaCanvasGroup.alpha ? staminaFadeInSpeed : staminaFadeOutSpeed;
        staminaCanvasGroup.alpha = Mathf.MoveTowards(staminaCanvasGroup.alpha, targetStaminaAlpha, speed * Time.deltaTime);
    }

    private void UpdateStaminaDisplay(float amount)
    {
        if (staminaFillImage != null) staminaFillImage.fillAmount = amount;
        if (staminaGhostFillImage != null) staminaGhostFillImage.fillAmount = amount;
    }

    #endregion

    #region Interaction Prompt Logic

    public void ShowInteractionPrompt(string message)
    {
        if (promptTextLegacy != null)
        {
            promptTextLegacy.fontSize = 20;
            promptTextLegacy.text = message;
        }

        if (promptTextTMP != null)
        {
            promptTextTMP.fontSize = 20;
            promptTextTMP.text = message;
        }

        targetPromptAlpha = 1f;

        if (crosshairImage != null)
        {
            crosshairImage.color = crosshairInteractColor;
        }
    }

    public void HideInteractionPrompt()
    {
        targetPromptAlpha = 0f;

        if (crosshairImage != null)
        {
            crosshairImage.color = crosshairNormalColor;
        }
    }

    private void UpdatePromptFade()
    {
        if (promptCanvasGroup == null) return;

        promptCanvasGroup.alpha = Mathf.MoveTowards(
            promptCanvasGroup.alpha,
            targetPromptAlpha,
            promptFadeSpeed * Time.deltaTime
        );
    }

    #endregion

    #region Health / Damage Vignette Logic

    public void UpdateHealthDisplay()
    {
        if (playerHealth == null) return;

        float healthPercentage = playerHealth.CurrentHealth / playerHealth.MaxHealth;

        if (healthFillImage != null)
        {
            healthFillImage.fillAmount = healthPercentage;
        }
    }

    private void UpdateDamageVignette()
    {
        if (damageVignetteGroup == null || playerHealth == null) return;

        float healthFactor = Mathf.Clamp01(playerHealth.CurrentHealth / playerHealth.MaxHealth);
        float damageFactor = 1f - healthFactor;

        float targetAlpha = 0f;

        if (damageFactor > 0.05f)
        {
            targetAlpha = Mathf.Pow(damageFactor, 1.4f) * maxVignetteAlpha;

            if (healthFactor <= 0.25f)
            {
                float pulse = (Mathf.Sin(Time.time * 6f) + 1f) * 0.5f;
                targetAlpha += pulse * 0.15f;
            }
        }

        damageVignetteGroup.alpha = Mathf.Lerp(damageVignetteGroup.alpha, targetAlpha, 5f * Time.deltaTime);
    }

    #endregion
}
