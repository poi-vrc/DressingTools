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

using Chocopoi.DressingTools.Components.Menu;
using Chocopoi.DressingTools.Inspector.Views;

namespace Chocopoi.DressingTools.UI.Presenters
{
    internal class MenuItemPresenter
    {
        private readonly IMenuItemView _view;

        public MenuItemPresenter(IMenuItemView view)
        {
            _view = view;
            SubscribeEvents();
        }

        private void SubscribeEvents()
        {
            _view.Load += OnLoad;
            _view.Unload += OnUnload;

            _view.NameChanged += OnNameChanged;
            _view.TypeChanged += OnTypeChanged;
        }

        private void UnsubscribeEvents()
        {
            _view.Load -= OnLoad;
            _view.Unload -= OnUnload;

            _view.NameChanged -= OnNameChanged;
        }

        private void OnNameChanged()
        {
            _view.Target.Name = _view.Name;
            _view.Repaint();
        }

        private void OnTypeChanged()
        {
            _view.Target.Type = (DTMenuItem.ItemType)_view.Type;
            _view.Repaint();
        }

        private void UpdateView()
        {
            _view.Name = _view.Target.Name;
            _view.Type = (int)_view.Target.Type;
            _view.Repaint();
        }

        private void OnLoad()
        {
            UpdateView();
        }

        private void OnUnload()
        {
            UnsubscribeEvents();
        }
    }
}
