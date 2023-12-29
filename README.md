# Unity project analysis

### Description
This tool analyses a Unity project directory and dumps following information about the project:

* For each scene file it dumps scene hierarchy.
* For the whole project it finds the unused scripts and dumps them into a csv file.

The analysis is performed based on the [Unity's YAML specification](https://blog.unity.com/engine-platform/understanding-unitys-serialization-language-yaml). 
The [YamlDotNet](https://github.com/aaubry/YamlDotNet) library is used to parse YAML files.

### Usage

```bash
./Parser.exe unity_project_path output_folder_path
```
