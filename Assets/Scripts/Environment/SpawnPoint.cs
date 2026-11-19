using UnityEngine;

namespace MUUKIA
{
    /// <summary>
    /// Punto de aparicion seguro y reutilizable para el jugador.
    /// Garantiza que el CharacterController aparezca con sus pies sobre el suelo,
    /// sin penetrar colisiones ni atravesar el suelo o terreno.
    /// </summary>
    public class SpawnPoint : MonoBehaviour
    {
        [Header("Configuracion")]
        [Tooltip("Si es true, posiciona al jugador de la escena en este punto al iniciar.")]
        [SerializeField] private bool autoSpawnOnStart = false;

        [Tooltip("Margen de elevacion de seguridad respecto al suelo (en metros). Evita que el SkinWidth penetre el colisionador.")]
        [SerializeField] private float safetyOffset = 0.08f;

        [Tooltip("Si es true, realiza un raycast hacia abajo para ubicar con precision la superficie del suelo.")]
        [SerializeField] private bool snapToGroundRaycast = true;

        [Tooltip("Distancia maxima de deteccion del suelo hacia abajo.")]
        [SerializeField] private float raycastDistance = 6f;

        [Header("Gizmos")]
        [SerializeField] private Color gizmoColor = new Color(0.2f, 0.85f, 0.4f, 0.7f);

        private void Start()
        {
            if (autoSpawnOnStart)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player == null)
                {
                    CharacterController cc = Object.FindAnyObjectByType<CharacterController>();
                    if (cc != null) player = cc.gameObject;
                }

                if (player != null)
                {
                    Spawn(player.transform);
                }
            }
        }

        /// <summary>
        /// Ubica de forma segura al jugador en este punto de aparicion.
        /// </summary>
        public void Spawn(Transform playerTransform)
        {
            if (playerTransform == null) return;

            Vector3 targetPos = GetSpawnPosition();
            Quaternion targetRot = transform.rotation;

            CharacterController cc = playerTransform.GetComponent<CharacterController>();
            if (cc != null)
            {
                cc.enabled = false;
            }

            playerTransform.position = targetPos;
            playerTransform.rotation = targetRot;

            Physics.SyncTransforms();

            if (cc != null)
            {
                cc.enabled = true;
            }

            PlayerMovement movement = playerTransform.GetComponent<PlayerMovement>();
            if (movement != null)
            {
                movement.ResetVelocity();
            }
        }

        /// <summary>
        /// Calcula la posicion exacta del punto de aparicion con el margen de seguridad sobre el suelo.
        /// </summary>
        public Vector3 GetSpawnPosition()
        {
            if (snapToGroundRaycast)
            {
                Vector3 rayOrigin = transform.position + Vector3.up * 1.5f;
                if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, raycastDistance + 1.5f, ~0, QueryTriggerInteraction.Ignore))
                {
                    return hit.point + Vector3.up * safetyOffset;
                }
            }

            return transform.position + Vector3.up * safetyOffset;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = gizmoColor;
            Vector3 center = transform.position + Vector3.up * 0.9f;
            Gizmos.DrawWireCube(center, new Vector3(0.7f, 1.8f, 0.7f));
            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(transform.position + Vector3.up * 1.5f, transform.forward * 1.2f);
        }
    }
}
