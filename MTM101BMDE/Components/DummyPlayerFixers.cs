using UnityEngine;

namespace MTM101BaldAPI.Components
{
    internal class DummyPlayerFixers : MonoBehaviour
    {
        private ItemManager itm;
        private void Awake()
        {
            itm = GetComponent<ItemManager>();
        }
        private void OnDestroy()
        {
            if (StickerManager.Instance != null)
                StickerManager.Instance.OnStickerApplied -= itm.UpdateTargetInventorySize;
        }
    }

}