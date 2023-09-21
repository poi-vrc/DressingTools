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
'use strict';

const fs = require('fs');
const config = require('./docusaurus.config');

class App {
    copyDocsIfNeeded(locale, filePathSuffix) {
        const basePath = `docs${filePathSuffix}`;
        console.log("basePath: " + basePath);
    
        const fileList = fs.readdirSync(basePath);
    
        for (const fileName of fileList) {
            const srcPath = `${basePath}/${fileName}`;
            console.log("srcPath: " + srcPath);
    
            if (fs.lstatSync(srcPath).isDirectory()) {
                console.log("is dir, visiting");
                this.copyDocsIfNeeded(locale, `${filePathSuffix}/${fileName}`);
            } else {
                const destFolder = `i18n/${locale}/docusaurus-plugin-content-docs/current${filePathSuffix}`;
                const destPath = `${destFolder}/${fileName}`;
                console.log("destPath: " + destPath);
    
                if (!fs.existsSync(destPath)) {
                    console.log("does not exist, copying");
                    fs.mkdirSync(destFolder, { recursive: true });
                    fs.copyFileSync(srcPath, destPath);
                } else {
                    console.log("already exist, skipping");
                }
            }
        }
    }

    copyCodeJsonIfNeeded(defCodeJsonPath, locale) {
        const folder = `i18n/${locale}`;
        const path = `${folder}/code.json`;
        console.log("codeJsonPath: " + path);

        if (!fs.existsSync(path)) {
            console.log("does not exist, copying");
            fs.mkdirSync(folder, { recursive: true });
            fs.copyFileSync(defCodeJsonPath, path);
        } else {
            console.log("already exist, skipping");
        }
    }

    run() {
        const defaultLocale = config.i18n.defaultLocale;
        const locales = config.i18n.locales;
        
        // check if contain defaultLocale and locales
        if (!defaultLocale || !locales) {
            console.error("config does not contain default locale or locales");
            process.exit(-1);
            return;
        }
        
        const defCodeJsonPath = `i18n/${defaultLocale}/code.json`;
        if (!fs.existsSync(defCodeJsonPath)) {
            console.error(`${defaultLocale} does not contain code json: ${defCodeJsonPath}`);
            process.exit(-1);
            return;
        }

        for (const locale of locales) {
            if (locale === defaultLocale) {
                continue;
            }
            console.log(`------------ ${locale} start ------------`)
            this.copyCodeJsonIfNeeded(defCodeJsonPath, locale);
            this.copyDocsIfNeeded(locale, "");
            console.log(`------------ ${locale} end ------------`)
        }
    }
}

const app = new App();
app.run();
