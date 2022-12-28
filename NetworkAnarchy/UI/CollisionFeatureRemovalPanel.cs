using NetworkAnarchy.Localization;
using QCommonLib.UI;
using UnityEngine;

namespace NetworkAnarchy.UI
{
    internal class CollisionFeatureRemovalPanel : QPopupWindow
    {
        //protected override PopupFrequency Frequency => PopupFrequency.Always;
        protected override bool ShouldShow => NetworkAnarchy.showCollisionRemoval;
        protected override bool IncludeBottomButtonGap => true;

        public override void Start()
        {
            base.Start();

            SetSize(new Vector2(430, 220));
            SetText("Network Anarchy", Str.popup_collisionRemoved);

            OKButton();

            //UIButton button = CreateButton(this);
            //button.autoSize = false;
            //button.textHorizontalAlignment = UIHorizontalAlignment.Center;
            //button.size = new Vector2(80, 30);
            //button.text = "OK";
            //button.relativePosition = new Vector3(width / 2 - button.width / 2, height - 40);
            //button.eventClicked += (c, p) =>
            //{
            //    Close();
            //};
        }

        public override void Close()
        {
            base.Close();
            NetworkAnarchy.showCollisionRemoval.value = false;
        }
    }
}
