---
sidebar_position: 1
---

# Licensing

The whole DressingTools system mainly consists of three parts and they are separately licensed and some are dual-licensed.

### poi-vrc/DressingTools [[Link]](https://github.com/poi-vrc/DressingTools)

This is the main heart of DressingTools system. ```poi-vrc/DressingTools``` is mainly licensed **under the GNU General Public License v3 (GPLv3)** [(tl;dr)](https://www.tldrlegal.com/license/gnu-general-public-license-v3-gpl-3).

The logo assets are **NOT public domain** and they are **separately licensed under specific terms** [[Link]](https://github.com/poi-vrc/DressingTools/blob/master/logo/README.md).

It is an implementation on top of DressingToolsLib and most assembly classes are marked as internal and not supposed to be used by
other developers. Your derived projects **must be licensed under GPLv3 or later and released open-source**, if you develop on top of
this repository.

If you want to extend DressingTools by adding new providers and modules, you should use DressingToolsLib instead.


### poi-vrc/DressingToolsLib

:::caution
DressingToolsLib is in the future. The current alpha version has not separated the library part from the main repository. (https://github.com/poi-vrc/DressingTools/issues/92)
:::

DressingToolsLib is a framework that assembles DressingTools and **provides interfaces for third-party developers** to
add external modules and providers to DressingTools.

It is licensed under the **GNU Lesser General Public License v3 (LGPLv3)** [(tl;dr)](https://www.tldrlegal.com/license/gnu-lesser-general-public-license-v3-lgpl-3),
which is more friendly to applications using the repository as a library without releasing the source code.


### poi-vrc/AvatarLib [[Link]](https://github.com/poi-vrc/AvatarLib)

AvatarLib is a generic library that contains a bunch of quick-to-use APIs for plugins and applications to add, modify an avatar.

It is licensed under the **GNU Lesser General Public License v3 (LGPLv3)** [(tl;dr)](https://www.tldrlegal.com/license/gnu-lesser-general-public-license-v3-lgpl-3).
