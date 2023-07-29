using System.Collections.Generic;
using Chocopoi.DressingTools.Cabinet;
using Chocopoi.DressingTools.Dresser;
using Chocopoi.DressingTools.Logging;
using Chocopoi.DressingTools.UIBase.Presenters;
using Chocopoi.DressingTools.UIBase.Views;
using UnityEditor;
using UnityEngine;

namespace Chocopoi.DressingTools.UI.Presenters
{
    internal class WearableConfigPresenter : IWearableConfigPresenter
    {
        private IWearableConfigView wearableConfigView;

        public WearableConfigPresenter(IWearableConfigView wearableConfigView)
        {
            this.wearableConfigView = wearableConfigView;
        }
    }
}
