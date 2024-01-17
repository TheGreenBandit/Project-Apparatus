using Dissonance;
using GameNetcodeStuff;
using HarmonyLib;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

namespace ProjectApparatus
{
    public interface IBindable
    {
        int bind { get; set; }
        bool settingKeybind { get; set; }
    }

    public class Features
    {
        public ExampleFeature examplefeature = new ExampleFeature();
        public class ExampleFeature : IBindable
        {
            //base stuff
            public int bind { get; set; }
            
            public bool settingKeybind { get; set; }
            public bool m_is_enabled = false;

            //feature specific
            public int value = 0;

            public void OnInit()
            {
                // add stuff to change when starting, useful for storing original value of stuff
            }
            public void OnUpdate()
            {
                if (PAUtils.GetAsyncKeyState(Features.Instance.examplefeature.bind) != 0) //todo make this work? 
                {
                    if (m_is_enabled)
                    {
                        m_is_enabled = false;
                        OnDisable();
                    }
                    else
                    {
                        m_is_enabled = true;
                        OnInit();
                    }
                }

                //add what you want the feature to do here, for example this feature is looping a value and resetting it once it reaches. 

                if (!m_is_enabled)
                    return;

                if (value < 255) //this function works but for some reason the bind dont wanna so u cant turn it on
                    value++;
                else
                    value = 0;

            }
            public void OnDisable()
            {
                // add stuff to change when disabling, useful for restoring original value of stuff
            }
        }

        public class ExampleButton : IBindable
        {
            //base stuff
            public int bind { get; set; }
            public bool settingKeybind { get; set; }
            public static ExampleButton Instance
            {
                get
                {
                    if (instance == null)
                    {
                        instance = new ExampleButton();
                    }
                    return instance;
                }
            }

            public void OnUpdate()
            {
                if (PAUtils.GetAsyncKeyState(bind) != 0)
                    Action();
            }

            public void Action() //just call the action in gui, by adding the onupdate it allows for binding to keybind
            {

            }
            private static ExampleButton instance;
        }

        public class NoClip : IBindable
        {
            //base stuff
            public int bind { get; set; }
            public bool settingKeybind { get; set; }

            public static NoClip Instance //by making it an instance u can add it to the loop that is at the bottom of the file
            {
                get
                {
                    if (instance == null)
                    {
                        instance = new NoClip();
                    }
                    return instance;
                }
            }

            public void OnUpdate()
            {
                if (!Settings.Instance.settingsData.b_Noclip)
                    return;

                Collider localCollider = GameObjectManager.Instance.localPlayer.GetComponent<CharacterController>();
                if (!localCollider) return;

                Transform localTransform = GameObjectManager.Instance.localPlayer.transform;
                localCollider.enabled = !(localTransform && PAUtils.GetAsyncKeyState(bind) != 0);

                if (!localCollider.enabled)
                {
                    bool WKey = PAUtils.GetAsyncKeyState((int)Keys.W) != 0,
                        AKey = PAUtils.GetAsyncKeyState((int)Keys.A) != 0,
                        SKey = PAUtils.GetAsyncKeyState((int)Keys.S) != 0,
                        DKey = PAUtils.GetAsyncKeyState((int)Keys.D) != 0,
                        SpaceKey = PAUtils.GetAsyncKeyState((int)Keys.Space) != 0,
                        CtrlKey = PAUtils.GetAsyncKeyState((int)Keys.LControlKey) != 0;

                    Vector3 inVec = new Vector3(0, 0, 0);

                    if (WKey)
                        inVec += localTransform.forward;
                    if (SKey)
                        inVec -= localTransform.forward;
                    if (AKey)
                        inVec -= localTransform.right;
                    if (DKey)
                        inVec += localTransform.right;
                    if (SpaceKey)
                        inVec.y += localTransform.up.y;
                    if (CtrlKey)
                        inVec.y -= localTransform.up.y;

                    GameObjectManager.Instance.localPlayer.transform.position += inVec
                        * (Settings.Instance.settingsData.fl_NoclipSpeed * Time.deltaTime);
                }
            }

            private static NoClip instance;
        }

        public class SelfRevive : IBindable
        {
            //base stuff
            public int bind { get; set; }
            public bool settingKeybind { get; set; }
            public static SelfRevive Instance
            {
                get
                {
                    if (instance == null)
                    {
                        instance = new SelfRevive();
                    }
                    return instance;
                }
            }

            public void OnUpdate()
            {
                if (PAUtils.GetAsyncKeyState(bind) != 0)
                    Action();
            }

            public void Action()
            {
                PlayerControllerB localPlayer = GameObjectManager.Instance.localPlayer;
                if (!localPlayer) return;

                StartOfRound.Instance.allPlayersDead = false;
                localPlayer.ResetPlayerBloodObjects(localPlayer.isPlayerDead);
                if (localPlayer.isPlayerDead || localPlayer.isPlayerControlled)
                {
                    localPlayer.isClimbingLadder = false;
                    localPlayer.ResetZAndXRotation();
                    localPlayer.thisController.enabled = true;
                    localPlayer.health = 100;
                    localPlayer.disableLookInput = false;
                    if (localPlayer.isPlayerDead)
                    {
                        localPlayer.isPlayerDead = false;
                        localPlayer.isPlayerControlled = true;
                        localPlayer.isInElevator = true;
                        localPlayer.isInHangarShipRoom = true;
                        localPlayer.isInsideFactory = false;
                        localPlayer.wasInElevatorLastFrame = false;
                        StartOfRound.Instance.SetPlayerObjectExtrapolate(false);
                        localPlayer.TeleportPlayer(StartOfRound.Instance.playerSpawnPositions[0].position, false, 0f, false, true);
                        localPlayer.setPositionOfDeadPlayer = false;
                        localPlayer.DisablePlayerModel(StartOfRound.Instance.allPlayerObjects[localPlayer.playerClientId], true, true);
                        localPlayer.helmetLight.enabled = false;
                        localPlayer.Crouch(false);
                        localPlayer.criticallyInjured = false;
                        localPlayer.playerBodyAnimator?.SetBool("Limp", false);
                        localPlayer.bleedingHeavily = false;
                        localPlayer.activatingItem = false;
                        localPlayer.twoHanded = false;
                        localPlayer.inSpecialInteractAnimation = false;
                        localPlayer.disableSyncInAnimation = false;
                        localPlayer.inAnimationWithEnemy = null;
                        localPlayer.holdingWalkieTalkie = false;
                        localPlayer.speakingToWalkieTalkie = false;
                        localPlayer.isSinking = false;
                        localPlayer.isUnderwater = false;
                        localPlayer.sinkingValue = 0f;
                        localPlayer.statusEffectAudio.Stop();
                        localPlayer.DisableJetpackControlsLocally();
                        localPlayer.health = 100;
                        localPlayer.mapRadarDotAnimator.SetBool("dead", false);
                        if (localPlayer.IsOwner)
                        {
                            HUDManager.Instance.gasHelmetAnimator.SetBool("gasEmitting", false);
                            localPlayer.hasBegunSpectating = false;
                            HUDManager.Instance.RemoveSpectateUI();
                            HUDManager.Instance.gameOverAnimator.SetTrigger("revive");
                            localPlayer.hinderedMultiplier = 1f;
                            localPlayer.isMovementHindered = 0;
                            localPlayer.sourcesCausingSinking = 0;
                            localPlayer.reverbPreset = StartOfRound.Instance.shipReverb;
                        }
                    }
                    SoundManager.Instance.earsRingingTimer = 0f;
                    localPlayer.voiceMuffledByEnemy = false;
                    SoundManager.Instance.playerVoicePitchTargets[localPlayer.playerClientId] = 1f;
                    SoundManager.Instance.SetPlayerPitch(1f, (int)localPlayer.playerClientId);
                    if (localPlayer.currentVoiceChatIngameSettings == null)
                    {
                        StartOfRound.Instance.RefreshPlayerVoicePlaybackObjects();
                    }
                    if (localPlayer.currentVoiceChatIngameSettings != null)
                    {
                        if (localPlayer.currentVoiceChatIngameSettings.voiceAudio == null)
                            localPlayer.currentVoiceChatIngameSettings.InitializeComponents();

                        if (localPlayer.currentVoiceChatIngameSettings.voiceAudio == null)
                            return;

                        localPlayer.currentVoiceChatIngameSettings.voiceAudio.GetComponent<OccludeAudio>().overridingLowPass = false;
                    }
                }
                PlayerControllerB playerControllerB = GameNetworkManager.Instance.localPlayerController;
                playerControllerB.bleedingHeavily = false;
                playerControllerB.criticallyInjured = false;
                playerControllerB.playerBodyAnimator.SetBool("Limp", false);
                playerControllerB.health = 100;
                HUDManager.Instance.UpdateHealthUI(100, false);
                playerControllerB.spectatedPlayerScript = null;
                HUDManager.Instance.audioListenerLowPass.enabled = false;
                StartOfRound.Instance.SetSpectateCameraToGameOverMode(false, playerControllerB);
                RagdollGrabbableObject[] array = UnityEngine.Object.FindObjectsOfType<RagdollGrabbableObject>();
                for (int j = 0; j < array.Length; j++)
                {
                    if (!array[j].isHeld)
                    {
                        if (StartOfRound.Instance.IsServer)
                        {
                            if (array[j].NetworkObject.IsSpawned)
                                array[j].NetworkObject.Despawn(true);
                            else
                                UnityEngine.Object.Destroy(array[j].gameObject);
                        }
                    }
                    else if (array[j].isHeld && array[j].playerHeldBy != null)
                    {
                        array[j].playerHeldBy.DropAllHeldItems(true, false);
                    }
                }
                DeadBodyInfo[] array2 = UnityEngine.Object.FindObjectsOfType<DeadBodyInfo>();
                for (int k = 0; k < array2.Length; k++)
                {
                    UnityEngine.Object.Destroy(array2[k].gameObject);
                }
                StartOfRound.Instance.livingPlayers = StartOfRound.Instance.connectedPlayersAmount + 1;
                StartOfRound.Instance.allPlayersDead = false;
                StartOfRound.Instance.UpdatePlayerVoiceEffects();
                StartOfRound.Instance.shipAnimator.ResetTrigger("ShipLeave");
            }
            private static SelfRevive instance;
        }

        public class Thirdperson : MonoBehaviour // credits: https://thunderstore.io/c/lethal-company/p/Verity/3rdPerson/
        {
            [HarmonyPatch(typeof(QuickMenuManager), "OpenQuickMenu")]
            public class QuickMenuManager_OpenQuickMenu_Patch
            {
                public static void Prefix()
                {
                    _previousState = ThirdpersonCamera.ViewState;
                    if (ThirdpersonCamera.ViewState)
                    {
                        GameObjectManager.Instance.localPlayer.quickMenuManager.isMenuOpen = false;
                        ThirdpersonCamera.Toggle();
                    }
                }
            }

            [HarmonyPatch(typeof(QuickMenuManager), "CloseQuickMenu")]
            public class QuickMenuManager_CloseQuickMenu_Patch
            {
                public static void Prefix()
                {
                    if (_previousState)
                    {
                        GameObjectManager.Instance.localPlayer.inTerminalMenu = false;
                        ThirdpersonCamera.Toggle();
                    }
                }
            }

            [HarmonyPatch(typeof(Terminal), "BeginUsingTerminal")]
            public class Terminal_BeginUsingTerminal_Patch
            {
                public static void Prefix()
                {
                    _previousState = ThirdpersonCamera.ViewState;
                    if (ThirdpersonCamera.ViewState)
                    {
                        GameObjectManager.Instance.localPlayer.quickMenuManager.isMenuOpen = false;
                        ThirdpersonCamera.Toggle();
                    }
                }
            }

            [HarmonyPatch(typeof(Terminal), "QuitTerminal")]
            public class Terminal_QuitTerminal_Patch
            {
                public static void Prefix()
                {
                    if (_previousState)
                    {
                        GameObjectManager.Instance.localPlayer.inTerminalMenu = false;
                        ThirdpersonCamera.Toggle();
                    }
                }
            }

            [HarmonyPatch(typeof(PlayerControllerB), "KillPlayer")]
            public class PlayerControllerB_KillPlayer_Patch
            {
                public static void Prefix()
                {
                    if (ThirdpersonCamera.ViewState)
                        ThirdpersonCamera.Toggle();
                }
            }

            public void Start()
            {
                ThirdpersonCamera ThirdpersonCamera = base.gameObject.AddComponent<ThirdpersonCamera>();
                ThirdpersonCamera.hideFlags = HideFlags.HideAndDontSave;
                UnityEngine.Object.DontDestroyOnLoad(ThirdpersonCamera);
            }

            public class ThirdpersonCamera : MonoBehaviour //dont feel like fucking with this to make it match
            {
                public int m_bind { get; set; }
                public bool m_setting_keybind { get; set; }

                private void Awake()
                {
                    ThirdpersonCamera._camera = base.gameObject.AddComponent<Camera>();
                    ThirdpersonCamera._camera.hideFlags = HideFlags.HideAndDontSave;
                    ThirdpersonCamera._camera.fieldOfView = 66f;
                    ThirdpersonCamera._camera.nearClipPlane = 0.1f;
                    ThirdpersonCamera._camera.cullingMask = 557520895;
                    ThirdpersonCamera._camera.enabled = false;
                    Object.DontDestroyOnLoad(ThirdpersonCamera._camera);
                }

                private void Update()
                {
                    if (GameObjectManager.Instance.localPlayer == null
                        || GameObjectManager.Instance.localPlayer.quickMenuManager.isMenuOpen || GameObjectManager.Instance.localPlayer.inTerminalMenu || GameObjectManager.Instance.localPlayer.isPlayerDead)
                    {
                        return;
                    }

                    ThirdpersonUpdate();
                    if(PAUtils.GetAsyncKeyState(m_bind) != 0)
                        ThirdpersonCamera.Toggle();
                }

                private void ThirdpersonUpdate()
                {
                    Camera gameplayCamera = GameObjectManager.Instance.localPlayer.gameplayCamera;
                    Vector3 a = gameplayCamera.transform.forward * -1f;
                    Vector3 b = gameplayCamera.transform.TransformDirection(Vector3.right) * 0.6f;
                    Vector3 b2 = Vector3.up * 0.1f;
                    float value = Settings.Instance.settingsData.fl_ThirdpersonDistance;
                    ThirdpersonCamera._camera.transform.position = gameplayCamera.transform.position + a * value + b + b2;
                    ThirdpersonCamera._camera.transform.rotation = Quaternion.LookRotation(gameplayCamera.transform.forward);
                }

                public static void Toggle()
                {
                    if (GameObjectManager.Instance.localPlayer == null || GameObjectManager.Instance.localPlayer.isTypingChat || GameObjectManager.Instance.localPlayer.quickMenuManager.isMenuOpen || GameObjectManager.Instance.localPlayer.inTerminalMenu || GameObjectManager.Instance.localPlayer.isPlayerDead)
                        return;
                    ThirdpersonCamera.ViewState = !ThirdpersonCamera.ViewState;
                    GameObject gameObject = GameObject.Find("Systems/UI/Canvas/Panel/");
                    Canvas component = GameObject.Find("Systems/UI/Canvas/").GetComponent<Canvas>();
                    if (ThirdpersonCamera.ViewState)
                    {
                        GameObjectManager.Instance.localPlayer.thisPlayerModel.shadowCastingMode = ShadowCastingMode.On;
                        gameObject.SetActive(false);
                        component.worldCamera = ThirdpersonCamera._camera;
                        component.renderMode = 0;
                        GameObjectManager.Instance.localVisor.SetActive(false);
                        GameObjectManager.Instance.localPlayer.thisPlayerModelArms.enabled = false;
                        GameObjectManager.Instance.localPlayer.gameplayCamera.enabled = false;
                        ThirdpersonCamera._camera.enabled = true;
                        return;
                    }
                    GameObjectManager.Instance.localPlayer.thisPlayerModel.shadowCastingMode = ShadowCastingMode.ShadowsOnly;
                    gameObject.SetActive(true);
                    component.worldCamera = GameObject.Find("UICamera").GetComponent<Camera>();
                    component.renderMode = (RenderMode)1;
                    GameObjectManager.Instance.localVisor.SetActive(!Settings.Instance.settingsData.b_RemoveVisor);
                    GameObjectManager.Instance.localPlayer.thisPlayerModelArms.enabled = (Possession.possessedEnemy == null);
                    GameObjectManager.Instance.localPlayer.gameplayCamera.enabled = true;
                    ThirdpersonCamera._camera.enabled = false;
                }

                public static Camera _camera;
                public static bool ViewState;
            }

            private static bool _previousState;
        }

        public static class Possession
        {
            public static bool beginPossession = false;
            public static EnemyAI lastpossessedEnemy = null, possessedEnemy = null;
            public static void StartPossession()
            {
                PlayerControllerB localPlayer = GameObjectManager.Instance.localPlayer;
                if (!localPlayer
                    || localPlayer.isPlayerDead) return;

                float closestDistance = float.MaxValue;
                EnemyAI nearestEnemy = null;

                foreach (EnemyAI enemy in GameObjectManager.Instance.enemies)
                {
                    if (enemy == lastpossessedEnemy
                        || enemy.isEnemyDead) continue;

                    float distanceToEnemy = PAUtils.GetDistance(GameObjectManager.Instance.localPlayer.transform.position,
                        enemy.transform.position);

                    if (distanceToEnemy < closestDistance)
                    {
                        closestDistance = distanceToEnemy;
                        nearestEnemy = enemy;
                    }
                }

                if (nearestEnemy != null)
                {
                    StopPossession();
                    possessedEnemy = nearestEnemy;
                    beginPossession = true;
                }
            }

            public static void StopPossession()
            {
                PlayerControllerB localPlayer = GameObjectManager.Instance.localPlayer;

                if (localPlayer && !localPlayer.isPlayerDead)
                {
                    localPlayer.DisablePlayerModel(localPlayer.playersManager.allPlayerObjects[localPlayer.playerClientId], true, false);
                    localPlayer.thisPlayerModelArms.enabled = true;
                }

                if (lastpossessedEnemy != null)
                {
                    foreach (Collider col in lastpossessedEnemy.GetComponentsInChildren<Collider>())
                        col.enabled = true;

                    lastpossessedEnemy.ChangeEnemyOwnerServerRpc(0);
                    lastpossessedEnemy.updatePositionThreshold = 1f;
                    lastpossessedEnemy.moveTowardsDestination = true;
                }

                possessedEnemy = null;
                lastpossessedEnemy = null;
            }

            public static void UpdatePossession()
            {
                if (possessedEnemy)
                {
                    PlayerControllerB localPlayer = GameObjectManager.Instance.localPlayer;
                    if (!localPlayer
                        || localPlayer.isPlayerDead)
                    {
                        StopPossession();
                        return;
                    }

                    if (possessedEnemy.isEnemyDead)
                    {
                        StopPossession();
                        return;
                    }

                    if (beginPossession)
                    {
                        localPlayer.DisablePlayerModel(localPlayer.playersManager.allPlayerObjects[localPlayer.playerClientId], false, true); // This is client-side
                        GameObjectManager.Instance.localPlayer.TeleportPlayer(possessedEnemy.transform.position);
                        beginPossession = false;
                    }

                    foreach (Collider col in possessedEnemy.GetComponentsInChildren<Collider>())
                        col.enabled = false;

                    possessedEnemy.ChangeEnemyOwnerServerRpc(GameObjectManager.Instance.localPlayer.actualClientId);
                    possessedEnemy.updatePositionThreshold = 0;
                    possessedEnemy.moveTowardsDestination = false;

                    possessedEnemy.transform.eulerAngles = GameObjectManager.Instance.localPlayer.transform.eulerAngles;
                    possessedEnemy.transform.position = GameObjectManager.Instance.localPlayer.transform.position;

                    lastpossessedEnemy = possessedEnemy;
                }
            }
        }

        public static Features Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Features();
                }
                return instance;
            }
        }
        public void UpdateAll()
        {
            SelfRevive.Instance.OnUpdate();
            NoClip.Instance.OnUpdate();
            examplefeature.OnUpdate();
            ExampleButton.Instance.OnUpdate();
        }
        private static Features instance;
    }
}
