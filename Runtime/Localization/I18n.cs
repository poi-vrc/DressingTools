/*
 * Copyright (c) 2023 chocopoi
 * 
 * This file is part of DressingTools.
 * 
 * DressingTools is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
 * 
 * DressingTools is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License along with DressingTools. If not, see <https://www.gnu.org/licenses/>.
 */

using Chocopoi.DressingFramework.Localization;

namespace Chocopoi.DressingTools.Localization
{
    internal class I18n
    {
        private const string TranslatorIdentifier = "com.chocopoi.vrc.dressingtools.i18n";
        private const string TranslationsFolder = "Packages/com.chocopoi.vrc.dressingtools/Translations";

        public static I18nTranslator ToolTranslator
        {
            get
            {
                var translator = I18nManager.Instance.Translator(TranslatorIdentifier);
                if (translator.GetAvailableLocales().Length == 0)
                {
                    translator.LoadTranslations(TranslationsFolder);
                }
                return translator;
            }
        }
    }
}

