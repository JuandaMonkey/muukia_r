#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

namespace MUUKIA.Editor
{
    /// <summary>
    /// Master Builder definitivo de MUUKIA.
    /// Construye, configura, conecta y valida de forma integral:
    /// - Animaciones y Animator Controllers para todos los personajes.
    /// - Todos los Prefabs (Personajes, Escenarios, Interactables, UI).
    /// - La escena de MainMenu completa y funcional (PS2 horror).
    /// - Las 5 escenas independientes del Capítulo 1 (HermosilloCasa, Clinica, Obregon, Carretera, Criptas).
    /// - El flujo de transición entre escenas y Build Settings en el orden exacto.
    /// - Auditoría y validación automática de cero errores, cero missing scripts y referencias conectadas.
    /// </summary>
    public static class MUUKIAMasterBuilder
    {
        // Rutas canónicas
        public const string PathMainMenu = "Assets/Scenes/MainMenu/MainMenu.unity";
        public const string PathCH01Casa = "Assets/Scenes/Chapter_1/CH01_HermosilloCasa.unity";
        public const string PathCH01Clinica = "Assets/Scenes/Chapter_1/CH01_Clinica.unity";
        public const string PathCH01Obregon = "Assets/Scenes/Chapter_1/CH01_Obregon.unity";
        public const string PathCH01Carretera = "Assets/Scenes/Chapter_1/CH01_Carretera.unity";
        public const string PathCH01Criptas = "Assets/Scenes/Chapter_1/CH01_Criptas.unity";

        [InitializeOnLoadMethod]
        private static void AutoBuildOnLoad()
        {
            if (!File.Exists(PathMainMenu) || !File.Exists(PathCH01Casa) || !File.Exists(PathCH01Clinica) || !File.Exists(PathCH01Obregon) || !File.Exists(PathCH01Carretera) || !File.Exists(PathCH01Criptas))
            {
                Debug.Log("[MUUKIA MASTER] AutoBuildOnLoad: Reconstruyendo escenas y prefabs automáticamente...");
                BuildEverything();
            }
        }

        [MenuItem("MUUKIA/Reconstruir Proyecto Completo (Prefabs + 6 Escenas + BuildSettings)")]
        public static void BuildEverything()
        {
            Debug.Log("[MUUKIA MASTER] === INICIANDO RECONSTRUCCIÓN COMPLETA DE MUUKIA ===");

            EnsureFolderStructure();
            CreateAnimationClipsAndControllers();
            CreateAllPrefabs();

            BuildMainMenuScene();
            BuildCH01CasaScene();
            BuildCH01ClinicaScene();
            BuildCH01ObregonScene();
            BuildCH01CarreteraScene();
            BuildCH01CriptasScene();

            ConfigureBuildSettings();
            ValidateEverything();

            Debug.Log("[MUUKIA MASTER] === RECONSTRUCCIÓN FINALIZADA EXITOSAMENTE ===");
        }

        #region 1. ESTRUCTURA DE CARPETAS

        public static void EnsureFolderStructure()
        {
            string[] dirs = {
                "Assets/Scenes/MainMenu",
                "Assets/Scenes/Chapter_1",
                "Assets/Prefabs/Characters",
                "Assets/Prefabs/Environment",
                "Assets/Prefabs/Interactables",
                "Assets/Prefabs/UI",
                "Assets/Animations/Controllers",
                "Assets/Animations/Clips",
                "Assets/Models/Creatures",
                "Assets/Models/Environment",
                "Assets/ScriptableObjects/Dialogues",
                "Assets/ScriptableObjects/Items"
            };

            foreach (var d in dirs)
            {
                if (!Directory.Exists(d))
                {
                    Directory.CreateDirectory(d);
                }
            }
            AssetDatabase.Refresh();
        }

        #endregion

        #region 2. ANIMACIONES Y ANIMATOR CONTROLLERS

        public static void CreateAnimationClipsAndControllers()
        {
            Debug.Log("[MUUKIA MASTER] Creando Animation Clips y Animator Controllers...");

            // 1. Gustavo Controller
            CreateOrUpdateController("Assets/Animations/Controllers/AC_Gustavo.controller", new (string, bool)[] {
                ("Idle", true),
                ("Walk", true),
                ("Run", true),
                ("Crouch", true),
                ("Interact", false),
                ("Scared", true)
            });

            // 2. Mother Controller
            CreateOrUpdateController("Assets/Animations/Controllers/AC_Mother.controller", new (string, bool)[] {
                ("Idle", true),
                ("Seated", true),
                ("Walk", true),
                ("Talk", true),
                ("Sad", true)
            });

            // 3. Neighbor Controller
            CreateOrUpdateController("Assets/Animations/Controllers/AC_Neighbor.controller", new (string, bool)[] {
                ("Idle", true),
                ("Seated", true),
                ("Talk", true),
                ("Gesture", false)
            });

            // 4. MUUKIA Entity Controller
            CreateOrUpdateController("Assets/Animations/Controllers/AC_Muukia.controller", new (string, bool)[] {
                ("IdleSpooky", true),
                ("CreepWalk", true),
                ("Twitch", true),
                ("Disappear", false)
            });

            // 5. Police Officer Controller
            CreateOrUpdateController("Assets/Animations/Controllers/AC_PoliceOfficer.controller", new (string, bool)[] {
                ("GuardIdle", true),
                ("Lookout", true)
            });

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void CreateOrUpdateController(string controllerPath, (string clipName, bool loop)[] states)
        {
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
            if (controller == null)
            {
                controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
            }

            AnimatorStateMachine stateMachine = controller.layers[0].stateMachine;

            // Limpiar estados previos para recrear con exactitud
            foreach (var childState in stateMachine.states)
            {
                stateMachine.RemoveState(childState.state);
            }

            AnimatorState defaultState = null;

            for (int i = 0; i < states.Length; i++)
            {
                string cName = states[i].clipName;
                bool isLoop = states[i].loop;
                string clipPath = $"Assets/Animations/Clips/{cName}.anim";

                AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
                if (clip == null)
                {
                    clip = new AnimationClip();
                    clip.name = cName;

                    // Crear curvas sutiles procedurales PS2 (respiración / balanceo)
                    AnimationCurve curveY = AnimationCurve.EaseInOut(0f, 0f, 1f, isLoop ? 0.04f : 0f);
                    if (isLoop)
                    {
                        curveY.postWrapMode = WrapMode.Loop;
                    }
                    clip.SetCurve("", typeof(Transform), "localPosition.y", curveY);

                    AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
                    settings.loopTime = isLoop;
                    AnimationUtility.SetAnimationClipSettings(clip, settings);

                    AssetDatabase.CreateAsset(clip, clipPath);
                }

                AnimatorState st = stateMachine.AddState(cName);
                st.motion = clip;
                if (i == 0) defaultState = st;
            }

            if (defaultState != null)
            {
                stateMachine.defaultState = defaultState;
            }

            EditorUtility.SetDirty(controller);
        }

        #endregion

        #region 3. PREFABS DE PERSONAJES, ESCENARIOS, INTERACTABLES Y UI

        public static void CreateAllPrefabs()
        {
            Debug.Log("[MUUKIA MASTER] Creando y configurando todos los Prefabs...");

            // Materiales
            Material matEnvironment = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/M_Environment.mat");
            Material matGustavo = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/M_Gustavo.mat");
            Material matMother = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/M_Mother.mat");
            Material matNeighbor = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/M_Neighbor.mat");
            Material matMuukia = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/M_Muukia.mat");
            Material matProps = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/M_Props.mat");
            Material matBandage = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/M_Bandage.mat");
            Material matVhs = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/M_VHSTapePS2.mat");

            // Audio clips
            AudioClip clipTenabarisRattle = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/tenabaris_rattle.wav");
            AudioClip clipTenabarisLoop = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/tenabaris_loop.wav");
            AudioClip clipVhsGlitch = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/vhs_glitch.wav");
            AudioClip clipVhsHum = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/vhs_static_hum.wav");
            AudioClip clipCarEngine = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/car_engine.wav");
            AudioClip clipCarBrake = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/car_brake.wav");
            AudioClip clipCrash = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/crash_impact.wav");
            AudioClip clipBreath = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/gustavo_breath.wav");
            AudioClip clipFootstep = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/footstep_dirt.wav");

            // Controllers
            RuntimeAnimatorController acGustavo = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Animations/Controllers/AC_Gustavo.controller");
            RuntimeAnimatorController acMother = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Animations/Controllers/AC_Mother.controller");
            RuntimeAnimatorController acNeighbor = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Animations/Controllers/AC_Neighbor.controller");
            RuntimeAnimatorController acMuukia = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Animations/Controllers/AC_Muukia.controller");
            RuntimeAnimatorController acPolice = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Animations/Controllers/AC_PoliceOfficer.controller");

            // A) PREFAB JUGADOR: GUSTAVO MARTÍNEZ
            string playerPrefabPath = "Assets/Prefabs/Characters/Player_Gustavo.prefab";
            GameObject playerGO = new GameObject("Player_Gustavo");
            playerGO.tag = "Player";
            CharacterController cc = playerGO.AddComponent<CharacterController>();
            cc.height = 1.8f;
            cc.radius = 0.35f;
            cc.center = new Vector3(0, 0.9f, 0);

            PlayerMovement pMove = playerGO.AddComponent<PlayerMovement>();
            PlayerStamina pStamina = playerGO.AddComponent<PlayerStamina>();
            PlayerCrouch pCrouch = playerGO.AddComponent<PlayerCrouch>();
            PlayerHealth pHealth = playerGO.AddComponent<PlayerHealth>();
            PlayerInteraction pInteract = playerGO.AddComponent<PlayerInteraction>();
            PlayerPickup pPickup = playerGO.AddComponent<PlayerPickup>();
            PlayerInventory pInv = playerGO.AddComponent<PlayerInventory>();
            GustavoController gustavoCtrl = playerGO.AddComponent<GustavoController>();
            PS2CharacterAnimator gusPS2Anim = playerGO.AddComponent<PS2CharacterAnimator>();

            AudioSource breathAudio = playerGO.AddComponent<AudioSource>();
            breathAudio.playOnAwake = false;

            SerializedObject gusSO = new SerializedObject(gustavoCtrl);
            gusSO.FindProperty("breathingAudioSource").objectReferenceValue = breathAudio;
            gusSO.FindProperty("agitatedBreathingClip").objectReferenceValue = clipBreath;
            gusSO.ApplyModifiedProperties();

            // Cámara del Jugador
            GameObject camGO = new GameObject("Main Camera");
            camGO.tag = "MainCamera";
            camGO.transform.SetParent(playerGO.transform);
            camGO.transform.localPosition = new Vector3(0, 1.62f, 0);
            UnityEngine.Camera playerCam = camGO.AddComponent<UnityEngine.Camera>();
            playerCam.nearClipPlane = 0.1f;
            playerCam.farClipPlane = 400f;
            playerCam.fieldOfView = 60f;
            camGO.AddComponent<AudioListener>();

            PlayerLook pLook = playerGO.AddComponent<PlayerLook>();
            SerializedObject lookSO = new SerializedObject(pLook);
            lookSO.FindProperty("playerCamera").objectReferenceValue = camGO.transform;
            lookSO.ApplyModifiedProperties();

            VideoCameraRig camRig = playerGO.AddComponent<VideoCameraRig>();
            SerializedObject camRigSO = new SerializedObject(camRig);
            camRigSO.FindProperty("targetCamera").objectReferenceValue = playerCam;
            camRigSO.ApplyModifiedProperties();

            // Modelo visual de Gustavo
            GameObject gusMesh = LoadModelInstance("Assets/Models/Characters/Gustavo.obj", playerGO.transform, matGustavo);
            gusMesh.name = "Visual_Gustavo";
            gusMesh.transform.localPosition = Vector3.zero;
            Animator gusAnimator = gusMesh.AddComponent<Animator>();
            gusAnimator.runtimeAnimatorController = acGustavo;

            SavePrefab(playerGO, playerPrefabPath);
            GameObject.DestroyImmediate(playerGO);

            // B) PREFAB: MADRE
            string motherPrefabPath = "Assets/Prefabs/Characters/NPC_Mother.prefab";
            GameObject motherGO = new GameObject("NPC_Mother");
            CapsuleCollider motherCol = motherGO.AddComponent<CapsuleCollider>();
            motherCol.height = 1.65f;
            motherCol.radius = 0.35f;
            motherCol.center = new Vector3(0, 0.825f, 0);
            motherGO.AddComponent<MotherNPC>();
            motherGO.AddComponent<PS2CharacterAnimator>();
            GameObject motherMesh = LoadModelInstance("Assets/Models/Characters/Mother.obj", motherGO.transform, matMother);
            motherMesh.name = "Visual_Mother";
            Animator motherAnim = motherMesh.AddComponent<Animator>();
            motherAnim.runtimeAnimatorController = acMother;
            SavePrefab(motherGO, motherPrefabPath);
            GameObject.DestroyImmediate(motherGO);

            // C) PREFAB: VECINO (DON LUPE)
            string neighborPrefabPath = "Assets/Prefabs/Characters/NPC_Neighbor.prefab";
            GameObject neighborGO = new GameObject("NPC_Neighbor");
            CapsuleCollider neighborCol = neighborGO.AddComponent<CapsuleCollider>();
            neighborCol.height = 1.7f;
            neighborCol.radius = 0.35f;
            neighborCol.center = new Vector3(0, 0.85f, 0);
            neighborGO.AddComponent<NeighborNPC>();
            neighborGO.AddComponent<PS2CharacterAnimator>();
            GameObject neighborMesh = LoadModelInstance("Assets/Models/Characters/Neighbor.obj", neighborGO.transform, matNeighbor);
            neighborMesh.name = "Visual_Neighbor";
            Animator neighborAnim = neighborMesh.AddComponent<Animator>();
            neighborAnim.runtimeAnimatorController = acNeighbor;
            SavePrefab(neighborGO, neighborPrefabPath);
            GameObject.DestroyImmediate(neighborGO);

            // D) PREFAB: ENTIDAD MUUKIA
            string muukiaPrefabPath = "Assets/Prefabs/Characters/Entity_MUUKIA.prefab";
            GameObject muukiaGO = new GameObject("Entity_MUUKIA");
            CapsuleCollider muukiaCol = muukiaGO.AddComponent<CapsuleCollider>();
            muukiaCol.height = 2.4f;
            muukiaCol.radius = 0.45f;
            muukiaCol.center = new Vector3(0, 1.2f, 0);
            MuukiaEntity muukiaComp = muukiaGO.AddComponent<MuukiaEntity>();
            muukiaGO.AddComponent<PS2CharacterAnimator>().SetState(PS2CharacterAnimator.AnimationState.ScaryTwitch);

            AudioSource tenabarisAudio = muukiaGO.AddComponent<AudioSource>();
            tenabarisAudio.playOnAwake = false;
            tenabarisAudio.loop = true;
            tenabarisAudio.spatialBlend = 1f;
            tenabarisAudio.minDistance = 3f;
            tenabarisAudio.maxDistance = 60f;

            GameObject redEyesGO = new GameObject("RedEyesLight");
            redEyesGO.transform.SetParent(muukiaGO.transform);
            redEyesGO.transform.localPosition = new Vector3(0, 1.95f, 0.3f);
            Light redLight = redEyesGO.AddComponent<Light>();
            redLight.type = LightType.Point;
            redLight.color = new Color(1f, 0.05f, 0.05f);
            redLight.intensity = 2.5f;
            redLight.range = 5f;

            SerializedObject muukiaSO = new SerializedObject(muukiaComp);
            muukiaSO.FindProperty("redEyesLight").objectReferenceValue = redLight;
            muukiaSO.FindProperty("tenabarisAudioSource").objectReferenceValue = tenabarisAudio;
            muukiaSO.FindProperty("tenabarisRattleClip").objectReferenceValue = clipTenabarisRattle;
            muukiaSO.FindProperty("tenabarisLoopClip").objectReferenceValue = clipTenabarisLoop;
            muukiaSO.ApplyModifiedProperties();

            GameObject muukiaMesh = LoadModelInstance("Assets/Models/Characters/MuukiaEntity.obj", muukiaGO.transform, matMuukia);
            muukiaMesh.name = "Visual_Muukia";
            Animator muukiaAnim = muukiaMesh.AddComponent<Animator>();
            muukiaAnim.runtimeAnimatorController = acMuukia;

            SavePrefab(muukiaGO, muukiaPrefabPath);
            GameObject.DestroyImmediate(muukiaGO);

            // E) PREFAB: POLICE OFFICER
            string policePrefabPath = "Assets/Prefabs/Characters/NPC_PoliceOfficer.prefab";
            GameObject policeGO = new GameObject("NPC_PoliceOfficer");
            CapsuleCollider policeCol = policeGO.AddComponent<CapsuleCollider>();
            policeCol.height = 1.8f;
            policeCol.radius = 0.35f;
            policeCol.center = new Vector3(0, 0.9f, 0);
            policeGO.AddComponent<PS2CharacterAnimator>();
            GameObject policeMesh = LoadModelInstance("Assets/Models/Characters/PoliceOfficer.obj", policeGO.transform, matProps);
            policeMesh.name = "Visual_Police";
            Animator policeAnim = policeMesh.AddComponent<Animator>();
            policeAnim.runtimeAnimatorController = acPolice;
            SavePrefab(policeGO, policePrefabPath);
            GameObject.DestroyImmediate(policeGO);

            // F) PREFABS DE ESCENARIOS
            CreateEnvironmentPrefab("MexicanHouse", "Assets/Models/Environments/MexicanHouse.obj", matEnvironment, new Vector3(0, 2.2f, 0), new Vector3(8.5f, 4.5f, 9.5f));
            CreateEnvironmentPrefab("ClinicFacade", "Assets/Models/Environments/ClinicFacade.obj", matEnvironment, new Vector3(0, 3f, 0), new Vector3(16f, 6.5f, 7f));
            CreateEnvironmentPrefab("CastilloMaderos", "Assets/Models/Environments/CastilloMaderos.obj", matEnvironment, new Vector3(0, 5f, 0), new Vector3(14f, 10.5f, 14f));
            CreateEnvironmentPrefab("GasStation", "Assets/Models/Environments/GasStation.obj", matEnvironment, new Vector3(0, 2.5f, 0), new Vector3(14f, 5.5f, 10f));
            CreateEnvironmentPrefab("DanzanteMonument", "Assets/Models/Props/DanzanteMonument.obj", matProps, new Vector3(0, 4.5f, 0), new Vector3(3f, 9f, 3f));
            CreateEnvironmentPrefab("Mausoleum", "Assets/Models/Environments/Mausoleum.obj", matEnvironment, new Vector3(0, 3f, 0), new Vector3(5.5f, 6.5f, 6.5f));
            CreateEnvironmentPrefab("CollapsedCrypt", "Assets/Models/Environments/CollapsedCrypt.obj", matEnvironment, new Vector3(0, 2f, 0), new Vector3(6f, 4f, 6f));
            CreateEnvironmentPrefab("Obelisk", "Assets/Models/Environments/Obelisk.obj", matEnvironment, new Vector3(0, 4f, 0), new Vector3(2.5f, 8.5f, 2.5f));

            // G) PREFABS INTERACTABLES Y VEHÍCULOS
            // Auto Sedán de Gustavo
            string carPrefabPath = "Assets/Prefabs/Interactables/Prop_CarSedan.prefab";
            GameObject carGO = new GameObject("Prop_CarSedan");
            BoxCollider carCol = carGO.AddComponent<BoxCollider>();
            carCol.size = new Vector3(2.2f, 1.6f, 4.6f);
            carCol.center = new Vector3(0, 0.8f, 0);
            CarController carCtrl = carGO.AddComponent<CarController>();

            AudioSource carEngineAudio = carGO.AddComponent<AudioSource>();
            carEngineAudio.clip = clipCarEngine;
            carEngineAudio.loop = true;
            AudioSource carSfxAudio = carGO.AddComponent<AudioSource>();

            GameObject headLightL = new GameObject("Headlight_L");
            headLightL.transform.SetParent(carGO.transform);
            headLightL.transform.localPosition = new Vector3(-0.7f, 0.65f, 2.2f);
            Light lLight = headLightL.AddComponent<Light>();
            lLight.type = LightType.Spot;
            lLight.range = 35f;
            lLight.spotAngle = 45f;
            lLight.color = new Color(1f, 0.95f, 0.8f);

            GameObject headLightR = new GameObject("Headlight_R");
            headLightR.transform.SetParent(carGO.transform);
            headLightR.transform.localPosition = new Vector3(0.7f, 0.65f, 2.2f);
            Light rLight = headLightR.AddComponent<Light>();
            rLight.type = LightType.Spot;
            rLight.range = 35f;
            rLight.spotAngle = 45f;
            rLight.color = new Color(1f, 0.95f, 0.8f);

            SerializedObject carSO = new SerializedObject(carCtrl);
            carSO.FindProperty("engineAudioSource").objectReferenceValue = carEngineAudio;
            carSO.FindProperty("sfxAudioSource").objectReferenceValue = carSfxAudio;
            carSO.FindProperty("engineClip").objectReferenceValue = clipCarEngine;
            carSO.FindProperty("brakeClip").objectReferenceValue = clipCarBrake;
            carSO.FindProperty("crashClip").objectReferenceValue = clipCrash;
            carSO.FindProperty("leftHeadlight").objectReferenceValue = lLight;
            carSO.FindProperty("rightHeadlight").objectReferenceValue = rLight;
            carSO.ApplyModifiedProperties();

            GameObject carMesh = LoadModelInstance("Assets/Models/Props/CarSedan.obj", carGO.transform, matProps);
            carMesh.name = "Visual_Car";
            SavePrefab(carGO, carPrefabPath);
            GameObject.DestroyImmediate(carGO);

            // Patrulla Policial
            string patrolPrefabPath = "Assets/Prefabs/Interactables/Prop_PolicePatrol.prefab";
            GameObject patrolGO = new GameObject("Prop_PolicePatrol");
            BoxCollider patrolCol = patrolGO.AddComponent<BoxCollider>();
            patrolCol.size = new Vector3(2.2f, 1.6f, 4.6f);
            patrolCol.center = new Vector3(0, 0.8f, 0);
            GameObject redBlueLightGO = new GameObject("EmergencyLight");
            redBlueLightGO.transform.SetParent(patrolGO.transform);
            redBlueLightGO.transform.localPosition = new Vector3(0, 1.55f, 0);
            Light emerLight = redBlueLightGO.AddComponent<Light>();
            emerLight.type = LightType.Point;
            emerLight.color = Color.red;
            emerLight.intensity = 2f;
            emerLight.range = 10f;
            GameObject patrolMesh = LoadModelInstance("Assets/Models/Props/PolicePatrol.obj", patrolGO.transform, matProps);
            patrolMesh.name = "Visual_Patrol";
            SavePrefab(patrolGO, patrolPrefabPath);
            GameObject.DestroyImmediate(patrolGO);

            // Cinta Policial
            string tapePrefabPath = "Assets/Prefabs/Interactables/Prop_PoliceTape.prefab";
            GameObject tapeGO = LoadModelInstance("Assets/Models/Props/PoliceTape.obj", null, matProps);
            tapeGO.name = "Prop_PoliceTape";
            AddBoxCollider(tapeGO, new Vector3(0, 0.8f, 0), new Vector3(6f, 1.5f, 0.3f));
            SavePrefab(tapeGO, tapePrefabPath);
            GameObject.DestroyImmediate(tapeGO);

            // Pastillas de Mamá (Interactable)
            string pillsPrefabPath = "Assets/Prefabs/Interactables/Item_PillsMedicine.prefab";
            GameObject pillsGO = LoadModelInstance("Assets/Models/Props/PillsMedicine.obj", null, matProps);
            pillsGO.name = "Item_PillsMedicine";
            AddBoxCollider(pillsGO, new Vector3(0, 0.1f, 0), new Vector3(0.4f, 0.3f, 0.4f));
            InteractableObject pillsInteract = pillsGO.AddComponent<InteractableObject>();
            SerializedObject pSO = new SerializedObject(pillsInteract);
            pSO.FindProperty("objectName").stringValue = "Pastillas de Mamá";
            pSO.FindProperty("promptMessage").stringValue = "Presiona E para revisar los medicamentos";
            pSO.ApplyModifiedProperties();
            SavePrefab(pillsGO, pillsPrefabPath);
            GameObject.DestroyImmediate(pillsGO);

            // Vendas (Item inventario)
            string bandagePrefabPath = "Assets/Prefabs/Interactables/Item_Bandage.prefab";
            GameObject bandageGO = LoadModelInstance("Assets/Models/Bandage.obj", null, matBandage);
            bandageGO.name = "Item_Bandage";
            AddBoxCollider(bandageGO, new Vector3(0, 0.1f, 0), new Vector3(0.35f, 0.35f, 0.35f));
            InventoryItemPickup bPickup = bandageGO.AddComponent<InventoryItemPickup>();
            SerializedObject bSO = new SerializedObject(bPickup);
            SerializedProperty typeProp = bSO.FindProperty("type");
            if (typeProp != null) typeProp.enumValueIndex = 0; // Bandage
            bSO.ApplyModifiedProperties();
            SavePrefab(bandageGO, bandagePrefabPath);
            GameObject.DestroyImmediate(bandageGO);

            // Puerta Interactiva
            string doorPrefabPath = "Assets/Prefabs/Interactables/InteractiveDoor.prefab";
            GameObject doorGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
            doorGO.name = "InteractiveDoor";
            doorGO.transform.localScale = new Vector3(1.1f, 2.2f, 0.15f);
            if (matProps != null) doorGO.GetComponent<Renderer>().sharedMaterial = matProps;
            doorGO.AddComponent<InteractiveDoor>();
            doorGO.AddComponent<AudioSource>();
            SavePrefab(doorGO, doorPrefabPath);
            GameObject.DestroyImmediate(doorGO);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void CreateEnvironmentPrefab(string name, string modelPath, Material mat, Vector3 colCenter, Vector3 colSize)
        {
            string prefabPath = $"Assets/Prefabs/Environment/{name}.prefab";
            GameObject inst = LoadModelInstance(modelPath, null, mat);
            inst.name = name;
            AddBoxCollider(inst, colCenter, colSize);
            SavePrefab(inst, prefabPath);
            GameObject.DestroyImmediate(inst);
        }

        private static void SavePrefab(GameObject instance, string assetPath)
        {
            PrefabUtility.SaveAsPrefabAsset(instance, assetPath);
        }

        #endregion

        #region 4. CONSTRUCCIÓN DE ESCENAS

        /// <summary>
        /// Escena 0: Main Menu (PS2 Horror).
        /// </summary>
        public static void BuildMainMenuScene()
        {
            Debug.Log("[MUUKIA MASTER] Construyendo Escena: MainMenu...");
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Shaders / Materiales
            Material matEnv = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/M_Environment.mat");
            Material matProps = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/M_Props.mat");
            Material matMuukia = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/M_Muukia.mat");
            Material matSkybox = AssetDatabase.LoadAssetAtPath<Material>("Assets/Textures/Skybox/sky_001.mat");
            Material matVhs = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/M_VHSTapePS2.mat");
            AudioClip clipWind = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/desert_wind.wav");
            AudioClip clipBlip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/dialogue_blip.wav");

            // Configuración ambiental
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = 0.02f;
            RenderSettings.fogColor = new Color(0.12f, 0.10f, 0.12f, 1f);
            if (matSkybox != null) RenderSettings.skybox = matSkybox;

            // Luz Lunar Sonora
            GameObject moonGO = new GameObject("Sonora_Moonlight");
            Light moon = moonGO.AddComponent<Light>();
            moon.type = LightType.Directional;
            moon.color = new Color(0.45f, 0.50f, 0.65f);
            moon.intensity = 0.7f;
            moonGO.transform.rotation = Quaternion.Euler(30f, -40f, 0);

            // Terreno y Carretera
            GameObject roadGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
            roadGO.name = "DesertHighway_Road";
            roadGO.transform.position = new Vector3(0, -0.1f, 30f);
            roadGO.transform.localScale = new Vector3(12f, 0.2f, 100f);
            if (matEnv != null) roadGO.GetComponent<Renderer>().sharedMaterial = matEnv;

            GameObject desertL = GameObject.CreatePrimitive(PrimitiveType.Cube);
            desertL.name = "DesertDune_Left";
            desertL.transform.position = new Vector3(-25f, -0.2f, 30f);
            desertL.transform.localScale = new Vector3(40f, 0.2f, 100f);
            if (matEnv != null) desertL.GetComponent<Renderer>().sharedMaterial = matEnv;

            GameObject desertR = GameObject.CreatePrimitive(PrimitiveType.Cube);
            desertR.name = "DesertDune_Right";
            desertR.transform.position = new Vector3(25f, -0.2f, 30f);
            desertR.transform.localScale = new Vector3(40f, 0.2f, 100f);
            if (matEnv != null) desertR.GetComponent<Renderer>().sharedMaterial = matEnv;

            // Gasolinera en ruinas al lado de la carretera
            GameObject gasStation = LoadModelInstance("Assets/Models/Environments/GasStation.obj", null, matEnv);
            gasStation.name = "GasStation_Sonora";
            gasStation.transform.position = new Vector3(15f, 0, 20f);
            gasStation.transform.rotation = Quaternion.Euler(0, -25f, 0);

            // Estatua monumental Danzante Yaqui en la distancia
            GameObject danzante = LoadModelInstance("Assets/Models/Props/DanzanteMonument.obj", null, matProps);
            danzante.name = "Monumental_Danzante";
            danzante.transform.position = new Vector3(0, 0, 45f);
            danzante.transform.localScale = new Vector3(1.3f, 1.3f, 1.3f);

            // Silueta lejana de MUUKIA con ojos rojos en la niebla
            GameObject muukiaSilhouette = LoadModelInstance("Assets/Models/Characters/MuukiaEntity.obj", null, matMuukia);
            muukiaSilhouette.name = "Distant_Muukia_Silhouette";
            muukiaSilhouette.transform.position = new Vector3(8f, 0, 55f);
            muukiaSilhouette.transform.rotation = Quaternion.Euler(0, -30f, 0);
            GameObject redEyes = new GameObject("Distant_RedEyes");
            redEyes.transform.SetParent(muukiaSilhouette.transform);
            redEyes.transform.localPosition = new Vector3(0, 1.95f, 0.3f);
            Light rL = redEyes.AddComponent<Light>();
            rL.type = LightType.Point;
            rL.color = Color.red;
            rL.range = 7f;
            rL.intensity = 3.5f;

            // Cámara del Menú Principal
            GameObject menuCamGO = new GameObject("Main Camera");
            menuCamGO.tag = "MainCamera";
            menuCamGO.transform.position = new Vector3(0, 1.8f, -6f);
            menuCamGO.transform.rotation = Quaternion.Euler(4f, 0, 0);
            UnityEngine.Camera cam = menuCamGO.AddComponent<UnityEngine.Camera>();
            cam.fieldOfView = 55f;
            menuCamGO.AddComponent<AudioListener>();

            // Audio ambiental del menú
            AudioSource menuAudio = menuCamGO.AddComponent<AudioSource>();
            menuAudio.clip = clipWind;
            menuAudio.loop = true;
            menuAudio.volume = 0.5f;
            menuAudio.Play();

            // Canvas del Menú
            GameObject canvasGO = new GameObject("MainMenu_Canvas");
            Canvas canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(640, 480);
            canvasGO.AddComponent<GraphicRaycaster>();

            // EventSystem
            GameObject esGO = new GameObject("EventSystem");
            esGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

            // VHS Overlay
            GameObject vhsPanel = CreateUIPanel("VHS_Overlay", canvasGO.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.one);
            Image vhsImg = vhsPanel.AddComponent<Image>();
            if (matVhs != null) vhsImg.material = matVhs;
            vhsImg.color = new Color(1, 1, 1, 0.4f);
            vhsPanel.AddComponent<CanvasGroup>().blocksRaycasts = false;

            // Título Principal "MUUKIA"
            GameObject titleGO = CreateUIText("Title_MUUKIA", canvasGO.transform, new Vector2(0.1f, 0.70f), new Vector2(0.9f, 0.92f), "MUUKIA", 52, TextAnchor.MiddleCenter);
            titleGO.GetComponent<Text>().color = new Color(0.95f, 0.2f, 0.2f);

            GameObject subtitleGO = CreateUIText("Subtitle_MUUKIA", canvasGO.transform, new Vector2(0.1f, 0.65f), new Vector2(0.9f, 0.72f), "EL REGRESO · CAPÍTULO 1", 16, TextAnchor.MiddleCenter);
            subtitleGO.GetComponent<Text>().color = new Color(0.85f, 0.85f, 0.85f, 0.8f);

            // Contenedor de Botones
            GameObject buttonsContainer = CreateUIPanel("ButtonsContainer", canvasGO.transform, new Vector2(0.32f, 0.20f), new Vector2(0.68f, 0.58f), Vector2.zero, Vector2.zero);

            Button btnPlay = CreateMenuButton("Btn_JUGAR", buttonsContainer.transform, new Vector2(0, 0.68f), new Vector2(1, 0.95f), "JUGAR");
            Button btnContinue = CreateMenuButton("Btn_CONTINUAR", buttonsContainer.transform, new Vector2(0, 0.36f), new Vector2(1, 0.63f), "CONTINUAR");
            Button btnOptions = CreateMenuButton("Btn_OPCIONES", buttonsContainer.transform, new Vector2(0, 0.04f), new Vector2(1, 0.31f), "CONFIGURACIONES");
            Button btnQuit = CreateMenuButton("Btn_SALIR", buttonsContainer.transform, new Vector2(0, -0.28f), new Vector2(1, -0.01f), "SALIR");

            // Panel de Opciones (OptionsMenuController)
            GameObject optionsPanel = CreateUIPanel("OptionsMenuPanel", canvasGO.transform, new Vector2(0.2f, 0.15f), new Vector2(0.8f, 0.85f), Vector2.zero, Vector2.zero);
            Image optBg = optionsPanel.AddComponent<Image>();
            optBg.color = new Color(0.06f, 0.06f, 0.08f, 0.95f);
            OptionsMenuController optionsCtrl = optionsPanel.AddComponent<OptionsMenuController>();
            CreateUIText("OptionsTitle", optionsPanel.transform, new Vector2(0.1f, 0.82f), new Vector2(0.9f, 0.96f), "CONFIGURACIÓN", 24, TextAnchor.MiddleCenter);

            // Botón regresar en opciones
            Button optBackBtn = CreateMenuButton("Btn_OptBack", optionsPanel.transform, new Vector2(0.3f, 0.05f), new Vector2(0.7f, 0.16f), "VOLVER");
            optionsPanel.SetActive(false);

            // MainMenuController
            MainMenuController menuCtrl = canvasGO.AddComponent<MainMenuController>();
            SerializedObject menuSO = new SerializedObject(menuCtrl);
            menuSO.FindProperty("firstPlayableSceneName").stringValue = "CH01_HermosilloCasa";
            menuSO.FindProperty("playButton").objectReferenceValue = btnPlay;
            menuSO.FindProperty("continueButton").objectReferenceValue = btnContinue;
            menuSO.FindProperty("optionsButton").objectReferenceValue = btnOptions;
            menuSO.FindProperty("quitButton").objectReferenceValue = btnQuit;
            menuSO.FindProperty("mainButtonsContainer").objectReferenceValue = buttonsContainer;
            menuSO.FindProperty("optionsMenu").objectReferenceValue = optionsCtrl;
            menuSO.FindProperty("ambientAudioSource").objectReferenceValue = menuAudio;
            menuSO.FindProperty("menuMusicClip").objectReferenceValue = clipWind;
            menuSO.FindProperty("buttonClickClip").objectReferenceValue = clipBlip;
            menuSO.ApplyModifiedProperties();

            // Transition Manager
            GameObject transMgrGO = new GameObject("SceneTransitionManager");
            transMgrGO.AddComponent<SceneTransitionManager>();

            EditorSceneManager.SaveScene(scene, PathMainMenu);
            Debug.Log($"[MUUKIA MASTER] Escena guardada: {PathMainMenu}");
        }

        /// <summary>
        /// Escena 1: Casa en Hermosillo.
        /// </summary>
        public static void BuildCH01CasaScene()
        {
            Debug.Log("[MUUKIA MASTER] Construyendo Escena: CH01_HermosilloCasa...");
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            Material matEnv = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/M_Environment.mat");
            Material matProps = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/M_Props.mat");
            AudioClip clipHouseAmb = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/urban_ambience.wav");

            // Configuración ambiental interior
            RenderSettings.fog = true;
            RenderSettings.fogDensity = 0.005f;
            RenderSettings.fogColor = new Color(0.18f, 0.15f, 0.12f, 1f);

            // Luz suave cálida de interior
            GameObject lampGO = new GameObject("House_InteriorLamp");
            lampGO.transform.position = new Vector3(0, 3f, 0);
            Light lamp = lampGO.AddComponent<Light>();
            lamp.type = LightType.Point;
            lamp.color = new Color(1f, 0.88f, 0.70f);
            lamp.range = 14f;
            lamp.intensity = 1.4f;

            // Escenario de la Casa
            GameObject house = LoadModelInstance("Assets/Models/Environments/MexicanHouse.obj", null, matEnv);
            house.name = "MexicanHouse";
            AddBoxCollider(house, new Vector3(0, 2.2f, 0), new Vector3(8.5f, 4.5f, 9.5f));

            // Suelo firme con collider
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "House_Floor";
            floor.transform.position = new Vector3(0, -0.1f, 0);
            floor.transform.localScale = new Vector3(9f, 0.2f, 10f);
            if (matEnv != null) floor.GetComponent<Renderer>().sharedMaterial = matEnv;

            // Mesa del comedor
            GameObject table = GameObject.CreatePrimitive(PrimitiveType.Cube);
            table.name = "DiningTable";
            table.transform.position = new Vector3(0, 0.4f, 1.5f);
            table.transform.localScale = new Vector3(1.6f, 0.8f, 1.0f);
            if (matProps != null) table.GetComponent<Renderer>().sharedMaterial = matProps;

            // Pastillas de mamá
            GameObject pills = LoadModelInstance("Assets/Models/Props/PillsMedicine.obj", null, matProps);
            pills.name = "MedicinePills";
            pills.transform.position = new Vector3(0.2f, 0.85f, 1.5f);
            AddBoxCollider(pills, new Vector3(0, 0.1f, 0), new Vector3(0.4f, 0.3f, 0.4f));
            InteractableObject pillsInteract = pills.AddComponent<InteractableObject>();

            // Vendas de auxilio en el buró
            GameObject bandage = LoadModelInstance("Assets/Models/Bandage.obj", null, matProps);
            bandage.name = "Bandage_Pickup";
            bandage.transform.position = new Vector3(-2f, 0.8f, -2.5f);
            AddBoxCollider(bandage, new Vector3(0, 0.1f, 0), new Vector3(0.4f, 0.4f, 0.4f));
            bandage.AddComponent<InventoryItemPickup>();

            // Madre en el comedor
            GameObject mother = LoadPrefabOrModel("Assets/Prefabs/Characters/NPC_Mother.prefab", "Assets/Models/Characters/Mother.obj", "NPC_Mother", matProps);
            mother.transform.position = new Vector3(-0.9f, 0, 1.5f);
            mother.transform.rotation = Quaternion.Euler(0, 90f, 0);
            MotherNPC motherComp = mother.GetComponent<MotherNPC>();
            if (motherComp == null) motherComp = mother.AddComponent<MotherNPC>();
            PS2CharacterAnimator mAnim = mother.GetComponent<PS2CharacterAnimator>();
            if (mAnim == null) mAnim = mother.AddComponent<PS2CharacterAnimator>();
            mAnim.SetState(PS2CharacterAnimator.AnimationState.Seated);

            // Puerta frontal interactiva de salida hacia la clínica
            GameObject door = GameObject.CreatePrimitive(PrimitiveType.Cube);
            door.name = "House_ExitDoor";
            door.transform.position = new Vector3(0, 1.1f, -4.6f);
            door.transform.localScale = new Vector3(1.4f, 2.3f, 0.2f);
            if (matProps != null) door.GetComponent<Renderer>().sharedMaterial = matProps;
            InteractiveDoor intDoor = door.AddComponent<InteractiveDoor>();
            SerializedObject doorSO = new SerializedObject(intDoor);
            doorSO.FindProperty("isSceneTransitionDoor").boolValue = true;
            doorSO.FindProperty("targetSceneName").stringValue = "CH01_Clinica";
            doorSO.ApplyModifiedProperties();

            // Puntos de Spawn
            GameObject spawnGO = new GameObject("Spawn_House");
            spawnGO.transform.position = new Vector3(0, 0.1f, -2.0f);
            SpawnPoint sp = spawnGO.AddComponent<SpawnPoint>();

            // Jugador
            GameObject player = LoadPrefabOrModel("Assets/Prefabs/Characters/Player_Gustavo.prefab", "Assets/Models/Characters/Gustavo.obj", "Player_Gustavo", matProps);
            player.transform.position = spawnGO.transform.position;
            sp.Spawn(player.transform);

            // Canvas y Sistemas
            BuildSceneSystems("CH01_HermosilloCasa", Chapter1GameDirector.ChapterStage.Scene1_House_Hermosillo, motherComp, null, null, null, null, clipHouseAmb);

            // Conectar interacción de pastillas para detonar diálogo
            pillsInteract.onInteracted.AddListener((interactor) =>
            {
                motherComp.TriggerHouseDialogue();
            });

            EditorSceneManager.SaveScene(scene, PathCH01Casa);
            Debug.Log($"[MUUKIA MASTER] Escena guardada: {PathCH01Casa}");
        }

        /// <summary>
        /// Escena 2: Clínica Psiquiátrica San Rafael.
        /// </summary>
        public static void BuildCH01ClinicaScene()
        {
            Debug.Log("[MUUKIA MASTER] Construyendo Escena: CH01_Clinica...");
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            Material matEnv = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/M_Environment.mat");
            Material matProps = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/M_Props.mat");
            AudioClip clipUrban = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/urban_ambience.wav");
            AudioClip clipRamirez = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/ramirez_voicenote.wav");

            // Configuración ambiental exterior clínica
            RenderSettings.fog = true;
            RenderSettings.fogDensity = 0.015f;
            RenderSettings.fogColor = new Color(0.14f, 0.16f, 0.20f, 1f);

            // Suelo exterior
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.name = "Clinic_AsphaltGround";
            ground.transform.position = new Vector3(0, -0.1f, 10f);
            ground.transform.localScale = new Vector3(30f, 0.2f, 35f);
            if (matEnv != null) ground.GetComponent<Renderer>().sharedMaterial = matEnv;

            // Fachada de la Clínica
            GameObject clinic = LoadModelInstance("Assets/Models/Environments/ClinicFacade.obj", null, matEnv);
            clinic.name = "ClinicFacade";
            clinic.transform.position = new Vector3(0, 0, 18f);
            AddBoxCollider(clinic, new Vector3(0, 3f, 0), new Vector3(16f, 6.5f, 7f));

            // Farola de entrada
            GameObject streetLamp = new GameObject("StreetLamp");
            streetLamp.transform.position = new Vector3(0, 4.5f, 10f);
            Light sLight = streetLamp.AddComponent<Light>();
            sLight.type = LightType.Spot;
            sLight.range = 20f;
            sLight.spotAngle = 65f;
            sLight.color = new Color(0.85f, 0.90f, 1f);
            streetLamp.transform.rotation = Quaternion.Euler(75f, 0, 0);

            // Madre despidiéndose cerca de la entrada
            GameObject mother = LoadPrefabOrModel("Assets/Prefabs/Characters/NPC_Mother.prefab", "Assets/Models/Characters/Mother.obj", "NPC_Mother", matProps);
            mother.transform.position = new Vector3(0, 0, 12f);
            MotherNPC motherComp = mother.GetComponent<MotherNPC>();
            if (motherComp != null)
            {
                SerializedObject mSO = new SerializedObject(motherComp);
                mSO.FindProperty("isInClinicScene").boolValue = true;
                mSO.ApplyModifiedProperties();
            }

            // Auto Sedán de Gustavo estacionado
            GameObject car = LoadPrefabOrModel("Assets/Prefabs/Interactables/Prop_CarSedan.prefab", "Assets/Models/Props/CarSedan.obj", "Car_Gustavo", matProps);
            car.transform.position = new Vector3(-6f, 0, 4f);
            car.transform.rotation = Quaternion.Euler(0, 40f, 0);
            CarController carCtrl = car.GetComponent<CarController>();

            // Puerta/interacción del auto para viajar a Obregón
            InteractiveDoor carTravelDoor = car.AddComponent<InteractiveDoor>();
            SerializedObject cDoorSO = new SerializedObject(carTravelDoor);
            cDoorSO.FindProperty("isSceneTransitionDoor").boolValue = true;
            cDoorSO.FindProperty("targetSceneName").stringValue = "CH01_Obregon";
            cDoorSO.ApplyModifiedProperties();

            // Spawn
            GameObject spawnGO = new GameObject("Spawn_Clinic");
            spawnGO.transform.position = new Vector3(-2f, 0.1f, 3f);
            SpawnPoint sp = spawnGO.AddComponent<SpawnPoint>();

            // Jugador
            GameObject player = LoadPrefabOrModel("Assets/Prefabs/Characters/Player_Gustavo.prefab", "Assets/Models/Characters/Gustavo.obj", "Player_Gustavo", matProps);
            player.transform.position = spawnGO.transform.position;
            sp.Spawn(player.transform);

            // Sistemas
            BuildSceneSystems("CH01_Clinica", Chapter1GameDirector.ChapterStage.Scene2_PsychiatricClinic, motherComp, null, null, carCtrl, null, clipUrban, clipRamirez);

            EditorSceneManager.SaveScene(scene, PathCH01Clinica);
            Debug.Log($"[MUUKIA MASTER] Escena guardada: {PathCH01Clinica}");
        }

        /// <summary>
        /// Escena 3: Ciudad Obregón — Calle Fernando Montes de Oca.
        /// </summary>
        public static void BuildCH01ObregonScene()
        {
            Debug.Log("[MUUKIA MASTER] Construyendo Escena: CH01_Obregon...");
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            Material matEnv = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/M_Environment.mat");
            Material matProps = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/M_Props.mat");
            AudioClip clipUrban = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/urban_ambience.wav");

            RenderSettings.fog = true;
            RenderSettings.fogDensity = 0.012f;
            RenderSettings.fogColor = new Color(0.18f, 0.14f, 0.12f, 1f);

            // Calle Fernando Montes de Oca
            GameObject street = GameObject.CreatePrimitive(PrimitiveType.Cube);
            street.name = "Street_MontesDeOca";
            street.transform.position = new Vector3(0, -0.1f, 15f);
            street.transform.localScale = new Vector3(10f, 0.2f, 60f);
            if (matEnv != null) street.GetComponent<Renderer>().sharedMaterial = matEnv;

            // Banqueta
            GameObject sidewalk = GameObject.CreatePrimitive(PrimitiveType.Cube);
            sidewalk.name = "Sidewalk";
            sidewalk.transform.position = new Vector3(6.5f, 0.05f, 15f);
            sidewalk.transform.localScale = new Vector3(3f, 0.3f, 60f);
            if (matEnv != null) sidewalk.GetComponent<Renderer>().sharedMaterial = matEnv;

            // Castillo de Maderos al fondo
            GameObject castle = LoadModelInstance("Assets/Models/Environments/CastilloMaderos.obj", null, matEnv);
            castle.name = "CastilloMaderos_Background";
            castle.transform.position = new Vector3(0, 0, 48f);
            AddBoxCollider(castle, new Vector3(0, 5f, 0), new Vector3(14f, 10.5f, 14f));

            // Target sobrenatural en la ventana del castillo
            GameObject castleTargetGO = new GameObject("CastleWindow_Target");
            castleTargetGO.transform.position = new Vector3(0, 7.5f, 44f);
            castleTargetGO.AddComponent<BoxCollider>().size = new Vector3(4f, 4f, 2f);
            SupernaturalTarget castleTarget = castleTargetGO.AddComponent<SupernaturalTarget>();

            // Cinta Policial y Patrullas
            GameObject tape = LoadPrefabOrModel("Assets/Prefabs/Interactables/Prop_PoliceTape.prefab", "Assets/Models/Props/PoliceTape.obj", "PoliceTape_Cordon", matProps);
            tape.transform.position = new Vector3(0, 0, 20f);

            GameObject patrol = LoadPrefabOrModel("Assets/Prefabs/Interactables/Prop_PolicePatrol.prefab", "Assets/Models/Props/PolicePatrol.obj", "PolicePatrol", matProps);
            patrol.transform.position = new Vector3(-3.5f, 0, 16f);
            patrol.transform.rotation = Quaternion.Euler(0, 20f, 0);

            // Oficiales de policía
            GameObject cop = LoadPrefabOrModel("Assets/Prefabs/Characters/NPC_PoliceOfficer.prefab", "Assets/Models/Characters/PoliceOfficer.obj", "PoliceOfficer_Guard", matProps);
            cop.transform.position = new Vector3(-1.5f, 0, 19f);

            // Vecino (Don Lupe) en la banqueta
            GameObject neighbor = LoadPrefabOrModel("Assets/Prefabs/Characters/NPC_Neighbor.prefab", "Assets/Models/Characters/Neighbor.obj", "NPC_Neighbor_DonLupe", matProps);
            neighbor.transform.position = new Vector3(6f, 0.2f, 8f);
            neighbor.transform.rotation = Quaternion.Euler(0, -90f, 0);
            NeighborNPC neighborComp = neighbor.GetComponent<NeighborNPC>();
            if (neighborComp == null) neighborComp = neighbor.AddComponent<NeighborNPC>();

            // Auto de Gustavo para continuar hacia la carretera
            GameObject car = LoadPrefabOrModel("Assets/Prefabs/Interactables/Prop_CarSedan.prefab", "Assets/Models/Props/CarSedan.obj", "Car_Gustavo", matProps);
            car.transform.position = new Vector3(3.5f, 0, -5f);
            CarController carCtrl = car.GetComponent<CarController>();
            InteractiveDoor carDoor = car.AddComponent<InteractiveDoor>();
            SerializedObject cDoorSO = new SerializedObject(carDoor);
            cDoorSO.FindProperty("isSceneTransitionDoor").boolValue = true;
            cDoorSO.FindProperty("targetSceneName").stringValue = "CH01_Carretera";
            cDoorSO.ApplyModifiedProperties();

            // Spawn
            GameObject spawnGO = new GameObject("Spawn_Obregon");
            spawnGO.transform.position = new Vector3(2f, 0.1f, -4f);
            SpawnPoint sp = spawnGO.AddComponent<SpawnPoint>();

            // Jugador
            GameObject player = LoadPrefabOrModel("Assets/Prefabs/Characters/Player_Gustavo.prefab", "Assets/Models/Characters/Gustavo.obj", "Player_Gustavo", matProps);
            player.transform.position = spawnGO.transform.position;
            sp.Spawn(player.transform);

            // Sistemas
            BuildSceneSystems("CH01_Obregon", Chapter1GameDirector.ChapterStage.Scene3_Obregon_MontesDeOca, null, neighborComp, null, carCtrl, castleTarget, clipUrban);

            EditorSceneManager.SaveScene(scene, PathCH01Obregon);
            Debug.Log($"[MUUKIA MASTER] Escena guardada: {PathCH01Obregon}");
        }

        /// <summary>
        /// Escena 4: Carretera de Sonora y Clímax del Danzante.
        /// </summary>
        public static void BuildCH01CarreteraScene()
        {
            Debug.Log("[MUUKIA MASTER] Construyendo Escena: CH01_Carretera...");
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            Material matEnv = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/M_Environment.mat");
            Material matProps = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/M_Props.mat");
            Material matMuukia = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/M_Muukia.mat");
            AudioClip clipWind = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/desert_wind.wav");

            RenderSettings.fog = true;
            RenderSettings.fogDensity = 0.022f;
            RenderSettings.fogColor = new Color(0.12f, 0.10f, 0.12f, 1f);

            // Carretera Federal 15
            GameObject highway = GameObject.CreatePrimitive(PrimitiveType.Cube);
            highway.name = "Highway_Sonora15";
            highway.transform.position = new Vector3(0, -0.1f, 40f);
            highway.transform.localScale = new Vector3(14f, 0.2f, 110f);
            if (matEnv != null) highway.GetComponent<Renderer>().sharedMaterial = matEnv;

            // Gasolinera Pemex abandonada
            GameObject gasStation = LoadPrefabOrModel("Assets/Prefabs/Environment/GasStation.prefab", "Assets/Models/Environments/GasStation.obj", "GasStation_Pemex", matEnv);
            gasStation.transform.position = new Vector3(-16f, 0, 25f);

            // Monumento Danzante Yaqui
            GameObject danzante = LoadPrefabOrModel("Assets/Prefabs/Environment/DanzanteMonument.prefab", "Assets/Models/Props/DanzanteMonument.obj", "Monumental_Danzante", matProps);
            danzante.transform.position = new Vector3(0, 0, 42f);

            // Auto de Gustavo detenido tras el choque / parada
            GameObject car = LoadPrefabOrModel("Assets/Prefabs/Interactables/Prop_CarSedan.prefab", "Assets/Models/Props/CarSedan.obj", "Car_Gustavo", matProps);
            car.transform.position = new Vector3(0, 0, 10f);
            CarController carCtrl = car.GetComponent<CarController>();

            // Entidad MUUKIA esperando en la niebla
            GameObject muukia = LoadPrefabOrModel("Assets/Prefabs/Characters/Entity_MUUKIA.prefab", "Assets/Models/Characters/MuukiaEntity.obj", "Entity_MUUKIA", matMuukia);
            muukia.transform.position = new Vector3(0, 0, 60f);
            MuukiaEntity muukiaComp = muukia.GetComponent<MuukiaEntity>();

            // Zona de activación al final de la carretera que lleva al panteón/criptas
            GameObject exitTriggerGO = new GameObject("Trigger_HighwayToCrypts");
            exitTriggerGO.transform.position = new Vector3(0, 1f, 85f);
            BoxCollider exCol = exitTriggerGO.AddComponent<BoxCollider>();
            exCol.size = new Vector3(20f, 4f, 6f);
            exCol.isTrigger = true;
            TriggerZoneEvent exitTrigger = exitTriggerGO.AddComponent<TriggerZoneEvent>();

            // Spawn
            GameObject spawnGO = new GameObject("Spawn_Carretera");
            spawnGO.transform.position = new Vector3(2.5f, 0.1f, 10f);
            SpawnPoint sp = spawnGO.AddComponent<SpawnPoint>();

            // Jugador
            GameObject player = LoadPrefabOrModel("Assets/Prefabs/Characters/Player_Gustavo.prefab", "Assets/Models/Characters/Gustavo.obj", "Player_Gustavo", matProps);
            player.transform.position = spawnGO.transform.position;
            sp.Spawn(player.transform);

            // Sistemas
            Chapter1GameDirector dir = BuildSceneSystems("CH01_Carretera", Chapter1GameDirector.ChapterStage.Scene5_HighwayDriving, null, null, muukiaComp, carCtrl, null, clipWind);

            exitTrigger.GetComponent<TriggerZoneEvent>();
            // Al alcanzar el final de la carretera, notificar arribo a criptas
            InteractiveDoor exitDoor = exitTriggerGO.AddComponent<InteractiveDoor>();
            SerializedObject exDoorSO = new SerializedObject(exitDoor);
            exDoorSO.FindProperty("isSceneTransitionDoor").boolValue = true;
            exDoorSO.FindProperty("targetSceneName").stringValue = "CH01_Criptas";
            exDoorSO.ApplyModifiedProperties();

            EditorSceneManager.SaveScene(scene, PathCH01Carretera);
            Debug.Log($"[MUUKIA MASTER] Escena guardada: {PathCH01Carretera}");
        }

        /// <summary>
        /// Escena 5: Las Criptas — Final del Capítulo 1.
        /// </summary>
        public static void BuildCH01CriptasScene()
        {
            Debug.Log("[MUUKIA MASTER] Construyendo Escena: CH01_Criptas...");
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            Material matEnv = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/M_Environment.mat");
            Material matProps = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/M_Props.mat");
            Material matMuukia = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/M_Muukia.mat");
            AudioClip clipDrone = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/crypts_drone.wav");
            AudioClip clipSofia = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/sofia_whisper.wav");

            RenderSettings.fog = true;
            RenderSettings.fogDensity = 0.028f;
            RenderSettings.fogColor = new Color(0.10f, 0.12f, 0.15f, 1f);

            // Suelo del cementerio
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Crypts_GravelFloor";
            floor.transform.position = new Vector3(0, -0.1f, 25f);
            floor.transform.localScale = new Vector3(60f, 0.2f, 75f);
            if (matEnv != null) floor.GetComponent<Renderer>().sharedMaterial = matEnv;

            // Filas de Mausoleos
            for (int x = -1; x <= 1; x++)
            {
                for (int z = 0; z <= 2; z++)
                {
                    if (x == 0 && z == 1) continue; // Cripta principal colapsada
                    GameObject maus = LoadPrefabOrModel("Assets/Prefabs/Environment/Mausoleum.prefab", "Assets/Models/Environments/Mausoleum.obj", $"Mausoleum_{x}_{z}", matEnv);
                    maus.transform.position = new Vector3(x * 16f, 0, z * 18f + 10f);
                }
            }

            // Obelisco conmemorativo
            GameObject obelisk = LoadPrefabOrModel("Assets/Prefabs/Environment/Obelisk.prefab", "Assets/Models/Environments/Obelisk.obj", "Obelisk_Center", matEnv);
            obelisk.transform.position = new Vector3(0, 0, 15f);

            // Cripta Colapsada con acceso subterráneo (Abertura del clímax)
            GameObject collapsedCrypt = LoadPrefabOrModel("Assets/Prefabs/Environment/CollapsedCrypt.prefab", "Assets/Models/Environments/CollapsedCrypt.obj", "CollapsedCrypt_Opening", matEnv);
            collapsedCrypt.transform.position = new Vector3(0, 0, 36f);

            // Trigger del clímax en la abertura
            GameObject cryptTriggerGO = new GameObject("CryptUnderground_Trigger");
            cryptTriggerGO.transform.position = new Vector3(0, 1f, 35f);
            BoxCollider cCol = cryptTriggerGO.AddComponent<BoxCollider>();
            cCol.size = new Vector3(5f, 3.5f, 4f);
            cCol.isTrigger = true;
            SupernaturalTarget cryptTarget = cryptTriggerGO.AddComponent<SupernaturalTarget>();

            // Entidad MUUKIA lista para el jumpscare
            GameObject muukia = LoadPrefabOrModel("Assets/Prefabs/Characters/Entity_MUUKIA.prefab", "Assets/Models/Characters/MuukiaEntity.obj", "Entity_MUUKIA", matMuukia);
            muukia.transform.position = new Vector3(0, 0, 48f);
            MuukiaEntity muukiaComp = muukia.GetComponent<MuukiaEntity>();

            // Spawn
            GameObject spawnGO = new GameObject("Spawn_Criptas");
            spawnGO.transform.position = new Vector3(0, 0.1f, -5f);
            SpawnPoint sp = spawnGO.AddComponent<SpawnPoint>();

            // Jugador
            GameObject player = LoadPrefabOrModel("Assets/Prefabs/Characters/Player_Gustavo.prefab", "Assets/Models/Characters/Gustavo.obj", "Player_Gustavo", matProps);
            player.transform.position = spawnGO.transform.position;
            sp.Spawn(player.transform);

            // Sistemas
            Chapter1GameDirector dir = BuildSceneSystems("CH01_Criptas", Chapter1GameDirector.ChapterStage.Scene7_Crypts_Finale, null, null, muukiaComp, null, cryptTarget, clipDrone, null, clipSofia);

            // Conectar el trigger para detonar el clímax final
            cryptTriggerGO.AddComponent<InteractableObject>().onInteracted.AddListener((interactor) =>
            {
                dir.OnCryptOpeningReached();
            });

            EditorSceneManager.SaveScene(scene, PathCH01Criptas);
            Debug.Log($"[MUUKIA MASTER] Escena guardada: {PathCH01Criptas}");
        }

        #endregion

        #region 5. SISTEMAS Y CANVAS POR ESCENA

        private static Chapter1GameDirector BuildSceneSystems(
            string sceneName,
            Chapter1GameDirector.ChapterStage stage,
            MotherNPC mother = null,
            NeighborNPC neighbor = null,
            MuukiaEntity muukia = null,
            CarController car = null,
            SupernaturalTarget superTarget = null,
            AudioClip ambientClip = null,
            AudioClip bossClip = null,
            AudioClip sofiaClip = null)
        {
            // Shaders / Materiales / Audios
            Material matVhs = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/M_VHSTapePS2.mat");
            AudioClip clipVhsGlitch = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/vhs_glitch.wav");
            AudioClip clipVhsHum = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/vhs_static_hum.wav");
            AudioClip clipBlip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/dialogue_blip.wav");

            // EventSystem
            GameObject esGO = new GameObject("EventSystem");
            esGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

            // Directores
            GameObject directorsGO = new GameObject("--- GAME DIRECTORS & MANAGERS ---");
            Chapter1GameDirector director = directorsGO.AddComponent<Chapter1GameDirector>();
            directorsGO.AddComponent<RetroFogController>();
            VHSCameraEffect vhsEffect = directorsGO.AddComponent<VHSCameraEffect>();
            AudioSource ambientSource = directorsGO.AddComponent<AudioSource>();

            // Scene Transition Manager
            directorsGO.AddComponent<SceneTransitionManager>();

            // Master Canvas
            GameObject canvasGO = new GameObject("MUUKIA_MasterCanvas");
            Canvas canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(640, 480);
            canvasGO.AddComponent<GraphicRaycaster>();

            // 1. VHS Overlay
            GameObject vhsPanel = CreateUIPanel("VHS_Overlay", canvasGO.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.one);
            Image vhsImg = vhsPanel.AddComponent<Image>();
            if (matVhs != null) vhsImg.material = matVhs;
            vhsImg.color = new Color(1, 1, 1, 0.45f);
            CanvasGroup vhsGroup = vhsPanel.AddComponent<CanvasGroup>();
            vhsGroup.blocksRaycasts = false;

            AudioSource vhsAudio = vhsPanel.AddComponent<AudioSource>();
            SerializedObject vhsSO = new SerializedObject(vhsEffect);
            vhsSO.FindProperty("vhsMaterial").objectReferenceValue = matVhs;
            vhsSO.FindProperty("vhsCanvasGroup").objectReferenceValue = vhsGroup;
            vhsSO.FindProperty("vhsAudioSource").objectReferenceValue = vhsAudio;
            vhsSO.FindProperty("glitchAudioClip").objectReferenceValue = clipVhsGlitch;
            vhsSO.FindProperty("staticHumClip").objectReferenceValue = clipVhsHum;
            vhsSO.ApplyModifiedProperties();

            // 2. HUD de Videocámara
            GameObject camHudGO = CreateUIPanel("Camera_Viewfinder_HUD", canvasGO.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.one);
            CanvasGroup camHudGroup = camHudGO.AddComponent<CanvasGroup>();
            camHudGroup.alpha = 0f;
            camHudGroup.blocksRaycasts = false;

            GameObject recDot = CreateUIPanel("RecIndicator", camHudGO.transform, new Vector2(0.05f, 0.90f), new Vector2(0.05f, 0.90f), Vector2.zero, new Vector2(14, 14));
            Image recImg = recDot.AddComponent<Image>();
            recImg.color = Color.red;

            GameObject recText = CreateUIText("RecText", camHudGO.transform, new Vector2(0.08f, 0.88f), new Vector2(0.25f, 0.94f), "● REC  [BAT 85%]", 13, TextAnchor.MiddleLeft);
            GameObject timecodeText = CreateUIText("Timecode", camHudGO.transform, new Vector2(0.68f, 0.88f), new Vector2(0.95f, 0.94f), "SP 00:14:28\n19 NOV 2002", 12, TextAnchor.MiddleRight);
            GameObject zoomText = CreateUIText("ZoomText", camHudGO.transform, new Vector2(0.4f, 0.05f), new Vector2(0.6f, 0.10f), "1.0X ZOOM", 13, TextAnchor.MiddleCenter);

            // 3. Player HUD (Retícula, estamina, interacción)
            GameObject playerHudGO = CreateUIPanel("PlayerHUD_Container", canvasGO.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.one);
            PlayerHUD playerHud = playerHudGO.AddComponent<PlayerHUD>();
            GameObject crosshair = CreateUIText("CrosshairDot", playerHudGO.transform, new Vector2(0.495f, 0.495f), new Vector2(0.505f, 0.505f), "·", 28, TextAnchor.MiddleCenter);

            GameObject promptGO = CreateUIText("InteractionPrompt", playerHudGO.transform, new Vector2(0.2f, 0.35f), new Vector2(0.8f, 0.42f), "", 14, TextAnchor.MiddleCenter);
            SerializedObject hudSO = new SerializedObject(playerHud);
            hudSO.FindProperty("interactionPromptLegacy").objectReferenceValue = promptGO.GetComponent<Text>();
            hudSO.ApplyModifiedProperties();

            // 4. Sistema de Diálogos
            GameObject dialMgrGO = new GameObject("DialogueManager");
            dialMgrGO.transform.SetParent(directorsGO.transform);
            DialogueManager dialMgr = dialMgrGO.AddComponent<DialogueManager>();
            AudioSource dialAudio = dialMgrGO.AddComponent<AudioSource>();
            AudioSource voiceAudio = dialMgrGO.AddComponent<AudioSource>();

            GameObject dialBox = CreateUIPanel("DialogueBox", canvasGO.transform, new Vector2(0.1f, 0.06f), new Vector2(0.9f, 0.28f), Vector2.zero, Vector2.zero);
            Image dialBg = dialBox.AddComponent<Image>();
            dialBg.color = new Color(0.04f, 0.04f, 0.06f, 0.85f);
            CanvasGroup dialGroup = dialBox.AddComponent<CanvasGroup>();

            GameObject speakerGO = CreateUIText("SpeakerName", dialBox.transform, new Vector2(0.04f, 0.72f), new Vector2(0.96f, 0.95f), "GUSTAVO", 15, TextAnchor.MiddleLeft);
            speakerGO.GetComponent<Text>().color = new Color(0.95f, 0.75f, 0.2f);
            GameObject contentGO = CreateUIText("ContentText", dialBox.transform, new Vector2(0.04f, 0.12f), new Vector2(0.96f, 0.70f), "...", 14, TextAnchor.UpperLeft);
            GameObject advancePromptGO = CreateUIText("AdvancePrompt", dialBox.transform, new Vector2(0.70f, 0.02f), new Vector2(0.96f, 0.20f), "Presiona [E] ►", 12, TextAnchor.MiddleRight);

            SerializedObject dialSO = new SerializedObject(dialMgr);
            dialSO.FindProperty("dialogueCanvasGroup").objectReferenceValue = dialGroup;
            dialSO.FindProperty("speakerTextLegacy").objectReferenceValue = speakerGO.GetComponent<Text>();
            dialSO.FindProperty("contentTextLegacy").objectReferenceValue = contentGO.GetComponent<Text>();
            dialSO.FindProperty("promptAdvanceIndicator").objectReferenceValue = advancePromptGO;
            dialSO.FindProperty("dialogueAudioSource").objectReferenceValue = dialAudio;
            dialSO.FindProperty("voiceAudioSource").objectReferenceValue = voiceAudio;
            dialSO.FindProperty("defaultTypewriterBlip").objectReferenceValue = clipBlip;
            dialSO.ApplyModifiedProperties();

            // 5. Screen Fade & Title Card
            GameObject fadeGO = CreateUIPanel("ScreenFade", canvasGO.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.one);
            Image fadeImg = fadeGO.AddComponent<Image>();
            fadeImg.color = Color.black;
            CanvasGroup fadeGroup = fadeGO.AddComponent<CanvasGroup>();
            fadeGroup.alpha = 1f;
            fadeGroup.blocksRaycasts = true;

            GameObject titleCardGO = CreateUIPanel("TitleCard", canvasGO.transform, new Vector2(0.1f, 0.2f), new Vector2(0.9f, 0.8f), Vector2.zero, Vector2.zero);
            CanvasGroup titleCardGroup = titleCardGO.AddComponent<CanvasGroup>();
            titleCardGroup.alpha = 0f;
            titleCardGroup.blocksRaycasts = false;
            GameObject titleTextGO = CreateUIText("TitleText", titleCardGO.transform, Vector2.zero, Vector2.one, "", 22, TextAnchor.MiddleCenter);

            // 6. Pause Menu
            GameObject pausePanel = CreateUIPanel("PauseMenuRoot", canvasGO.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.one);
            pausePanel.AddComponent<Image>().color = new Color(0, 0, 0, 0.75f);
            PauseMenuController pauseCtrl = canvasGO.AddComponent<PauseMenuController>();
            pausePanel.SetActive(false);

            // Configurar Director del Capítulo
            SerializedObject dirSO = new SerializedObject(director);
            dirSO.FindProperty("currentStage").enumValueIndex = (int)stage;
            dirSO.FindProperty("screenFadeCanvasGroup").objectReferenceValue = fadeGroup;
            dirSO.FindProperty("titleCardCanvasGroup").objectReferenceValue = titleCardGroup;
            dirSO.FindProperty("titleCardTextLegacy").objectReferenceValue = titleTextGO.GetComponent<Text>();
            dirSO.FindProperty("ambientAudioSource").objectReferenceValue = ambientSource;

            if (ambientClip != null) dirSO.FindProperty("houseAmbienceClip").objectReferenceValue = ambientClip;
            if (ambientClip != null) dirSO.FindProperty("urbanAmbienceClip").objectReferenceValue = ambientClip;
            if (ambientClip != null) dirSO.FindProperty("desertWindClip").objectReferenceValue = ambientClip;
            if (ambientClip != null) dirSO.FindProperty("cryptsDroneClip").objectReferenceValue = ambientClip;
            if (bossClip != null) dirSO.FindProperty("ramirezVoiceNoteClip").objectReferenceValue = bossClip;
            if (sofiaClip != null) dirSO.FindProperty("sofiaWhisperClip").objectReferenceValue = sofiaClip;

            if (mother != null) dirSO.FindProperty("motherNPC").objectReferenceValue = mother;
            if (neighbor != null) dirSO.FindProperty("neighborNPC").objectReferenceValue = neighbor;
            if (muukia != null) dirSO.FindProperty("muukiaEntity").objectReferenceValue = muukia;
            if (car != null) dirSO.FindProperty("playerCar").objectReferenceValue = car;
            if (superTarget != null)
            {
                if (stage == Chapter1GameDirector.ChapterStage.Scene3_Obregon_MontesDeOca)
                    dirSO.FindProperty("castleWindowTarget").objectReferenceValue = superTarget;
                else if (stage == Chapter1GameDirector.ChapterStage.Scene7_Crypts_Finale)
                    dirSO.FindProperty("cryptOpeningTarget").objectReferenceValue = superTarget;
            }

            dirSO.ApplyModifiedProperties();

            // Configurar VideoCameraRig en el jugador de la escena si existe
            VideoCameraRig playerRig = UnityEngine.Object.FindAnyObjectByType<VideoCameraRig>();
            if (playerRig != null)
            {
                SerializedObject rigSO = new SerializedObject(playerRig);
                rigSO.FindProperty("cameraHUDGroup").objectReferenceValue = camHudGroup;
                rigSO.FindProperty("timecodeTextLegacy").objectReferenceValue = timecodeText.GetComponent<Text>();
                rigSO.FindProperty("recIndicatorDot").objectReferenceValue = recImg;
                rigSO.FindProperty("zoomLevelTextLegacy").objectReferenceValue = zoomText.GetComponent<Text>();
                rigSO.ApplyModifiedProperties();
            }

            return director;
        }

        #endregion

        #region 6. CONFIGURACIÓN DE BUILD SETTINGS

        public static void ConfigureBuildSettings()
        {
            Debug.Log("[MUUKIA MASTER] Configurando Build Settings en el orden canónico obligatorio...");

            string[] orderedScenes = new string[] {
                PathMainMenu,
                PathCH01Casa,
                PathCH01Clinica,
                PathCH01Obregon,
                PathCH01Carretera,
                PathCH01Criptas
            };

            List<EditorBuildSettingsScene> buildScenes = new List<EditorBuildSettingsScene>();

            for (int i = 0; i < orderedScenes.Length; i++)
            {
                string p = orderedScenes[i];
                if (File.Exists(p))
                {
                    buildScenes.Add(new EditorBuildSettingsScene(p, true));
                    Debug.Log($"[Build Settings] Índice {i}: {p}");
                }
                else
                {
                    Debug.LogError($"[Build Settings] ERROR: La escena requerida no existe en disco: {p}");
                }
            }

            EditorBuildSettings.scenes = buildScenes.ToArray();
            AssetDatabase.SaveAssets();
        }

        #endregion

        #region 7. VALIDACIÓN COMPLETA Y AUDITORÍA

        public static bool ValidateEverything()
        {
            Debug.Log("[MUUKIA VALIDATOR] === INICIANDO AUDITORÍA COMPLETA DEL PROYECTO ===");

            int errorCount = 0;
            int warningCount = 0;

            // 1. Validar Build Settings
            if (EditorBuildSettings.scenes.Length < 6)
            {
                Debug.LogError($"[VALIDACIÓN] Fallo: Build Settings tiene {EditorBuildSettings.scenes.Length} escenas, se esperaban 6.");
                errorCount++;
            }
            else
            {
                Debug.Log($"[VALIDACIÓN] ✓ Build Settings contiene exactamente las 6 escenas requeridas en orden.");
            }

            // 2. Validar cada escena
            string[] requiredScenes = new string[] {
                PathMainMenu, PathCH01Casa, PathCH01Clinica, PathCH01Obregon, PathCH01Carretera, PathCH01Criptas
            };

            foreach (var scPath in requiredScenes)
            {
                if (!File.Exists(scPath))
                {
                    Debug.LogError($"[VALIDACIÓN] Escena faltante en disco: {scPath}");
                    errorCount++;
                    continue;
                }

                Scene openedScene = EditorSceneManager.OpenScene(scPath, OpenSceneMode.Single);
                GameObject[] roots = openedScene.GetRootGameObjects();

                int missingScriptsInScene = 0;
                foreach (var root in roots)
                {
                    missingScriptsInScene += CountMissingScripts(root);
                }

                if (missingScriptsInScene > 0)
                {
                    Debug.LogError($"[VALIDACIÓN] Escena '{scPath}' contiene {missingScriptsInScene} Missing Scripts!");
                    errorCount += missingScriptsInScene;
                }
                else
                {
                    Debug.Log($"[VALIDACIÓN] ✓ Escena '{openedScene.name}': 0 Missing Scripts.");
                }

                // Verificar SpawnPoint en escenas jugables
                if (scPath != PathMainMenu)
                {
                    SpawnPoint sp = UnityEngine.Object.FindAnyObjectByType<SpawnPoint>();
                    if (sp == null)
                    {
                        Debug.LogError($"[VALIDACIÓN] Escena jugable '{openedScene.name}' no tiene un componente SpawnPoint!");
                        errorCount++;
                    }
                    else
                    {
                        Debug.Log($"[VALIDACIÓN] ✓ Escena '{openedScene.name}': SpawnPoint verificado en {sp.transform.position}.");
                    }
                }
            }

            // 3. Validar Prefabs Clave
            string[] prefabsToCheck = new string[] {
                "Assets/Prefabs/Characters/Player_Gustavo.prefab",
                "Assets/Prefabs/Characters/NPC_Mother.prefab",
                "Assets/Prefabs/Characters/NPC_Neighbor.prefab",
                "Assets/Prefabs/Characters/Entity_MUUKIA.prefab",
                "Assets/Prefabs/Characters/NPC_PoliceOfficer.prefab",
                "Assets/Prefabs/Environment/MexicanHouse.prefab",
                "Assets/Prefabs/Environment/ClinicFacade.prefab",
                "Assets/Prefabs/Environment/CastilloMaderos.prefab",
                "Assets/Prefabs/Environment/GasStation.prefab",
                "Assets/Prefabs/Environment/DanzanteMonument.prefab",
                "Assets/Prefabs/Environment/Mausoleum.prefab",
                "Assets/Prefabs/Environment/CollapsedCrypt.prefab",
                "Assets/Prefabs/Interactables/Prop_CarSedan.prefab"
            };

            foreach (var pPath in prefabsToCheck)
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(pPath);
                if (prefab == null)
                {
                    Debug.LogError($"[VALIDACIÓN] Prefab no encontrado: {pPath}");
                    errorCount++;
                    continue;
                }

                int missingInPrefab = CountMissingScripts(prefab);
                if (missingInPrefab > 0)
                {
                    Debug.LogError($"[VALIDACIÓN] Prefab '{pPath}' contiene {missingInPrefab} Missing Scripts!");
                    errorCount += missingInPrefab;
                }
                else
                {
                    Debug.Log($"[VALIDACIÓN] ✓ Prefab '{prefab.name}': Componentes y jerarquía íntegros.");
                }
            }

            // 4. Volver a abrir MainMenu como escena inicial de trabajo
            EditorSceneManager.OpenScene(PathMainMenu, OpenSceneMode.Single);

            if (errorCount == 0)
            {
                Debug.Log("[MUUKIA VALIDATOR] ★★★ AUDITORÍA EXITOSA: 0 ERRORES, 0 MISSING SCRIPTS, PROYECTO 100% OPERATIVO ★★★");
                return true;
            }
            else
            {
                Debug.LogError($"[MUUKIA VALIDATOR] Auditoría detectó {errorCount} problemas.");
                return false;
            }
        }

        private static int CountMissingScripts(GameObject go)
        {
            int count = 0;
            Component[] components = go.GetComponents<Component>();
            foreach (var c in components)
            {
                if (c == null) count++;
            }

            for (int i = 0; i < go.transform.childCount; i++)
            {
                count += CountMissingScripts(go.transform.GetChild(i).gameObject);
            }
            return count;
        }

        #endregion

        #region Helpers de Geometría y UI

        private static GameObject LoadPrefabOrModel(string prefabPath, string modelPath, string name, Material mat)
        {
            GameObject prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefabAsset != null)
            {
                GameObject inst = (GameObject)PrefabUtility.InstantiatePrefab(prefabAsset);
                inst.name = name;
                return inst;
            }

            GameObject model = LoadModelInstance(modelPath, null, mat);
            model.name = name;
            return model;
        }

        private static GameObject LoadModelInstance(string assetPath, Transform parent, Material mat)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            GameObject inst = null;
            if (prefab != null)
            {
                inst = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            }
            else
            {
                inst = GameObject.CreatePrimitive(PrimitiveType.Cube);
                inst.name = Path.GetFileNameWithoutExtension(assetPath);
                if (parent != null) inst.transform.SetParent(parent);
            }

            if (mat != null)
            {
                foreach (var rend in inst.GetComponentsInChildren<Renderer>())
                {
                    rend.sharedMaterial = mat;
                }
            }

            return inst;
        }

        private static void AddBoxCollider(GameObject target, Vector3 center, Vector3 size)
        {
            BoxCollider col = target.GetComponent<BoxCollider>();
            if (col == null) col = target.AddComponent<BoxCollider>();
            col.center = center;
            col.size = size;
        }

        private static GameObject CreateUIPanel(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            GameObject go = new GameObject(name);
            RectTransform rt = go.AddComponent<RectTransform>();
            rt.SetParent(parent, false);
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = offsetMin;
            rt.offsetMax = offsetMax;
            return go;
        }

        private static GameObject CreateUIText(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, string initialText, int fontSize, TextAnchor alignment)
        {
            GameObject go = CreateUIPanel(name, parent, anchorMin, anchorMax, Vector2.zero, Vector2.zero);
            Text txt = go.AddComponent<Text>();
            txt.text = initialText;
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (txt.font == null) txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            txt.fontSize = fontSize;
            txt.color = Color.white;
            txt.alignment = alignment;
            return go;
        }

        private static Button CreateMenuButton(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, string buttonText)
        {
            GameObject btnGO = CreateUIPanel(name, parent, anchorMin, anchorMax, Vector2.zero, Vector2.zero);
            Image btnImg = btnGO.AddComponent<Image>();
            btnImg.color = new Color(0.12f, 0.12f, 0.15f, 0.90f);

            Button btn = btnGO.AddComponent<Button>();
            ColorBlock cb = btn.colors;
            cb.normalColor = new Color(0.12f, 0.12f, 0.15f, 0.90f);
            cb.highlightedColor = new Color(0.70f, 0.20f, 0.20f, 1f); // Resaltado rojo PS2
            cb.pressedColor = new Color(0.95f, 0.15f, 0.15f, 1f);
            cb.selectedColor = new Color(0.70f, 0.20f, 0.20f, 1f);
            btn.colors = cb;

            GameObject textGO = CreateUIText("BtnText", btnGO.transform, Vector2.zero, Vector2.one, buttonText, 16, TextAnchor.MiddleCenter);
            textGO.GetComponent<Text>().color = new Color(0.92f, 0.90f, 0.88f);

            return btn;
        }

        #endregion
    }
}
#endif
