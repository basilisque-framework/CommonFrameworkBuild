<!--
   Copyright 2025 Alexander Stärk

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
-->
# Basilisque - Common Framework Build

## Overview
[![GitHub](https://img.shields.io/badge/GitHub-Project-%23004880.svg?logo=github)](https://github.com/basilisque-framework/CommonFrameworkBuild)
[![License](https://img.shields.io/badge/License-Apache%20License%202.0-%23D22128.svg?logo=apache&logoColor=%23D22128)](https://github.com/basilisque-framework/CommonFrameworkBuild/blob/main/LICENSE.txt)  

This project provides common build configuration for all parts of the Basilisque framework.  
It is __NOT__ recommended to use this unless you're developing a part of the framework.

## Description
This project doesn't contain any assemblies that will be deployed with the target project. Instead it only contains build time configuration (.props/.targets-files, ...) to provide all projects of the Basilisque framework with a common basic set of configuration.  
It implicitly adds a dependency to [Basilisque.CommonBuild](https://github.com/basilisque-framework/CommonBuild) to the target project. So the configuration provided by that project will also be applied.  

### Usage
Simply installing this NuGet package will automatically include the contained .props and .targets files in the build.
For more information about how this can be configured, please see the [GitHub page](https://github.com/basilisque-framework/CommonFrameworkBuild).

## License
The Basilisque framework (including this repository) is licensed under the [Apache License, Version 2.0](LICENSE.txt).