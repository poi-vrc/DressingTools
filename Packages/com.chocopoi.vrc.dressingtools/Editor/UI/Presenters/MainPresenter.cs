﻿using System.Collections.Generic;
using Chocopoi.DressingTools.Cabinet;
using Chocopoi.DressingTools.UIBase.Presenters;
using Chocopoi.DressingTools.UIBase.Views;
using Newtonsoft.Json;
using UnityEngine;

namespace Chocopoi.DressingTools.UI.Presenters
{
    internal class MainPresenter : IMainPresenter
    {
        private IMainView mainView;

        public MainPresenter(IMainView mainView)
        {
            this.mainView = mainView;
        }

        public void StartDressingWizard()
        {
            // TODO: reset dressing tab?
            mainView.SwitchTab(1);
        }

        public void AddToCabinet(DTCabinet cabinet, GameObject wearableGameObject, DTWearableConfig config)
        {
            var cabinetWearable = new DTCabinetWearable(config)
            {
                wearableGameObject = wearableGameObject,
                // empty references
                appliedObjects = new List<GameObject>(),
                // serialize a json copy for backward compatibility backup 
                serializedJson = JsonConvert.SerializeObject(config, new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                })
            };

            cabinet.wearables.Add(cabinetWearable);

            // TODO: reset dressing tab?
            // return to cabinet page
            mainView.SwitchTab(0);
        }
    }
}