/*
 * Copyright (c) 2024 chocopoi
 * 
 * This file is part of DressingTools.
 * 
 * DressingTools is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
 * 
 * DressingTools is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License along with DressingTools. If not, see <https://www.gnu.org/licenses/>.
 */

using System.Collections.Generic;
using System.Linq;
using Chocopoi.DressingTools.Event;
using Chocopoi.DressingTools.OneConf.Cabinet.Modules;
using Chocopoi.DressingTools.OneConf.Wearable.Modules;

#if DT_VRCSDK3A
using Chocopoi.DressingTools.OneConf.Integration.VRChat.Modules;
#endif

namespace Chocopoi.DressingTools.OneConf
{
    // this class is temporaily used to store modules centrally
    // it will probably removed after the API layer is created
    internal class ModuleManager
    {
        public static ModuleManager Instance { get => s_instance ?? (s_instance = new ModuleManager()); }
        private static ModuleManager s_instance = null;

        private readonly Dictionary<string, CabinetModuleProvider> _cabMods;
        private readonly Dictionary<string, WearableModuleProvider> _wearMods;
        private readonly List<IToolEventHandler> _handlers;

        public ModuleManager()
        {
            _cabMods = new Dictionary<string, CabinetModuleProvider>();
            _wearMods = new Dictionary<string, WearableModuleProvider>();
            _handlers = new List<IToolEventHandler>();
            AddModules();
        }

        private void AddCabinetModule(CabinetModuleProvider provider) => _cabMods[provider.Identifier] = provider;

        private void AddWearableModule(WearableModuleProvider provider) => _wearMods[provider.Identifier] = provider;

        private void AddModules()
        {
            AddCabinetModule(new CabinetAnimCabinetModuleProvider());
            AddWearableModule(new ArmatureMappingWearableModuleProvider());
            AddWearableModule(new BlendshapeSyncWearableModuleProvider());
            AddWearableModule(new CabinetAnimWearableModuleProvider());
            AddWearableModule(new MoveRootWearableModuleProvider());

#if DT_VRCSDK3A
            AddCabinetModule(new VRCCabinetAnimCabinetModuleProvider());
            AddWearableModule(new VRCMergeAnimLayerWearableModuleProvider());
#endif

            _handlers.Add(new BlendshapeSyncWearableModuleProvider.EventAdapter());
            _handlers.Add(new CabinetAnimWearableModuleProvider.EventAdapter());
        }

        public CabinetModuleProvider GetCabinetModuleProvider(string identifier)
        {
            return _cabMods.ContainsKey(identifier) ? _cabMods[identifier] : null;
        }

        public WearableModuleProvider GetWearableModuleProvider(string identifier)
        {
            return _wearMods.ContainsKey(identifier) ? _wearMods[identifier] : null;
        }

        public List<WearableModuleProvider> GetAllWearableModuleProviders()
        {
            return _wearMods.Values.ToList();
        }

        // TODO: replace to use c# events
        public List<IToolEventHandler> GetAllToolEventHandlers()
        {
            return _handlers;
        }
    }
}
