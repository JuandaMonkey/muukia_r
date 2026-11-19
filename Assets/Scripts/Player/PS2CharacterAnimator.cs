using UnityEngine;

namespace MUUKIA
{
    /// <summary>
    /// Animador procedural estilo PS2 para personajes low-poly.
    /// Simula las animaciones características de principios de los 2000:
    /// balanceo rígido al caminar, respiración sutil, giro de cabeza, temblor y posturas sentadas.
    /// </summary>
    public class PS2CharacterAnimator : MonoBehaviour
    {
        public enum AnimationState { Idle, Walk, Run, Seated, Trembling, ScaryTwitch }

        [Header("Estado Actual")]
        [SerializeField] private AnimationState currentState = AnimationState.Idle;

        [Header("Jerarquía de Miembros (Opcional)")]
        [SerializeField] private Transform headBone;
        [SerializeField] private Transform torsoBone;
        [SerializeField] private Transform leftArmBone;
        [SerializeField] private Transform rightArmBone;
        [SerializeField] private Transform leftLegBone;
        [SerializeField] private Transform rightLegBone;

        [Header("Ajustes de Movimiento")]
        [SerializeField] private float walkCycleSpeed = 6f;
        [SerializeField] private float breathingSpeed = 2f;
        [SerializeField] private float breathingAmount = 0.02f;
        [SerializeField] private float stepStride = 25f;

        private Vector3 initialPosition;
        private Quaternion initialRotation;
        private float animTime = 0f;

        private void Start()
        {
            initialPosition = transform.localPosition;
            initialRotation = transform.localRotation;
            animTime = Random.Range(0f, 10f);
        }

        private void Update()
        {
            animTime += Time.deltaTime;

            switch (currentState)
            {
                case AnimationState.Idle:
                    AnimateIdle();
                    break;

                case AnimationState.Walk:
                    AnimateWalk(1f);
                    break;

                case AnimationState.Run:
                    AnimateWalk(1.6f);
                    break;

                case AnimationState.Seated:
                    AnimateSeated();
                    break;

                case AnimationState.Trembling:
                    AnimateTrembling();
                    break;

                case AnimationState.ScaryTwitch:
                    AnimateScaryTwitch();
                    break;
            }
        }

        public void SetState(AnimationState state)
        {
            currentState = state;
        }

        private void AnimateIdle()
        {
            // Respiración vertical PS2
            float breath = Mathf.Sin(animTime * breathingSpeed) * breathingAmount;
            transform.localPosition = initialPosition + new Vector3(0, breath, 0);

            if (torsoBone != null)
            {
                float tilt = Mathf.Sin(animTime * (breathingSpeed * 0.5f)) * 1.5f;
                torsoBone.localRotation = Quaternion.Euler(tilt, 0, 0);
            }
        }

        private void AnimateWalk(float speedMultiplier)
        {
            float t = animTime * walkCycleSpeed * speedMultiplier;
            float bob = Mathf.Abs(Mathf.Sin(t)) * 0.05f;
            float sway = Mathf.Sin(t) * 2f;

            transform.localPosition = initialPosition + new Vector3(0, bob, 0);
            transform.localRotation = initialRotation * Quaternion.Euler(0, 0, sway);

            // Oscilación de extremidades si están asignadas
            float legAngle = Mathf.Sin(t) * stepStride;
            if (leftLegBone != null) leftLegBone.localRotation = Quaternion.Euler(legAngle, 0, 0);
            if (rightLegBone != null) rightLegBone.localRotation = Quaternion.Euler(-legAngle, 0, 0);

            if (leftArmBone != null) leftArmBone.localRotation = Quaternion.Euler(-legAngle * 0.8f, 0, 0);
            if (rightArmBone != null) rightArmBone.localRotation = Quaternion.Euler(legAngle * 0.8f, 0, 0);
        }

        private void AnimateSeated()
        {
            float subtleBreath = Mathf.Sin(animTime * 1.5f) * 0.01f;
            transform.localPosition = initialPosition + new Vector3(0, subtleBreath, 0);

            if (torsoBone != null)
            {
                torsoBone.localRotation = Quaternion.Euler(5f + subtleBreath * 50f, 0, 0);
            }
        }

        private void AnimateTrembling()
        {
            // Temblor de pánico psicológico
            float jitterX = (Random.value - 0.5f) * 0.02f;
            float jitterZ = (Random.value - 0.5f) * 0.02f;
            transform.localPosition = initialPosition + new Vector3(jitterX, 0, jitterZ);
        }

        private void AnimateScaryTwitch()
        {
            // Movimiento antinatural de MUUKIA (estilo entidad espectral japonesa/yaqui)
            float twitch = 0f;
            if (Random.value < 0.06f)
            {
                twitch = (Random.value - 0.5f) * 35f;
            }

            if (headBone != null)
            {
                headBone.localRotation = Quaternion.Euler(twitch, Random.Range(-10f, 10f), twitch * 0.5f);
            }

            float unnaturalSway = Mathf.Sin(animTime * 0.8f) * 4f;
            transform.localRotation = initialRotation * Quaternion.Euler(0, unnaturalSway, 0);
        }
    }
}
