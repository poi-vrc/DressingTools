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
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEngine.UIElements;

namespace Chocopoi.DressingTools.UI.Elements
{
    /// <summary>
    /// A self-made simple implementation to show elements in a table-like style
    /// </summary>
    [ExcludeFromCodeCoverage]
    internal class TableView : VisualElement
    {
        public const int DefaultRowHeight = 18;

        public class TableModel
        {
            public string[] Headers { get; private set; }
            public int ColumnCount { get; private set; }
            public List<VisualElement[]> Rows { get; private set; }

            public TableModel(int colCount)
            {
                Headers = null;
                ColumnCount = colCount;
                Rows = new List<VisualElement[]>();
            }

            public TableModel(string[] headers)
            {
                Headers = headers;
                ColumnCount = headers.Length;
                Rows = new List<VisualElement[]>();
            }

            public void AddRow(params VisualElement[] views)
            {
                if (views.Length != ColumnCount)
                {
                    return;
                }
                Rows.Add(views);
            }

            public void RemoveRow(int idx)
            {
                if (idx < 0 || idx >= Rows.Count)
                {
                    return;
                }
                Rows.RemoveAt(idx);
            }

            public void Clear()
            {
                Rows.Clear();
            }
        }

        private TableModel _model;
        private int _rowHeight;

        public TableView(TableModel model, int rowHeight = DefaultRowHeight)
        {
            _model = model;
            _rowHeight = rowHeight;
            Repaint();
        }

        public void Repaint()
        {
            Clear();

            style.flexDirection = FlexDirection.Row;

            var colViews = new List<VisualElement>();
            for (var i = 0; i < _model.ColumnCount; i++)
            {
                var container = new VisualElement();
                container.AddToClassList("table-view-col");
                container.AddToClassList($"table-view-col-{i}");
                container.style.flexDirection = FlexDirection.Column;
                colViews.Add(container);
                Add(container);
            }

            if (_model.Headers != null)
            {
                for (var i = 0; i < _model.Headers.Length; i++)
                {
                    var container = new VisualElement();
                    container.AddToClassList("table-view-header");
                    container.AddToClassList($"table-view-header-{i}");
                    container.style.flexGrow = 0;
                    container.style.alignContent = Align.Center;
                    container.style.flexDirection = FlexDirection.Row;
                    container.style.height = _rowHeight;
                    container.Add(new Label(_model.Headers[i]));
                    colViews[i].Add(container);
                }
            }

            foreach (var row in _model.Rows)
            {
                for (var i = 0; i < row.Length; i++)
                {
                    var container = new VisualElement();
                    container.AddToClassList("table-view-row");
                    container.AddToClassList($"table-view-row-{i}");
                    container.style.flexGrow = 0;
                    container.style.alignContent = Align.FlexStart;
                    container.style.flexDirection = FlexDirection.Row;
                    container.style.height = _rowHeight;
                    container.Add(row[i]);
                    colViews[i].Add(container);
                }
            }
        }
    }
}
