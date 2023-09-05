# Creating a New Code Fix

A code fix, also referred to as a quick fix or code action, serves as a mechanism within the editor to propose and implement code changes within the current file. 
This functionality is facilitated through the [Code Action Request](https://microsoft.github.io/language-server-protocol/specifications/lsp/3.17/specification/#textDocument_codeAction) provided by the Language Server Protocol (LSP). 
You might associate code fixes with the concept of "light bulbs" found in certain integrated development environments (IDEs).

To introduce a new code fix within the context of FSAutocomplete, there are several essential components to consider:

1. **Code Fix File**: This pertains to the actual implementation of the code fix.

2. **Registration in LSP Servers**: Registration of the code fix is required in both of the associated LSP servers.

3. **Unit Test Setup**: Proper unit tests need to be established to ensure the correctness and effectiveness of the new code fix.

To streamline the process of creating a new code fix, a convenient `FAKE` target has been provided. By executing the following command:

```bash
dotnet run --project ./build/build.fsproj -- -t ScaffoldCodeFix YourCodeFixName
```

The above command accomplishes the following tasks:

- Generation of three files:
  - The implementation file for your code fix.
  - A signature file associated with your code fix.
  - A dedicated standalone unit test file.

Furthermore, this command updates the following files to properly register the new code fix:

- `src/FsAutoComplete/LspServers/AdaptiveFSharpLspServer.fs`
- `src/FsAutoComplete/LspServers/FsAutoComplete.Lsp.fs`
- `test/FsAutoComplete.Tests.Lsp/CodeFixTests/Tests.fs`

The unit test file contains a single focused test, allowing you to promptly verify the functionality. To run this initial test, you have two options:

1. Using the `dotnet test` command:
   ```bash
   dotnet test -f net6.0 ./test/FsAutoComplete.Tests.Lsp/FsAutoComplete.Tests.Lsp.fsproj
   ```

2. Alternatively, using the `dotnet run` command:
   ```bash
   dotnet run -f net6.0 --project ./test/FsAutoComplete.Tests.Lsp/FsAutoComplete.Tests.Lsp.fsproj
   ```

This comprehensive approach ensures that the newly introduced code fix is properly integrated, tested, and ready for seamless integration into the FSAutocomplete environment.
