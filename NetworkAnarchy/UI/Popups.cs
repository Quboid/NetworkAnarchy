using NetworkAnarchy.Lang;
using QCommonLib.UI;
using UnityEngine;

namespace NetworkAnarchy.UI
{
    //internal class ButtonReminderToastPanel : QToast
    //{
    //    protected override bool ShouldShow
    //    {
    //        get
    //        {
    //            if (NetworkAnarchy.instance == null) return false;
    //            if (NetworkAnarchy.instance.m_firstRun) return false;
    //            if (!NetworkAnarchy.instance.IsActive) return false;
    //            if (!NetworkAnarchy.showButtonReminder) return false;
    //            if (NetworkAnarchy.m_toolOptionButton.m_toolOptionsPanel.isVisible) return false;
    //            return true;
    //        }
    //    }

    //    public override void Start()
    //    {
    //        autoPanelVAlign = PanelVAlignment.Bottom;
    //        Vector3 btnPos = NetworkAnarchy.m_toolOptionButton.absolutePosition;
    //        arrowOffset = 40;
    //        size = new Vector2(400, 50);
    //        absolutePosition = btnPos + new Vector3(-70, -123, 0);

    //        base.Start();
    //        SetText("Network Anarchy", Str.popup_buttonReminder);
    //        isVisible = ShouldShow;
    //    }

    //    public override void Close()
    //    {
    //        CloseOnce();
    //        NetworkAnarchy.showButtonReminder.value = false;
    //    }

    //    public void CloseOnce()
    //    {
    //        base.Close();
    //        NetworkAnarchy.instance.ButtonReminderToast = null;
    //    }
    //}

    //internal class CollisionFeatureRemovalPanel : QPopupWindow
    //{
    //    protected override bool ShouldShow => NetworkAnarchy.showCollisionRemoval;
    //    protected override bool IncludeBottomButtonGap => true;

    //    public override void Start()
    //    {
    //        base.Start();

    //        SetSize(new Vector2(430, 220));
    //        SetText("Network Anarchy", Str.popup_collisionRemoved);

    //        OKButton();
    //    }

    //    public override void Close()
    //    {
    //        base.Close();
    //        NetworkAnarchy.showCollisionRemoval.value = false;
    //    }
    //}
}
