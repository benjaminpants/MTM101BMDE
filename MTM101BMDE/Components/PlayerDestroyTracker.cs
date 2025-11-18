using UnityEngine;

namespace MTM101BaldAPI.Components
{
    internal class PlayerDestroyTracker : MonoBehaviour
    {
        public ItemManager itm;
        private void OnDestroy()
        {
            if (StickerManager.Instance != null)
                StickerManager.Instance.OnStickerApplied -= itm.UpdateTargetInventorySize;
        }
    }

}