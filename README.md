KeyFinder
=========

KeyFinder is a simple application that finds resource key strings in source files.


## The Motivation and the Problem

I use some functions to localize my applications. Some examples are as follows:
> LocalizeHelper.Localize("Save and Continue")

> l10n.getMessage("Are you sure to delete?")

> Localizer.Get("res_confirm_to_delete")

The function names can be different but the common point is that these functions use a key value. The function uses this key value to find the localized string based on the user's culture. 

In the development phase of a multi-lingual application, some of the keys may  not be localized. So, we need to find these unlocalized keys. Before this, the list of all keys can be useful. 

## How It Works
This tool extracts the resource keys in all source files in the directories and generates a file (Excel or text file) that includes the matches which are found by predefined search patterns (regular expressions.

### Configuration
The configuration parameters are in the ResourceUtilities.exe.config.

* **BaseSearchFolder**: This is appended to search paths that are not rooted.  
* **SearchPaths**: The directories that will be searched for code files. You can seperate directories with | character.
* **ExcludedPaths**:Just directory name or full path. Seperate with |
* **RecursiveSearchEnabled**: If you want to search subdirectories under the search paths recursively, set this to true.
* **FileFilter**: The extensions of the code files. Example: *.cs;*.cshtml;*.js
* **OutputFile**: The output file can be in two formats. If the extension is xlsx, an excel file is generated. If it's not, then a text file including xml output is generated.
* **OpenOutputFileAfterProcess**: Set this to true if you want the output file to be opened automatically after extraction process is finished.

### Search Patterns

The search patterns (regular expressions) are in patterns.txt.
You can find some examples in the file. Each search pattern should contain a named group, key.
An example is as follows:

```sh
Localize(?<type>(GlobalFormatted)|(Global)|(Local)|(Formatted))\((?<key>("[^"]+")|([^'"][^)]+))\)
```

This pattern matches the following strings:
```sh
LocalizeGlobal("Some text here")
LocalizeLocal("res_good")
```

As you see in the example, there are two named groups: key and type. Key is mandatory as mentioned above whereas type is not. 

The pattern is used to capture matches in the source files. A match consists of the following properties:
* **Line**: This is the line in which the match is found.
* **File**: This is the file in which the match is found.
* **LineNumber**: This is the line number where the match is found.
* **Key**: This is the value of the named group, key. 
* **Type**: This is the value of the named group, type. 	
* **IsVariable**: If the value of key starts with " or ', this is false. Also, the first and last " or ' are removed.
