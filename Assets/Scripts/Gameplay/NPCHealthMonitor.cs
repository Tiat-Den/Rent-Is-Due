using UnityEngine;
using UnityEngine.SceneManagement;

namespace RentIsDue.Gameplay
{
    /// <summary>
    /// Bulletproof runtime guardian that verifies Dealer_NPC and ToolShop_NPC exist,
    /// are active, properly positioned behind their counters, and have their renderers visible.
    /// Auto-restores them instantly if any scene loading or prefab glitch left them missing.
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public class NPCHealthMonitor : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoInit()
        {
            if (SceneManager.GetActiveScene().name != "SampleScene") return;

            GameObject guardian = GameObject.Find("NPC_Health_Guardian");
            if (guardian == null)
            {
                guardian = new GameObject("NPC_Health_Guardian");
                guardian.AddComponent<NPCHealthMonitor>();
                DontDestroyOnLoad(guardian);
            }
        }

        private void Start()
        {
            VerifyAndRestoreNPCs();
        }

        private void Update()
        {
            // Lightweight periodic check every 60 frames (~1 sec)
            if (Time.frameCount % 60 == 0)
            {
                VerifyAndRestoreNPCs();
            }
        }

        public static void VerifyAndRestoreNPCs()
        {
            CheckOrRestoreNPC("Dealer_NPC", "Dealer_Station", "Assets/Art/Characters/Textures/criminalMaleA.png", "Mat_Dealer_Skin", true);
            CheckOrRestoreNPC("ToolShop_NPC", "ToolShop_Station", "Assets/Art/Characters/Textures/survivorMaleB.png", "Mat_ToolShop_Skin", false);
        }

        private static void CheckOrRestoreNPC(string npcName, string stationName, string texturePath, string matName, bool isDealer)
        {
            GameObject npc = GameObject.Find(npcName);
            GameObject station = GameObject.Find(stationName);

            if (station == null) return; // Station not found in current scene

            bool needsRespawn = false;

            if (npc == null)
            {
                // Check if hidden under station
                Transform child = station.transform.Find(npcName);
                if (child != null)
                {
                    npc = child.gameObject;
                    if (!npc.activeSelf) npc.SetActive(true);
                }
                else
                {
                    needsRespawn = true;
                }
            }

            if (npc != null)
            {
                // Verify renderer health
                var smr = npc.GetComponentInChildren<SkinnedMeshRenderer>(true);
                if (smr == null || !smr.enabled)
                {
                    if (smr != null)
                    {
                        smr.enabled = true;
                        smr.updateWhenOffscreen = true;
                    }
                }
                else
                {
                    smr.updateWhenOffscreen = true;
                }

                // Verify scale & position
                if (Mathf.Abs(npc.transform.localScale.x - 0.48f) > 0.05f)
                {
                    npc.transform.localScale = Vector3.one * 0.48f;
                }

                if (npc.transform.parent != station.transform)
                {
                    npc.transform.SetParent(station.transform, false);
                }

                Vector3 localPos = npc.transform.localPosition;
                if (localPos.y < 0.2f || localPos.y > 0.8f || localPos.z < 0.2f)
                {
                    npc.transform.localPosition = new Vector3(0f, 0.45f, 0.70f);
                    npc.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
                }

                // Verify NPCLookAtPlayer is attached
                if (npc.GetComponent<NPCLookAtPlayer>() == null)
                {
                    npc.AddComponent<NPCLookAtPlayer>();
                }

                return;
            }

#if UNITY_EDITOR
            if (needsRespawn)
            {
                string charPath = "Assets/Art/Characters/characterMedium.fbx";
                GameObject charPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(charPath);
                if (charPrefab != null)
                {
                    npc = Instantiate(charPrefab, station.transform);
                    npc.name = npcName;
                    npc.transform.localPosition = new Vector3(0f, 0.45f, 0.70f);
                    npc.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
                    npc.transform.localScale = Vector3.one * 0.48f;

                    // Disable Mecanim Animator so NPCLookAtPlayer controls the pose
                    var anim = npc.GetComponent<Animator>();
                    if (anim != null) anim.enabled = false;

                    // Rigidbody kinematic
                    var rb = npc.GetComponent<Rigidbody>();
                    if (rb == null) rb = npc.AddComponent<Rigidbody>();
                    rb.isKinematic = true;

                    // Capsule collider trigger
                    var col = npc.GetComponent<CapsuleCollider>();
                    if (col == null) col = npc.AddComponent<CapsuleCollider>();
                    col.isTrigger = true;
                    col.center = new Vector3(0f, 1.9f, 0f);
                    col.height = 3.8f;
                    col.radius = 0.75f;

                    // Material
                    string matPath = $"Assets/Materials/Characters/{matName}.mat";
                    Material mat = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(matPath);
                    if (mat != null)
                    {
                        foreach (var r in npc.GetComponentsInChildren<Renderer>(true))
                        {
                            r.sharedMaterial = mat;
                            if (r is SkinnedMeshRenderer smr) smr.updateWhenOffscreen = true;
                        }
                    }

                    // Gameplay interactables
                    if (isDealer)
                    {
                        if (npc.GetComponent<RentIsDue.Economy.DealerInteractable>() == null)
                            npc.AddComponent<RentIsDue.Economy.DealerInteractable>();
                        if (npc.GetComponent<DailyOrderManager>() == null)
                            npc.AddComponent<DailyOrderManager>();
                    }
                    else
                    {
                        if (npc.GetComponent<RentIsDue.Shop.ToolShopInteractable>() == null)
                            npc.AddComponent<RentIsDue.Shop.ToolShopInteractable>();
                        if (npc.GetComponent<RentIsDue.Shop.ToolShopManager>() == null)
                            npc.AddComponent<RentIsDue.Shop.ToolShopManager>();
                    }

                    npc.AddComponent<NPCLookAtPlayer>();
                    Debug.Log($"<color=green>[NPCHealthMonitor] Auto-restored missing '{npcName}' at {stationName} successfully!</color>");
                }
            }
#endif
        }
    }
}
