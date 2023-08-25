<h1 align="center">Csv-Reader</h1>

[![license](https://img.shields.io/badge/LICENSE-MIT-green.svg)](LICENSE.md)

## Table of Contents

<!-- START doctoc generated TOC please keep comment here to allow auto update -->
<!-- DON'T EDIT THIS SECTION, INSTEAD RE-RUN doctoc TO UPDATE -->
<details>
<summary>Details</summary>

- [Overview](#overview)
    - [Features](#features)
    - [Demo](#demo)
- [Breaking changes of this fork](#breaking-changes-of-this-fork)
- [Setup](#setup)
    - [Requirement](#requirement)
    - [Install via OpenUPM](#install-via-openupm)
    - [Install via Package Manager](#install-via-package-manager)
 - [License](#license)

</details>
<!-- END doctoc generated TOC please keep comment here to allow auto update -->

## Overview

### Features
* You can easy to read your csv and convert them to ScriptableObject without any effort.
* Overview your csv data structure in editor mode
* Auto update csv data when any files change
* ...

### Demo
* Read [samples](https://www.notion.so/haivd/Csv-Reader-399f96ed1be84031825eebc0d13bba64) to understand how to use package

## Breaking changes in this fork
* Require Unity 2021.3 or higher.
* Many improvements on various places.
* Many refactoring regarding class hierarchy.
* :warning: The README might be out-of-date.

## Setup

### Requirements
* Unity 2021.3 or higher
- [Odin Inspector](https://odininspector.com/)

### Install via Package Manager
1. Open the Package Manager from Window > Package Manager
2. `+` button > Add package from git URL
3. Enter the following to install

```
https://github.com/Zitga-Tech/ZBase.Csv-Reader.git?path=/Packages/ZBase.CsvReader
```

<p align="center">
  <img width=500 src="https://user-images.githubusercontent.com/47441314/118421190-97842b00-b6fb-11eb-9f94-4dc94e82367a.png" alt="Package Manager">
</p>

Or, open Packages/manifest.json and add the following to the dependencies block.

```json
{
    "dependencies": {
        "com.zbase.csv-reader": "https://github.com/Zitga-Tech/ZBase.Csv-Reader.git?path=/Packages/ZBase.CsvReader"
    }
}
```

If you want to set the target version, specify it like follow.

```json
{
    "dependencies": {
        "com.zbase.csv-reader": "https://github.com/Zitga-Tech/ZBase.Csv-Reader.git?path=/Packages/ZBase.CsvReader#1.0.0"
    }
}
```

## License
This software is released under the MIT License.
You are free to use it within the scope of the license.
However, the following copyright and license notices are required for use.

* https://github.com/Zitga-Tech/ZBase.Csv-Reader/blob/master/LICENSE.md
