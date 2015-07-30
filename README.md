# FsAutoComplete

This project provides a command-line interface to the [FSharp.Compiler.Service](https://github.com/fsharp/FSharp.Compiler.Service/) project. It is intended to be used as a backend service for rich editing or 'intellisense' features for editors. Currently it is used by:

* [Emacs](https://github.com/fsharp/emacs-fsharp-mode)
* [Vim](https://github.com/fsharp/vim-fsharp)
* [Sublime Text](https://github.com/fsharp/sublime-fsharp-package)
* [Atom](https://github.com/fsprojects/FSharp.Atom)

This README is targeted at developers.

## Build Status

### Travis [![Travis build status](https://travis-ci.org/fsharp/FsAutoComplete.png)](https://travis-ci.org/fsharp/FsAutoComplete)

The configuration is contained in [.travis.yml](.travis.yml).

### AppVeyor [![AppVeyor build status](https://ci.appveyor.com/api/projects/status/0ld2sp0cl25ktiuq?svg=true)](https://ci.appveyor.com/project/rneatherway/fsautocomplete)

The configuration is contained in [appveyor.yml](appveyor.yml).

## Required software

* F# 3.1
* MSBuild 12

### Windows

This can be obtained by installing Visual Studio 2013 or downloading:

* [Visual F# Tools 3.1.2](http://www.microsoft.com/en-us/download/details.aspx?id=44011)
* [Microsoft Built Tools 2013](https://www.microsoft.com/en-us/download/details.aspx?id=40760)

### Mono

* Required: Mono >= 3.10
* Recommended: Mono >= 4.0.2

## Building and testing

There is a [FAKE script](build.fsx) with chain-loaders for [*nix](build.sh) and [Windows](build.cmd). This can be used for both building and running the unit and integration tests. It is also the core of the CI builds running on [Travis](../.travis.yml) and [AppVeyor](../appveyor.yml), and so also has the ability to run the Emacs unit and integration tests.

The [integration tests](FsAutoComplete/test/integration) use a simple strategy of running a scripted session with `fsautocomplete.exe` and then comparing the output with that saved in the repository. This requires careful checking when the test is first constructed. On later runs, absolute paths are removed using regular expressions to ensure that the tests are machine-independent.

There are [unit tests](FSharp.CompilerBinding.Tests) for FSharp.CompilerBinding, which smoothes the integration with [FSharp.Compiler.Service](https://github.com/fsharp/FSharp.Compiler.Service). The tests are simply constructed using NUnit.

## Communication protocol

It is expected that the editor will launch this program in the background and communicate over a pipe. It is possible to use interactively, although due to the lack of any readline support it isn't pleasant, and text pasted into the buffer may not be echoed. As a result, use this only for very simple debugging. For more complex scenarios it is better to write another integration test by copying an [existing one](FsAutoComplete/test/integration/Test1).

The available commands can be listed by running `fsautocomplete.exe --commands`. Commands are all on a single line, with the exception of the `parse` command, which should be followed by the current text of the file to parse (which may differ from the contents on disk), and terminated with a line containing only `<<EOF>>`.

Data is returned as JSON. An example of a simple session is:

    project "Test1.fsproj"
    {"Kind":"project","Data":{"Files":["<absolute path removed>/Program.fs"],"Output":"<absolute path removed>/bin/Debug/Test1.exe"}}
    parse "Program.fs"
    module X =
      let func x = x + 1

    let val2 = X.func 2
    <<EOF>>
    {"Kind":"INFO","Data":"Background parsing started"}
    completion "Program.fs" 4 13
    {"Kind":"completion","Data":["func"]}

Each response is exactly one line, which simplifies the application of a JSON parser. For further insight into the communication protocol, have a look over the integration tests, which have examples of all the features. Each folder contains one or more `*Runner.fsx` files which specify a sequence of commands to send, and `*.json` files, which contain the output.

