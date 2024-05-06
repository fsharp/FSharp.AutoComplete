# Changelog

## [0.72.3] - 2024-05-05

### Added

* [FSAC publishes a net8.0 TFM version of the tool as well, to prevent issues when running across TargetFrameworks](https://github.com/ionide/FsAutoComplete/pull/1281)
* [Long-running actions like typechecking specific files can now be cancelled by users](https://github.com/ionide/FsAutoComplete/pull/1274) (thanks @TheAngryByrd)

### Fixed

* [Fix restoring multiple script file NuGet dependencies in parallel](https://github.com/ionide/FsAutoComplete/pull/1275) (thanks @TheAngryByrd)

## [0.72.2] - 2024-04-30

### Fixed

* [Use actualRootPath instead of p.RootPath when peeking workspaces](https://github.com/ionide/FsAutoComplete/pull/1278) (thanks @oupson)

## [0.72.1] - 2024-04-25

### Added 

* [Show additional diagnostics specific to script files](https://github.com/ionide/FsAutoComplete/pull/1248) (thanks @TheAngryByrd)
* [Add some code fixes for type mismatch](https://github.com/ionide/FsAutoComplete/pull/1250) (thanks @nojaf)

### Fixed

* [Shift multiline paren contents less aggressively](https://github.com/ionide/FsAutoComplete/pull/1242) (thanks @brianrourkeboll)
* [fix unicode characters in F# compiler diagnostic messages](https://github.com/ionide/FsAutoComplete/pull/1265) (thanks @MrLuje)
* [Place XML doc lines before any attribute lists](https://github.com/ionide/FsAutoComplete/pull/1267) (thanks @dawedawe)
* [Don't generate params for explicit getters/setters](https://github.com/ionide/FsAutoComplete/pull/1268) (thanks @dawedawe)
* [Fix Nuget Script Restores when doing them in parallel](https://github.com/ionide/FsAutoComplete/pull/1275) (thanks @TheAngryByrd)

### Changed

* [Migrate Codefix Scaffolding](https://github.com/ionide/FsAutoComplete/pull/1256) (thanks @nojaf)
* [Bump ProjInfo to 0.64.0](https://github.com/ionide/FsAutoComplete/pull/1270) Check out the [release notes](https://github.com/ionide/proj-info/releases/tag/v0.64.0) for more details (thanks @baronfel) 
  * Fixes Loading Projects in some cases
  * Adds Traversal Project support

## [0.71.0] - 2024-03-07

### Added

* [Add unnecessary parentheses analyzer & code fix](https://github.com/fsharp/FsAutoComplete/pull/1235) (thanks @brianrourkeboll)

### Fixed

* [Fix debugger regression](https://github.com/fsharp/FsAutoComplete/pull/1230/files) (thanks @baronfel)

### Changed

* [Slightly better Project Loading messages](https://github.com/fsharp/FsAutoComplete/pull/1234) (thanks @TheAngryByrd)
* [add missing keyword list and extra information](https://github.com/fsharp/FsAutoComplete/pull/1226) (thanks @jkone27)
* [Speed up project load times](https://github.com/fsharp/FsAutoComplete/pull/1245) (thanks @TheAngryByrd)

## [0.70.1] - 2024-02-13

### Fixed

* [Fix the OutputPath returned from Project data so that it uses the executable/loadable assembly instead of reference assemblies.](https://github.com/fsharp/FsAutoComplete/pull/1230/)

### Changed

* [Analyzers: Update analyzers support to 0.24.0](https://github.com/fsharp/FsAutoComplete/pull/1229) (thanks @nojaf)


## [0.70.0] - 2024-02-06

### Changed

* [Update Ionide.ProjInfo and enable support for Reference Assemblies where they exist](https://github.com/fsharp/FsAutoComplete/pull/1228)

## [0.69.0] - 2024-01-14

### Added

- [Codefix: Update value in signature file](https://github.com/fsharp/FsAutoComplete/pull/1161) and [1220](https://github.com/fsharp/FsAutoComplete/pull/1220) (thanks @nojaf)

### Changed

- [Analyzers: Update analyzers support to 0.23.0](https://github.com/fsharp/FsAutoComplete/pull/1217) (thanks @dawedawe)

### Fixed

- [fix and improves l10n](https://github.com/fsharp/FsAutoComplete/pull/1181) (thanks @Tangent-90)
- [Fix AP signatures for APs with names which are substrings of other APs](https://github.com/fsharp/FsAutoComplete/pull/1211) (thanks @dawedawe)
- [fixing caching of cancelled cached tasks](https://github.com/fsharp/FsAutoComplete/pull/1221) (thanks @TheAngryByrd)

### Internal


## [0.68.0] - 2023-11-17

### Added

* [Dotnet 8 support](https://github.com/fsharp/FsAutoComplete/pull/1175) (Thanks @baronfel & @TheAngryByrd)
* [F# 8 Support](https://github.com/fsharp/FsAutoComplete/pull/1180) (Thanks @baronfel & @nojaf & @dawedawe)

### Changed

* [Updates Ionide.LanguageServerProtocol to 0.4.20](https://github.com/fsharp/FsAutoComplete/pull/1190) (Thanks @TheAngryByrd)
* [Update IcedTasks 0.9.2](https://github.com/fsharp/FsAutoComplete/pull/1197) (Thanks @TheAngryByrd)
* [Paket Simplify](https://github.com/fsharp/FsAutoComplete/pull/1204) (Thanks @1eyewonder)

### Fixed

- [Do ordinal string comparisons](https://github.com/fsharp/FsAutoComplete/pull/1193) (Thanks @dawedawe)
- [fix typo in FullNameExternalAutocomplete default value](https://github.com/fsharp/FsAutoComplete/pull/1196) (Thanks @MrLuje)
* [Fix tooltip errorhandling]()(https://github.com/fsharp/FsAutoComplete/pull/1195) (Thanks @pblasucci & @TheAngryByrd)

## [0.67.0] - 2023-10-28

### Changed

* [Better Completion for ExternalAutocomplete functions](https://github.com/fsharp/FsAutoComplete/pull/1178) (Thanks @Tangent-90!)
* LSP Refactoring [1179](https://github.com/fsharp/FsAutoComplete/pull/1179) [1188](https://github.com/fsharp/FsAutoComplete/pull/1188) (Thanks @TheAngryByrd)
* [Fix Spelling](https://github.com/fsharp/FsAutoComplete/pull/1182) (Thanks @TheAngryByrd)

### Fixed
* [Json serializer error can cause server crash ](https://github.com/fsharp/FsAutoComplete/pull/1189) (Thanks @TheAngryByrd)
* [Fixes a race condition with ProgressListener.End](https://github.com/fsharp/FsAutoComplete/pull/1183) (Thanks @TheAngryByrd)

## [0.66.1] - 2023-10-15

### Changed

* Fixed a bug in inlay hints generation for constructors and methods that would cause a crash on members with optional or ParamArray parameters.

## [0.66.0] - 2023-10-15

### Removed

* [The following options have been removed from the LSP. The old CLI options will trigger a warning if present, but will not crash the server]((https://github.com/fsharp/FsAutoComplete/pull/1174)) (Thanks @TheAngryByrd!)
  * The older, Non-Adaptive LSP implementation (in favor of using the Adaptive LSP server)
  * NamedText (in favor of RoslynSourceText)

### Changed

* [The Inlay Hints' Parameter Hints learned how to show parameter hints for constructor arguments and method parameters](https://github.com/fsharp/FsAutoComplete/pull/1176)

## [0.65.0] - 2023-10-09

### Added
- [Incoming Call Hierarchy](https://github.com/fsharp/FsAutoComplete/pull/1164) (thanks @TheAngryByrd!)

### Fixed 
- [Ignore requests that would cause circular dependencies in project references](https://github.com/fsharp/FsAutoComplete/pull/1173) (thanks @dawedawe!)

## [0.64.1] - 2023-10-05

### Fixed
- [fix the Define active pattern](https://github.com/fsharp/FsAutoComplete/pull/1170) (thanks @dawedawe!)

## [0.64.0] - 2023-09-27

### Added 
- [Add CodeActions for Number Constants: Convert between bases, Add digit group separators](https://github.com/fsharp/FsAutoComplete/pull/1167) (thanks @BooksBaum!)

### Changed
- [Default to RoslynSourceText](https://github.com/fsharp/FsAutoComplete/pull/1168) (thank @TheAngryByrd!)

## [0.63.1] - 2023-09-18

### Fixed
- [fix an expecto test detection](https://github.com/fsharp/FsAutoComplete/pull/1165) (Thanks @TheAngryByrd!)

## [0.63.0] - 2023-09-06

### Added
* [Add support for Expecto theory tests](https://github.com/fsharp/FsAutoComplete/pull/1160) (thanks @Numpsy!)
* [Add Scaffolding for Generating Codefixes](https://github.com/fsharp/FsAutoComplete/pull/1158) (thanks @nojaf!)
### Changed
* [Extract out AST-collecting-walker to a separate function + abstract class](https://github.com/fsharp/FsAutoComplete/pull/1154) (thanks @baronfel!)

### Fixed
* [Fixed File Index out of range issues](https://github.com/fsharp/FsAutoComplete/pull/1152) (thanks @Happypig375!)

## [0.62.0] - 2023-08-21

### Added

* A new flag for controlling FSAC's support of ParallelReferenceResolution - `fsharp.fsac.parallelReferenceResolution`. If true, this allows for more parallelization of the compilation.

### Changed

* Updated Ionide.LanguageServerProtocol to get better LSP 3.17 support
* Updated to FCS 7.0.400!

### Fixed

* [Massively improved the performance of comparing file paths in the LSP server](https://github.com/fsharp/FsAutoComplete/pull/1139) (thanks @TheAngryByrd!)
* [Improved getting declarations in the Adaptive LSP Server](https://github.com/fsharp/FsAutoComplete/pull/1150) (thanks @TheAngryByrd!)


## [0.61.1] - 2023-07-22

### Fixed

* [Reduce project option duplication, reducing memory usage](https://github.com/fsharp/FsAutoComplete/pull/1147) (thanks @TheAngryByrd!)


## [0.61.0] - 2023-07-16

### Added 

* [Codefix: Add codefix for redundant attribute suffix.](https://github.com/fsharp/FsAutoComplete/pull/1132) (thanks @nojaf!)
* [Add module to SemanticTokenTypes](https://github.com/fsharp/FsAutoComplete/pull/1137) (thanks @nojaf!)
* [Codefix: Add type annotations to entire function](https://github.com/fsharp/FsAutoComplete/pull/1138) (thanks @nojaf!)
* [Codefix: RemovePatternArgument quick fix](https://github.com/fsharp/FsAutoComplete/pull/1142) (thanks @edgarfgp!)
* [Codefix:  for interpolated string](https://github.com/fsharp/FsAutoComplete/pull/1143), [fix #1](https://github.com/fsharp/FsAutoComplete/pull/1146) (thanks @nojaf!)

### Changed

* [Swap maybe for option CEs](https://github.com/fsharp/FsAutoComplete/pull/1131) (thanks @TheAngryByrd!)

### Fixed

* [Make ServerProgressReport threadsafe](https://github.com/fsharp/FsAutoComplete/pull/1130) (thanks @TheAngryByrd!)
* [Fix range handling for code completion in interpolated strings](https://github.com/fsharp/FsAutoComplete/pull/1133) (thanks @kojo12228!)
* [Fixing Typos](https://github.com/fsharp/FsAutoComplete/pull/1136) (thanks @TheAngryByrd!)
* [FSAC Not exiting on macos/linux](https://github.com/fsharp/FsAutoComplete/pull/1141) (thanks @TheAngryByrd!)
* [CI not failing on focused tests](https://github.com/fsharp/FsAutoComplete/pull/1145) (thanks @TheAngryByrd!)

### Removed

* [Remove old eventlistener](https://github.com/fsharp/FsAutoComplete/pull/1134) (thanks @TheAngryByrd!)

## [0.60.1] - 2023-07-01

### Added

* [A new set of settings for excluding files from built-in analyzers](https://github.com/fsharp/FsAutoComplete/pull/1120) (thanks @TheAngryByrd!)
* [A new setting for choosing the ISourceText implementation, along with a Roslyn-based implementation](https://github.com/fsharp/FsAutoComplete/pull/1123) (thanks @TheAngryByrd!)
* [The Generate Xml Doc codefix now works on properties with getters and setters](https://github.com/fsharp/FsAutoComplete/pull/1126) (thanks @dawedawe!)

## [0.60.0] - 2023-06-14

### Added

* [A new codefix for generating missing parameters and return types for XML Documentation](https://github.com/fsharp/FsAutoComplete/pull/1108) (thanks @dawedawe!)

### Fixed

* [Abstract classes generation now handles members with attributes](https://github.com/fsharp/FsAutoComplete/pull/1107)

### Changed

* [The FSharp.Compiler.Services were updated to 43.7.300, matching the F# Compiler shipped in .NET 7.0.300](https://github.com/fsharp/FsAutoComplete/pull/1116) (thanks @TheAngryByrd!)

## [0.59.6] - 2023-04-21

### Added

* [A new codefix to add the 'private' access modifier to bindings and types](https://github.com/fsharp/fsautocomplete/pull/1089) (thanks @dawedawe!)

### Fixed

* [Make the 'convert to positional DU pattern' codefix work in more cases](https://github.com/fsharp/fsautocomplete/pull/1090) (thanks @dawedawe!)
* [Handle text changes when they are empty in the classic LSP Server](https://github.com/fsharp/fsautocomplete/pull/1100) (thanks @augustfengd!)
* [Detect Expecto's Task-based tests](https://github.com/fsharp/fsautocomplete/pull/1105) (thanks @ratsclub!)

### Changed

* [Update Ionide.ProjInfo to get more logging](https://github.com/fsharp/fsautocomplete/pull/1093) (thanks @theangrybyrd!)
* [Update tooltips and Info Panel documentation](https://github.com/fsharp/fsautocomplete/pull/1099) (thanks @MaximeMangel!)
* [Update the xml doc generation codefix to work in more places](https://github.com/fsharp/fsautocomplete/pull/1106) (thanks @dawedawe!)
* [Make async more pervasive in the codebase and use AsyncAdaptive values in the Adaptive LSP](https://github.com/fsharp/FsAutoComplete/pull/1088) (thanks @theangrybyrd!)


## [0.59.5] - 2023-04-21

### Added

* [A new codefix to add the 'private' access modifier to bindings and types](https://github.com/fsharp/fsautocomplete/pull/1089) (thanks @dawedawe!)

### Fixed

* [Make the 'convert to positional DU pattern' codefix work in more cases](https://github.com/fsharp/fsautocomplete/pull/1090) (thanks @dawedawe!)
* [Handle text changes when they are empty in the classic LSP Server](https://github.com/fsharp/fsautocomplete/pull/1100) (thanks @augustfengd!)
* [Detect Expecto's Task-based tests](https://github.com/fsharp/fsautocomplete/pull/1105) (thanks @ratsclub!)

### Changed

* [Update Ionide.ProjInfo to get more logging](https://github.com/fsharp/fsautocomplete/pull/1093) (thanks @theangrybyrd!)
* [Update tooltips and Info Panel documentation](https://github.com/fsharp/fsautocomplete/pull/1099) (thanks @MaximeMangel!)
* [Update the xml doc generation codefix to work in more places](https://github.com/fsharp/fsautocomplete/pull/1106) (thanks @dawedawe!)
* [Make async more pervasive in the codebase and use AsyncAdaptive values in the Adaptive LSP](https://github.com/fsharp/FsAutoComplete/pull/1088) (thanks @theangrybyrd!)

## [0.59.4] - 2023-03-19

### Fixed

* [The Adaptive Server no longer sends errors for `textDocument/documentHighlight` calls that there is no symbol information for](https://github.com/fsharp/FsAutoComplete/pull/1075) (Thanks @haodeon!)

## [0.59.3] - 2023-03-19

### Added

* [A new command called `fsproj/renameFile` for renaming a file in the context of a particular project](https://github.com/fsharp/FsAutoComplete/pull/1075) (thanks @MangelMaxime!)

### Fixed

* [Only add a file once to a given project](https://github.com/fsharp/FsAutoComplete/pull/1076) (Thanks @MangelMaxime!)
* [Reduce memory consumption of the compiler typecheck cache](https://github.com/fsharp/FsAutoComplete/pull/1077) (thanks @theangrybyrd!)
  * To change the amount of typechecks kept by the compiler, change the `FSharp.fsac.cachedTypecheckCount` config value
* [Adaptive server now only reloads specific projects that changed, rather than the entire workspace](https://github.com/fsharp/FsAutoComplete/pull/1079) (Thanks @TheAngryByrd!)
* [Don't trigger the 'Replace prefix with _' codefix on _ matches](https://github.com/fsharp/FsAutoComplete/pull/1083) (Thanks @dawedawe!)
* [Handle `workspace/didChangeConfiguration` requests that don't provide an `FSharp` config property](https://github.com/fsharp/FsAutoComplete/pull/1084) (thanks @razzmatazz!)
* [Some threadpool exhaustion fixes due to blocking threads](https://github.com/fsharp/FsAutoComplete/pull/1080) (Thanks @TheAngyrByrd!)
* [Fix Adaptive server to lazily load projects specified instead of loading all projects in the workspace](https://github.com/fsharp/FsAutoComplete/pull/1082) (Thanks @TheAngryByrd!)

## [0.59.2] - 2023-03-12

### Added

* [A new codefix that converts 'bare' ///-comments to full XML documentation comments](https://github.com/fsharp/fsautocomplete/pull/1068) (thanks @dawedawe!)

### Changed

* [Enhancements to Find All References and Rename operations](https://github.com/fsharp/fsautocomplete/pull/1037) (thanks @BooksBaum and @theangrybyrd!)
* [Internal errors no longer report as LSP protocol errors](https://github.com/fsharp/fsautocomplete/pull/1069)
* [TestAdapterEntry items now include module information as well](https://github.com/fsharp/fsautocomplete/pull/1071) (thanks @kojo12228!)

### Fixed

* [IndexOutOfRange issue in signatureHelp](https://github.com/fsharp/fsautocomplete/pull/1067) (thanks @vain0x!)
* [ThreadPool exhaustion issue with ProgressListener](https://github.com/fsharp/fsautocomplete/pull/1070) (thanks @theangrybyrd!)
* [The 'convert positional DU usage to named patterns' codefix now works with multiple match clauses in the same pattern](https://github.com/fsharp/fsautocomplete/pull/1073) (thanks @dawedawe!)

## [0.59.1] - 2023-02-26

### Added

* [Support for logging traces to a configured OpenTelemetry collector endpoint](https://github.com/fsharp/fsautocomplete/pull/1060) (thanks @theangrybyrd!)
  * to use this, set the CLI argument `--otel-exporter-enabled`, and set the `OTEL_EXPORTER_OTLP_ENDPOINT` the the URL of a reachable collector before launching the app
  * then, send the `fsharp.notifications.trace` configuration set to `true`, and the  fsharp.notifications.traceNamespaces` configuration set to an array of string patterns for namespaces of activities to match on.

### Changed

* [Updated the built-in Fantomas client to 0.9.0](https://github.com/fsharp/fsautocomplete/pull/1043) (thanks @nojaf!)
* Brought tooltips using signatures into line with the design guidelines (thanks @dawedawe!)
  * https://github.com/fsharp/fsautocomplete/pull/1061
  * https://github.com/fsharp/fsautocomplete/pull/1063
  * https://github.com/fsharp/fsautocomplete/pull/1064
* Flow through the `InlineValuesProvider` configuration as expected, to light up LSP support for inline values.

## [0.59.0] - 2023-02-20

### Added

* [Support for F# 7](https://github.com/fsharp/fsautocomplete/pull/1043)
  * Note that as a side effect of this, some codefixes have been temporarily disabled. We'll be working on re-enabling them in the near future in conjuntions with the F# team.

### Changed

* [`fsharp/piplineHint` is now powered by the LSP InlineValues functionality](https://github.com/fsharp/fsautocomplete/pull/1042) (thanks @kaashyapan!)
* [Test fixes and enhancements for Adaptive mode](https://github.com/fsharp/fsautocomplete/pull/1053) (thanks @theangrybyrd!)

## [0.58.5] - 2023-02-04

### Added

* Add textDocument/inlineValue from LSP 3.17
* InlineValue config option to shadow PipelineHint config option
* Fix inlayHints for typed params #1046

## [0.58.4] - 2023-02-04

### Added

* Fix crash due to missing dependency on Microsoft.Extensions.Caching.Memory

## [0.58.3] - 2023-02-04

### Changed

* [Speed, typechecking, memory usage improvements for Adaptive and normal LSP servers](https://github.com/fsharp/FsAutoComplete/pull/1036)
* [Don't compute all references unnecessarily](https://github.com/fsharp/FsAutoComplete/pull/1052)

### Removed

* The `FSharp.enableReferenceCodeLens` is deprecated, it's been replaced by the `FSharp.codeLenses.references.enabled` setting.

## [0.58.2] - 2022-11-07

### Fixed

- [Fix reference resolution when there are refassemblies involved](https://github.com/fsharp/FsAutoComplete/pull/1038) (thanks @theangrybyrd!)

## [0.58.1] - 2022-10-19

### Fixed

* [Fix tooltips for some member accesses](https://github.com/fsharp/fsautocomplete/pull/1023) (thanks @theangrybyrd!)
* [Performance enhancements for AdaptiveLSPServer and file time fixes for both servers](https://github.com/fsharp/fsautocomplete/pull/1024) (thanks @theangrybyrd!)
* [Safer directory traversal when probing for projects](https://github.com/fsharp/fsautocomplete/pull/1023) (thanks @sheridanchris!)
* [Clear diagnostics and stale project options for removed files](https://github.com/fsharp/fsautocomplete/pull/1005) (thanks @MangelMaxime!)

### Removed

* [Remove the now-obsolete --background-service-enabled option](https://github.com/fsharp/fsautocomplete/pull/952)

## [0.58.0] - 2022-10-09

### Added

* [Experimental implementation of the LSP server based on FSharp.Data.Adaptive](https://github.com/fsharp/FsAutoComplete/pull/1007). It can be enabled by passing `--adaptive-lsp-server-enabled` on the CLI. (Thanks @TheAngryByrd!)

## [0.57.4] - 2022-09-30

### Fixed

* [Update LSP library so Code Actions calls don't crash anymore](https://github.com/fsharp/FsAutoComplete/pull/1018)

## [0.57.3] - 2022-09-27

### Fixed

* [Don't let codeFixes bring down the application](https://github.com/fsharp/FsAutoComplete/pull/1016)

### Changed

* [Renamed fantomas-tool settings to fantomas](https://github.com/fsharp/FsAutoComplete/pull/1012) (Thanks @nojaf!)

## [0.57.2] - 2022-09-24

### Fixed

* [Fix request cancellation causing crashes for emacs clients](https://github.com/fsharp/FsAutoComplete/pull/1013)

## [0.57.1] - 2022-09-22

### Changed

* [Remove diagnostics for files that aren't in the workspace when they are closed](https://github.com/fsharp/FsAutoComplete/pull/1010) (thanks @Booksbaum!)
* [Improve performance/concurrency for checking files](https://github.com/fsharp/FsAutoComplete/pull/1008) (thanks @Booksbaum!)

## [0.57.0] - 2022-09-05

### Added

* [Add `fsharp/addExistingFile` LSP command](https://github.com/fsharp/FsAutoComplete/pull/1002) (Thanks @MangelMaxime!)
* Support for .NET SDK 6.0.400 and 7.0.100
  * Primary change was updating the Ionide.ProjInfo dependency

### Changed

* [Improvements/Fixes for unused declarations](https://github.com/fsharp/FsAutoComplete/pull/998) (thanks @Booksbaum!)
  * Detect more cases when values are unused
  * Fixes for associated codefix to remove or ignore the value
* [Support removing files that are outside the fsproj directory](https://github.com/fsharp/FsAutoComplete/pull/1001) (thanks @MangelMaxime!)
* Reverted back to full-text synchronization from incremental sync
  * This didn't play well with the debounced checking that we do, so we need to rethink the interaction between the features

## [0.56.2] - 2022-08-19

### Changed

* [Use incremental text sync instead of full text sync](https://github.com/fsharp/FsAutoComplete/pull/981)
* [Nicer errors when CodeLenses cannot be resolved](https://github.com/fsharp/FsAutoComplete/pull/989)
* [Removed compiler-generated and hidden types from the documentation endpoints](https://github.com/fsharp/FsAutoComplete/pull/992) (thanks @MangelMaxime!)

### Added

* [Keywords should work in tooltips consistently now](https://github.com/fsharp/FsAutoComplete/pull/982)
* [Prevent codelenses from getting out of sync with document source](https://github.com/fsharp/FsAutoComplete/pull/987)
* [Added a new command fsharp/removeFile for removing a file from a project](https://github.com/fsharp/FsAutoComplete/pull/990)

### Fixed

* [Clear stale errors when renaming a file](https://github.com/fsharp/FsAutoComplete/pull/973)
* [Respect disabling in-memory project references](https://github.com/fsharp/FsAutoComplete/pull/974)
* [Remove unused formatting from completion and signature items](https://github.com/fsharp/FsAutoComplete/pull/979)
* [Performance regresssions in typechecking files](https://github.com/fsharp/FsAutoComplete/pull/977)
* [Better precondition checking for adding new files to projects](https://github.com/fsharp/FsAutoComplete/pull/991) (thanks @MangelMaxime!)
* [support CodeLenses for single-character identifiers](https://github.com/fsharp/FsAutoComplete/pull/994)

## [0.56.0] - 2022-07-23

### Added

* [Format range provider](https://github.com/fsharp/FsAutoComplete/pull/969)
* [Info toolip for inlay hints](https://github.com/fsharp/FsAutoComplete/pull/972)
* [Rename: Add backticks to name if necessary](https://github.com/fsharp/FsAutoComplete/pull/970) (thanks @Booksbaum!)

### Fixed

* [Disable cross-project typechecking on every edit](https://github.com/fsharp/FsAutoComplete/pull/971)


## [0.55.0] - 2022-07-12

### Added

* [Support for LSP 3.17 InlayHints](https://github.com/fsharp/FsAutoComplete/pull/943)

### Fixed

* [Codelens for -1 reference no longer shown](https://github.com/fsharp/FsAutoComplete/pull/965)
* [Remove backticks for signatures in signature help](https://github.com/fsharp/FsAutoComplete/pull/964)
* [Tons of bugs and enhancements to InlayHints](https://github.com/fsharp/FsAutoComplete/pull/957) (thanks @Booksbaum!)
* [Renames and reference counts are more accurate](https://github.com/fsharp/FsAutoComplete/pull/945)
* [Fix index out of bounds in signature helpers](https://github.com/fsharp/FsAutoComplete/pull/956) (thanks @Booksbaum!)

### Changed

* [Use the parent dotnet binary to set the toolspath](https://github.com/fsharp/FsAutoComplete/pull/958)

### Removed

* [FAKE Integration](https://github.com/fsharp/FsAutoComplete/pull/961)
* Custom InlayHints - (fsharp/inlayHints, removed in favor of LSP inlayHints)

## [0.54.0] - 2022-05-29

### Fixed

* [IndexOutOfBounds exceptions that took down the process](https://github.com/fsharp/FsAutoComplete/pull/938) (thanks @BooksBaum!)

### Changed

* [Update Ionide.LanguageServerProtocol to get new types and fixes](https://github.com/fsharp/FsAutoComplete/pull/948) (thanks @BooksBaum!)
* [Enable several features to be used with untitled/unsaved files](https://github.com/fsharp/FsAutoComplete/pull/949) (thanks @BooksBaum!)
  * Shift+F1 help, Info Panel, Pipeline Hints, and Line Lens all work now for unsaved/untitled loose files
  * This required an API change to the `fsharp/fileParsed` notification - it now returns a URI instead of a string
  * This required an API change to the `fsharp/pipelineHint` request - is is now `{ TextDocument: TextDocumentIdentifier }`

## [0.53.2] - 2022-05-13

### Added
* [Update Fantomas.Client to use new fantomas alpha if present](https://github.com/fsharp/FsAutoComplete/pull/938) (thanks @nojaf!)

## [0.53.1] - 2022-05-01

### Changed

* [Alter logic for showing inlay hints to show fewer hints on parameters](https://github.com/fsharp/FsAutoComplete/pull/9350) (thanks @Booksbaum!)

## [0.53.0] - 2022-04-29

### Added

* [New Codefix: rename parameter to match signature file](https://github.com/fsharp/FsAutoComplete/pull/917) (thanks @Booksbaum!)
* [Config toggles for both kinds of code lenses](https://github.com/fsharp/FsAutoComplete/pull/931)

### Changed

* [Don't trigger inlay hints for typed bindings](https://github.com/fsharp/FsAutoComplete/pull/922)
* [Updated to Ionide.LanguageServerProtocol 0.4.0](https://github.com/fsharp/FsAutoComplete/pull/933)
* [Trigger fewer inlay hints for certain kinds of parameters](https://github.com/fsharp/FsAutoComplete/pull/932)

### Fixed

* Don't hardcode state file to my personal user directory
* [Don't generate state file in an OS-specific way](https://github.com/fsharp/FsAutoComplete/pull/927)
* [ImplementInterface code fix unification and improvements](https://github.com/fsharp/FsAutoComplete/pull/929) (thanks @Booksbaum!)
* [More trigger locations and behavior fixes for the Add Explicit Type to Parameter CodeFix](https://github.com/fsharp/FsAutoComplete/pull/926) (thanks @Booksbaum!)

## [0.52.1] - 2020-04-16

### Changed

* [Updated proj-info to get support for C#/VB projects, as well as .NET SDK workload support](https://github.com/fsharp/FsAutoComplete/pull/920)

## [0.52.0] - 2020-04-14

### Added

* [New notification - `fsharp/testDetected`. This notification is fired per-file when tests are detected for the current file. The data in the payload can be used to run individual tests or groups of tests.](https://github.com/fsharp/FsAutoComplete/pull/893)
* [New endpoint - `fsharp/inlayHints](https://github.com/fsharp/FsAutoComplete/pull/907). This provides support for type annotation and parameter name inlay hints.
* [New codefix - convert erroring single-quoted interpolations to triple-quoted interpolations](https://github.com/fsharp/FsAutoComplete/pull/910)
* [New command-line argument - `--state-directory`. Specified a folder to store workspace-specific FSAC data.](https://github.com/fsharp/FsAutoComplete/pull/913)


### Changed

* [Update to .NET 6](https://github.com/fsharp/FsAutoComplete/pull/903) (Thanks @dsyme!)
* [Update to FCS 41.0.3](https://github.com/fsharp/FsAutoComplete/pull/890)
* [Update to Ionide.ProjInfo 0.58.2 to get fixes around the project loader loop](https://github.com/fsharp/FsAutoComplete/pull/904), [project cache](https://github.com/ionide/proj-info/pull/139), and [legacy project support](https://github.com/ionide/proj-info/pull/131)
* [Completions for types are much better now](https://github.com/fsharp/FsAutoComplete/pull/908) (thanks @tboby!)
* [Completions triggers on the first typed character](https://github.com/fsharp/FsAutoComplete/pull/909) (thanks @tboby!)
* [New CLI Parser with support for auto-completion and nicer help](https://github.com/fsharp/FsAutoComplete/pull/888)

### Fixed

* [Record stub generation works again](https://github.com/fsharp/FsAutoComplete/pull/905)
* The fsautocomplete.netcore.zip file that was previously added to the release announcement on GitHub is back again.
* [Several corner cases around code fixes and many LSP server endpoints](https://github.com/fsharp/FsAutoComplete/pull/911) ([part 2](https://github.com/fsharp/FsAutoComplete/pull/915)) (Thanks @Booksbaum!)

## [0.51.0] - 2022-03-13

### Fixed

* [No longer cause SignatureHelp errors due to errors in text navigation](https://github.com/fsharp/FsAutoComplete/pull/894)

### Added

* [New Codefix: Convert positional DU patterns to named patterns](https://github.com/fsharp/FsAutoComplete/pull/895)

## [0.50.1] - 2022-03-12

### Fixed

* [Fix textDocument/publishDiagnostics sometimes not getting sent](https://github.com/fsharp/FsAutoComplete/pull/887) (Thanks @Booksbaum!)
* [Fix completions in the middle of lines](https://github.com/fsharp/FsAutoComplete/pull/892)

## [0.50.0] - 2022-01-23

### Added

* New release process driven by this Changelog

### Changed

* [Update Fantomas.Client to prefer stable versions](https://github.com/fsharp/FsAutoComplete/pull/880) (Thanks @nojaf)
* [Moved to use the Ionide.LanguageServerProtocol shared nuget package](https://github.com/fsharp/FsAutoComplete/pull/875)

### Fixed
* [Sourcelink's go-to-definition works better on windows for deterministic paths](https://github.com/fsharp/FsAutoComplete/pull/878)
* [Fix missing commas in Info Panel generic type signatures](https://github.com/fsharp/FsAutoComplete/pull/870) (Thanks @jcmrva!)
* [Fix off-by-1 error in the negation-to-subtraction codefix](https://github.com/fsharp/FsAutoComplete/pull/882) (Thanks @jasiozet!)

## [0.49.5] - 2021-12-01

### Added

## [0.49.4] - 2021-11-20

### Added
* BUGFIX: [Fix background service](https://github.com/fsharp/FsAutoComplete/pull/858)
* BUGFIX: [Fix File System](https://github.com/fsharp/FsAutoComplete/pull/860)

## [0.49.3] - 2021-11-19

### Added
* ENHANCEMENT: [Better handling of file typechecking after FCS 40 update](https://github.com/fsharp/FsAutoComplete/pull/857)
* BUGFIX: [Fix regression in cross-project support after FCS 40 update in proj-info](https://github.com/fsharp/FsAutoComplete/pull/857)

## [0.49.2] - 2021-11-16

### Added
* BUGFIX: [Fix probing for dotnet binary locations in the dotnet tool](https://github.com/fsharp/FsAutoComplete/pull/854)

## [0.49.1] - 2021-11-14

### Added
* BUGFIX: [Fix stuck code lenses](https://github.com/fsharp/FsAutoComplete/pull/852) (thanks @beauvankirk!)

## [0.49.0] - 2021-10-29

### Added
* FEATURE: [Support .Net 6 and F# 6](https://github.com/fsharp/FsAutoComplete/pull/846)

## [0.48.2] - 2021-10-27

### Added
* BUGFIX: [Fix Fantomas.Client reference in the fsautocomplete dotnet tool](https://github.com/fsharp/FsAutoComplete/pull/844)

## [0.48.1] - 2021-10-24

### Added
* BUGFIX: [Bump Fantomas.Client to 0.3.1](https://github.com/fsharp/FsAutoComplete/pull/842) (thanks @nojaf!)

## [0.48.0] - 2021-10-23

### Added
* BUGFIX: [update handling of langword and crefs in see xmldoc nodes](https://github.com/fsharp/FsAutoComplete/pull/838)
* BUGFIX: [handle href elements on a, see, and xref xml doc comments](https://github.com/fsharp/FsAutoComplete/pull/839)
* FEATURE: [Use user's managed Fantomas dotnet tool instead of embedding directly into FSAC](https://github.com/fsharp/FsAutoComplete/pull/836) (thanks @nojaf!)

## [0.47.2] - 2021-09-09

### Added
* BUGFIX: [Fix dotnet template rendering on non-english locales](https://github.com/fsharp/FsAutoComplete/pull/826) (thanks @jmiven)
* ENHANCEMENT: [Don't provide completions or tooltips for string literals of all kinds](https://github.com/fsharp/FsAutoComplete/pull/830)
This allows for other extensions to provide completions/hover tooltips for these strings when configured to do so

## [0.47.1] - 2021-08-04

### Added
* BUGFIX: [Handle exceptions from fantomas a bit more safely](https://github.com/fsharp/FsAutoComplete/pull/823)

## [0.47.0] - 2021-07-25

### Added
* BUGFIX: [Fix loading of dotnet new templates](https://github.com/fsharp/FsAutoComplete/pull/815) (thanks @Happypig375)
* BUGFIX: [Fix datatype for workspace/applyEdit request](https://github.com/fsharp/FsAutoComplete/pull/816)
* ENHANCEMENT: [Update Fantomas to 4.5.0 stable](https://github.com/fsharp/FsAutoComplete/pull/813) (thanks @nojaf)
* ENHANCEMENT: [Enable running on .net 6 via rollForward](https://github.com/fsharp/FsAutoComplete/pull/818)
NOTE: if you have both 5.0 and 6.0 SDKs installed, you _must_ launch fsautocomplete by passing the `--fx-version` argument to the dotnet CLI. See [the cli docs](https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet) for more details.

## [0.46.7] - 2021-06-29

### Added
* ENHANCEMENT: [Make the RemoveUnusedBinding codefix work for parameters as well as nested bindings](https://github.com/fsharp/FsAutoComplete/pull/812)

## [0.46.6] - 2021-06-27

### Added
* ENHANCEMENT: [Make the Unused Value analyzer suggest single-underscore discards](https://github.com/fsharp/FsAutoComplete/pull/795) (thanks @pblasucci)
* ENHANCEMENT: [Add new 'Add explicit type annotation' codefix](https://github.com/fsharp/FsAutoComplete/pull/807)
This works for parameters, but not function-typed parameters
* BUGFIX: [Align with LSP protocol around command fields](https://github.com/fsharp/FsAutoComplete/commit/a3f5564ea579767f40cf673595db1efbcf755d85)
Fixes an issue in Ionide-vim (thanks @cannorin)

## [0.46.5] - 2021-06-21

### Added
* ENHANCEMENT: [Add diagnostic code links to Compiler, Linter, and Analyzer diagnostics](https://github.com/fsharp/FsAutoComplete/pull/804)

## [0.46.4] - 2021-06-18

### Added
* ENHANCEMENT: [Reenable FSharpLint linting](https://github.com/fsharp/FsAutoComplete/pull/799)

## [0.46.3] - 2021-06-17

### Added
* ENHANCEMENT: [Update Fantomas dependency to latest prerelease](https://github.com/fsharp/FsAutoComplete/pull/798)

## [0.46.2] - 2021-06-13

### Added
* BUGFIX: fix the dotnet tool packaging to include a missing dependency for code formatting
* BUGFIX: [fix indentation and insert position for unopened namespaces](https://github.com/fsharp/FsAutoComplete/pull/788) (Thanks @Booksbaum)
* ENHANCEMENT: [Render parameters that are functions with parens for readability](https://github.com/fsharp/FsAutoComplete/pull/785)

## [0.46.1] - 2021-06-09

### Added
* Publish the dotnet tool fsautocomplete to nuget. It can be installed with `dotnet tool install fsautocomplete`.

## [0.46.0] - 2021-05-15

### Added
* [Improve memory usage by reducing string array allocations](https://github.com/fsharp/FsAutoComplete/pull/777)
* [Fix fsharp/signature off-by-ones](https://github.com/fsharp/FsAutoComplete/pull/782) (Thanks @mhoogendoorn)
* [Fix analyzer usage](https://github.com/fsharp/FsAutoComplete/pull/783)
* [Add new codefixes](https://github.com/fsharp/FsAutoComplete/pull/784)
* Add missing self-identifier to instance member
* Refactor `typeof<'t>.Name` to `nameof('t)`

## [0.45.4] - 2021-04-30

### Added
* Fix returned tokens in `textDocument/semanticTokens/full` and `textDocument/semanticTokens/range` to no longer return zero-length tokens.

## [0.45.3] - 2021-04-23

### Added
* Improve edgecase detection when
  * finding declarations
  * finding type definitions
  * getting symbol usages
  * checking for inclusion in a file

## [0.45.2] - 2021-04-18

### Added
* Improve overload detection in `textDocument/signatureHelp` for methods

## [0.45.1] - 2021-04-18

### Added
* Fix regression in `textDocument/completion` introduced in 0.45.0

## [0.45.0] - 2021-04-17

### Added
* Update Unused Binding CodeFix to handle more cases
* Enable faster typechecking when signature files are present for a module
  * Happens transparently, but is mutually exclusive with analyzers.
* Refactors around tooltip signature generation
* Fix the display of units of measure in tooltips (`float<m/s>` instead of `float<MeasureInverse<MeasureProduct<.....>>>`)
* Much better experience for signature help for function applications and method calls
* Update the Generate Abstract Class CodeFix to work for abstract classes that aren't defined in F#

## [0.44.0] - 2021-03-15

### Added
* Update to Ionide.ProjInfo 0.51 to prevent workspace init deadlocks

## [0.43.0] - 2021-03-15

### Added
* Fantomas updated to 4.4 stable
* FCS 39 update
* More codefixes!
* Fixed serialization of the FormattingOptions type to prevent server crashes
* Performance enhancements for the BackgroundService

## [0.42.0] - 2021-02-03

### Added
* Many large changes, .Net 5 is required now
* Support for LSP semantic highlighting
* Fantomas upgrade to 4.4.0-beta-003
* FCS 38.0.2 upgrade
* Use Ionide.ProjInfo for the project system instead of the oen built into this repo
* Use local hosted msbuild to crack projects instead of managing builds ourselves

## [0.41.1] - 2020-03-23

### Added
* Fix `PublishDiagnosticsCapabilities` type [#574](https://github.com/fsharp/FsAutoComplete/pull/574) by [@Gastove](https://github.com/Gastove)
* Set defaultDotNetSDKRoot on Linux correctly [#576](https://github.com/fsharp/FsAutoComplete/pull/576) by [@Krzysztof-Cieslak](https://github.com/Krzysztof-Cieslak)

## [0.41.0] - 2020-03-10

### Added
* Rework documentation parser [#446](https://github.com/fsharp/FsAutoComplete/issues/446) by [@MangelMaxime](https://github.com/MangelMaxime)
* Update FAKE integration [#566](https://github.com/fsharp/FsAutoComplete/issues/566) by [@baronfel](https://github.com/baronfel)
* Update FSharp.Analyzers.SDK to 0.4 [#568](https://github.com/fsharp/FsAutoComplete/issues/568) by [@baronfel](https://github.com/baronfel)

## [0.40.1] - 2020-02-28

### Added
* Update to FCS 34.1 ( + all other deps) [#552](https://github.com/fsharp/FsAutoComplete/issues/556) by [@baronfel](https://github.com/baronfel)

## [0.40.0] - 2020-02-19

### Added
* Move Fantomas formatting to Core project [#553](https://github.com/fsharp/FsAutoComplete/issues/553) by [@Krzysztof-Cieslak](https://github.com/Krzysztof-Cieslak)
* Fix return type in signatures in documentation formatter [#554](https://github.com/fsharp/FsAutoComplete/issues/554) by [@Krzysztof-Cieslak](https://github.com/Krzysztof-Cieslak)
* Work around build infrastructure by [@Krzysztof-Cieslak](https://github.com/Krzysztof-Cieslak)
* Allows analyzer paths to be absolute [#555](https://github.com/fsharp/FsAutoComplete/issues/555) by [@Zaid-Ajaj](https://github.com/Zaid-Ajaj)
* Update FSI references version-finding algorithm to probe packs dir as well as runtimes dir [#556](https://github.com/fsharp/FsAutoComplete/issues/556) by [@baronfel](https://github.com/baronfel)
* Update FSharp.Analyzers.SDK to 0.3.0 and make them available only in .Net Core build [#557](https://github.com/fsharp/FsAutoComplete/issues/557) by [@Krzysztof-Cieslak](https://github.com/Krzysztof-Cieslak)

## [0.39.0]

### Added

## [0.38.2]

### Added

## [0.38.1] - 2019-04-16

### Added
* fix packaging of zip releases [#373](https://github.com/fsharp/FsAutoComplete/issues/373) by [@TOTBWF](https://github.com/TOTBWF)

## [0.38.0] - 2019-04-10

### Added
* upgrade to `FSharp.Compiler.Service` v28.0.0
* upgrade to `FSharpLint.Core` v0.10.8
* include symbolcache `runtimeconfig.json` and `deps.json` to .net core binaries
* add `default.win32manifest` to .net core binaries
* fix to allow run with only .NET Core Runtime 3 installed (previously v2.x was required) [#364](https://github.com/fsharp/FsAutoComplete/issues/364)
* add go-to-implementation command (`symbolimplementation`)

## [0.37.0] - 2019-02-28

### Added
* upgrade to `FSharp.Compiler.Service` v27.0.1
* upgrade to `FSharpLint.Core` v0.10.7

## [0.36.0] - 2019-02-20

### Added
* upgrade to `FSharp.Compiler.Service` v26.0.1 (#338)
* upgrade to `FSharpLint.Core` v0.10.5

## [0.35.0] - 2019-02-19

### Added
* new project parser for old fsproj/fsx based on `Dotnet.ProjInfo`, enabled by default in .NET Core FSAC
* add unused declarations diagnostic
* add simplified names analyzer
* add unused opens analyzer
* styling for XmlDocs and tooltips
* add find type declaration command
* adds autocomplete for external (from unopened namespaces and modules) symbols, provides information where and what `open` statements should be inserted
* add workspaceLoad async command
* add notifications (project loading, etc). In http mode, using websocket
* add generic parameters to tooltips
* include keywords in autocomplete only when needed
* don't autocomplete for comments, strings etc
* add project cache
* watch file changes, to trigger project reloading
* implement record stub generator (#297)
* add background and persistent symbol cache out of process
* use dnspy libs to navigate to decompiled files for external libs (#299)
* fsac .NET runs as 64 bit exe
* add description for '=' symbol (#312)
* fix autocomplete for literal values (#316)
* support keywords in helptext command
* add interface stub generator (#327)
* support `FSharp.Analyzers.SDK` analyzer
* upgrade to `FSharp.Compiler.Service` v25.0.1
* upgrade to `Dotnet.ProjInfo` v0.31.0
* upgrade to `FSharpLint.Core` v0.10.4

## [0.34.0] - 2017-09-13

### Added
* support mixed dotnet langs projects (#173)
* add detailed errors info (#175)
* add hostPID command line arg (#190)
* add workspace peek command (#191)
* fix ci, .net core 2.0 RTM, normalize fsprojs (#197)
* fix linter crash (#206)
* single console app (#212)

## [0.33.0] - 2017-06-13

### Added
* add sdk 2.0 support (#166)

## [0.32.0] - 2017-04-16

### Added
* .NET Core project support

## [0.31.1] - 2017-02-07

### Added
* Allow for inconsistent casing of "Fsharp" when detecting: #149.

## [0.31.0] - 2017-01-27

### Added
* Improvements from downstream ionide fork:
  - support msbuild15, same as preview2
  - Add Background checking
  - Performance updates for find usages
  - Implement GetNamespaceSuggestions
  - Update FSharpLint version
  - Optimize GetNamespaceSuggestions
  - Optimize GetDeclarations
  - Add endpoint for F1 Help
  - ... and more!
* (Some of these features only exposed currently via HTTP interface)

## [0.30.2] - 2016-10-31

### Added
* Add parse errors, tooltips for keywords, and signatures for constructors: #135.

## [0.30.1] - 2016-10-31

### Added
* Invalid release, ignore.

## [0.30.0] - 2016-10-30

### Added
* Add EnclosingEntity and IsAbstract to Declaration contract: #129.
* Merge Ionide changes (#123):
  - Glyphs
  - Update dependencies
  - Lint settings
  - Keyword completion

## [0.29.0] - 2016-07-12

### Added
* Add command for all declarations in known projects: #117.
* cache ProjectResponse, invalidate it if project file's last write time changed: #116.
* Add command to parse all known projects: #115.
* Merge Ionide changes (#110):
  - Naive support for project.json (this probably will be dropped in futture but let's have it now)
  - Better (file) paths normalization across different features
  - Resolve scripts to latest .Net on Windows
  - Make completion faster on Suave
  - Depend on F# 4 (FSharp.Core deployed with application) instead of 4.3.1
* Fix Symboluseproject: #104.

## [0.28.0] - 2016-04-05

### Added
* Backwards-incompatible: Make completions faster by not requiring a parse on each request: #99
* Add `SymbolUseProject` command: #101.
* Add typesig command, that doesn't get Comment data: #73
* Add extraction of xmldoc from other assemblies (from .xml files).

## [0.27.4] - 2016-02-18

### Added
* Normalize paths to source files from projects: #94.

## [0.27.3] - 2016-02-08

### Added
* Set MinThreads to avoid deadlocks on Mono < 4.2.2: #92.

## [0.27.2] - 2016-02-05

### Added
* Upgrade to FCS 2.0.0.4 to fix project cracking with spaces in paths: #91.

## [0.27.1] - 2016-01-26

### Added
* Upgrade to FCS 2.0.0.3 to fix VS2015 project cracking: #89.

## [0.27.0] - 2015-12-08

### Added
* Upgrade to FCS 2.0.0.0-beta and add project cracking verbosity option: #86.
* Add FSharpLint support: #83.

## [0.26.1] - 2015-10-23

### Added
* Switch to depend on FSharp.Core 4.3.1.0: #81.
* Don't output a BOM to standard out: #82

## [0.26.0] - 2015-10-20

### Added
* Fix for uncompiled referenced projects: #78.
* Backwards-incompatible: Framework no longer returned in `project` response.

## [0.25.1] - 2015-10-16

### Added
* Add App.config to FsAutoComplete.Suave release: #76.
* Also for fsautocomplete.exe.

## [0.25.0] - 2015-10-14

### Added
* Add Suave hosting for FSAC: #74.
* Backwards-incompatible: return GlyphName rather than code in
* Declarations message: #75.

## [0.24.1] - 2015-09-11

### Added
* Fix StackOverflowException and encoding issue: #70.

## [0.24.0] - 2015-09-04

### Added
* Backwards-incompatible: do not format help text, leave that to the client, which allows the display to be more semantic. #63 (due to @Krzysztof-Cieslak)

## [0.23.1] - 2015-09-02

### Added
* Fix MSBuild v14 support on non-English systems by avoiding attempting to load *.resources.dll (patch from @ryun).

## [0.23.0] - 2015-08-14

### Added
* Add a new `colorizations <true|false>` command to enable/disable asynchronous provision of colorization information following a parse: #60 (Fixes #44).
*  Newest FSharp.Core is used for type-checking scripts and for projects that do not reference FSharp.Core. Supports F# 3.0, 3.1 and 4.0: #59.
*  If MSBuild v12 is not available, instead try load MSBuild v14. This, together with the previous point, adds support for VS2015-only Windows installs: #57. Fixes: #12 #21 #23 #25 #54.
*  Backwards-incompatible: `compilerlocation` command has changed. Now provides path to best version of msbuild, fsc and fsi on Windows: #23.

## [0.22.0] - 2015-08-06

### Added
* Backwards-incompatible: Symbol use command now includes FileName rather than Filename

## [0.21.1] - 2015-08-06

### Added
* Reduce timeout message from 'error' to 'info'

## [0.21.0] - 2015-08-04

### Added
* Update to FCS 1.4.X (support for F# 4.0): #52
* Automatically reparse F# project files if they are changed on disk: #47

## [0.20.1] - 2015-07-30

### Added
* Fix exception in `symboluse` command: #46.

## [0.20.0] - 2015-07-28

### Added
* Backwards-incompatible changes:
  * Update helptext command to return { Name = ""; Text = "" }. Fixes #35.
  * `project` command response now has 'null' for OutputFile and TargetFramework if a value cannot be determined.
* FSharp.CompilerBinding removed, and used parts absorbed. Fixes #17.
* ScriptCheckerOptions fetched with no timeout, and also stores them. Fixes #18, #28.
* If a .fs file is not in a loaded project, produce an incomplete typecheck environment for it to give basic results.
* Update parsing of project options to include ProjectReferences. Fixes #39.
* Separate parsing of commands, main command loop, and formatting of response message into separate modules.

## [0.19.0] - 2015-06-30

### Added
* Add symboluse command - https://github.com/fsharp/FsAutoComplete/pull/34
* Breaking change: all columns returned are now 1-based. Format of error locations has also changed to be more consistent with other formats.
* Add param completion command - https://github.com/fsharp/FsAutoComplete/pull/30

## [0.18.2] - 2015-06-13

### Added
* Update to FCS 0.0.90 (fix referencing PCL projects) - https://github.com/fsharp/FsAutoComplete/pull/26

## [0.18.1] - 2015-06-09

### Added
* Prevent test assemblies from being included in release archives by avoiding forcing the output directory.

## [0.18.0] - 2015-06-03

### Added
* Adjust for 1-based column indexing - https://github.com/fsharp/FSharp.AutoComplete/pull/13
  * Note that this was previously the intended behaviour, but column indexes were treated as 0-based. Ensure that both line and column indexes sent in commands are 1-based.

## [0.17.0] - 2015-05-31

### Added
* Completion filtering - https://github.com/fsharp/FSharp.AutoComplete/pull/10

## [0.16.0] - 2015-05-28

### Added
* Implement multiple unsaved file checking - https://github.com/fsharp/FSharp.AutoComplete/pull/8

## [0.15.0] - 2015-05-20

### Added
* Add Glyphs to completion responses - https://github.com/fsharp/FSharp.AutoComplete/pull/1
